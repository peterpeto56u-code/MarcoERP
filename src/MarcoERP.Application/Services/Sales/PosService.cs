using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Sales;
using MarcoERP.Application.Mappers.Sales;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Entities.Accounting.Policies;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Sales;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Inventory;
using MarcoERP.Domain.Interfaces.Sales;

namespace MarcoERP.Application.Services.Sales
{
    /// <summary>
    /// Implements POS operations: session management, atomic sale completion,
    /// cancellation with full reversal, and POS reporting.
    /// 
    /// Reuses existing SalesInvoice entity, JournalEntry auto-posting, and InventoryMovement.
    /// Does NOT duplicate SalesInvoiceService posting logic — it follows the same pattern
    /// but wraps everything in a Serializable transaction for POS atomicity.
    /// 
    /// Revenue Journal:  DR Cash/Bank/AR  /  CR 4111 Sales  /  CR 2121 VAT Output
    /// COGS Journal:     DR 5111 COGS      /  CR 1131 Inventory  (per-line at WAC)
    /// </summary>
    [Module(SystemModule.Sales)]
    public sealed class PosService : IPosService
    {
        private readonly IPosSessionRepository _sessionRepo;
        private readonly IPosPaymentRepository _paymentRepo;
        private readonly ISalesInvoiceRepository _invoiceRepo;
        private readonly IProductRepository _productRepo;
        private readonly IWarehouseProductRepository _whProductRepo;
        private readonly IInventoryMovementRepository _movementRepo;
        private readonly IJournalEntryRepository _journalRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IFiscalYearRepository _fiscalYearRepo;
        private readonly IJournalNumberGenerator _journalNumberGen;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDateTimeProvider _dateTime;
        private readonly IValidator<OpenPosSessionDto> _openSessionValidator;
        private readonly IValidator<ClosePosSessionDto> _closeSessionValidator;
        private readonly IValidator<CompletePoseSaleDto> _completeSaleValidator;

        // ── GL Account Codes (same as SalesInvoiceService) ──────
        private const string CashAccountCode = "1111";    // النقدية — الصندوق الرئيسي
        private const string CardAccountCode = "1112";    // البنك / بطاقات الدفع
        private const string ArAccountCode = "1121";      // المدينون — ذمم تجارية
        private const string SalesAccountCode = "4111";   // المبيعات — عام
        private const string VatOutputAccountCode = "2121"; // ضريبة مخرجات مستحقة
        private const string CogsAccountCode = "5111";    // تكلفة البضاعة المباعة
        private const string InventoryAccountCode = "1131"; // المخزون

        public PosService(
            PosRepositories repos,
            PosServices services,
            PosValidators validators)
        {
            if (repos == null) throw new ArgumentNullException(nameof(repos));
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (validators == null) throw new ArgumentNullException(nameof(validators));

            _sessionRepo = repos.SessionRepo;
            _paymentRepo = repos.PaymentRepo;
            _invoiceRepo = repos.InvoiceRepo;
            _productRepo = repos.ProductRepo;
            _whProductRepo = repos.WhProductRepo;
            _movementRepo = repos.MovementRepo;
            _journalRepo = repos.JournalRepo;
            _accountRepo = repos.AccountRepo;

            _fiscalYearRepo = services.FiscalYearRepo;
            _journalNumberGen = services.JournalNumberGen;
            _unitOfWork = services.UnitOfWork;
            _currentUser = services.CurrentUser;
            _dateTime = services.DateTime;

            _openSessionValidator = validators.OpenSessionValidator;
            _closeSessionValidator = validators.CloseSessionValidator;
            _completeSaleValidator = validators.CompleteSaleValidator;
        }

        // ══════════════════════════════════════════════════════════
        //  SESSION MANAGEMENT
        // ══════════════════════════════════════════════════════════

        public async Task<ServiceResult<PosSessionDto>> OpenSessionAsync(OpenPosSessionDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<PosSessionDto>(_currentUser, PermissionKeys.PosAccess);
            if (authCheck != null) return authCheck;

            var vr = await _openSessionValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<PosSessionDto>.Failure(string.Join(" | ", vr.Errors.Select(e => e.ErrorMessage)));

            var userId = _currentUser.UserId;
            if (userId <= 0)
                return ServiceResult<PosSessionDto>.Failure("لم يتم تحديد المستخدم الحالي.");

            // Check if user already has an open session
            if (await _sessionRepo.HasOpenSessionAsync(userId, ct))
                return ServiceResult<PosSessionDto>.Failure("لديك جلسة مفتوحة بالفعل. أغلقها أولاً.");

            var sessionNumber = await _sessionRepo.GetNextSessionNumberAsync(ct);

            var session = new PosSession(
                sessionNumber,
                userId,
                dto.CashboxId,
                dto.WarehouseId,
                dto.OpeningBalance,
                _dateTime.UtcNow);

            await _sessionRepo.AddAsync(session, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _sessionRepo.GetByIdAsync(session.Id, ct);
            return ServiceResult<PosSessionDto>.Success(PosMapper.ToSessionDto(saved));
        }

        public async Task<ServiceResult<PosSessionDto>> GetCurrentSessionAsync(CancellationToken ct = default)
        {
            var userId = _currentUser.UserId;
            if (userId <= 0)
                return ServiceResult<PosSessionDto>.Failure("لم يتم تحديد المستخدم الحالي.");

            var session = await _sessionRepo.GetOpenSessionByUserAsync(userId, ct);
            if (session == null)
                return ServiceResult<PosSessionDto>.Failure("لا توجد جلسة مفتوحة.");

            return ServiceResult<PosSessionDto>.Success(PosMapper.ToSessionDto(session));
        }

        public async Task<ServiceResult<PosSessionDto>> CloseSessionAsync(ClosePosSessionDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<PosSessionDto>(_currentUser, PermissionKeys.PosAccess);
            if (authCheck != null) return authCheck;

            var vr = await _closeSessionValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<PosSessionDto>.Failure(string.Join(" | ", vr.Errors.Select(e => e.ErrorMessage)));

            var session = await _sessionRepo.GetByIdAsync(dto.SessionId, ct);
            if (session == null)
                return ServiceResult<PosSessionDto>.Failure("الجلسة غير موجودة.");

            if (!session.IsOpen)
                return ServiceResult<PosSessionDto>.Failure("الجلسة مغلقة بالفعل.");

            try
            {
                session.Close(dto.ActualClosingBalance, dto.Notes, _dateTime.UtcNow);
                _sessionRepo.Update(session);
                await _unitOfWork.SaveChangesAsync(ct);

                return ServiceResult<PosSessionDto>.Success(PosMapper.ToSessionDto(session));
            }
            catch (SalesInvoiceDomainException ex)
            {
                return ServiceResult<PosSessionDto>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult<PosSessionDto>> GetSessionByIdAsync(int id, CancellationToken ct = default)
        {
            var session = await _sessionRepo.GetWithPaymentsAsync(id, ct);
            if (session == null)
                return ServiceResult<PosSessionDto>.Failure("الجلسة غير موجودة.");

            return ServiceResult<PosSessionDto>.Success(PosMapper.ToSessionDto(session));
        }

        public async Task<ServiceResult<IReadOnlyList<PosSessionListDto>>> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessions = await _sessionRepo.GetAllAsync(ct);
            return ServiceResult<IReadOnlyList<PosSessionListDto>>.Success(
                sessions.Select(PosMapper.ToSessionListDto).ToList());
        }

        // ══════════════════════════════════════════════════════════
        //  PRODUCT LOOKUP (lightweight, AsNoTracking-style)
        // ══════════════════════════════════════════════════════════

        public async Task<ServiceResult<IReadOnlyList<PosProductLookupDto>>> LoadProductCacheAsync(CancellationToken ct = default)
        {
            var products = await _productRepo.GetAllWithUnitsAsync(ct);
            var result = products
                .Where(p => p.Status == ProductStatus.Active)
                .Select(PosMapper.ToProductLookupDto)
                .ToList();
            return ServiceResult<IReadOnlyList<PosProductLookupDto>>.Success(result);
        }

        public async Task<ServiceResult<PosProductLookupDto>> FindByBarcodeAsync(string barcode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return ServiceResult<PosProductLookupDto>.Failure("الباركود مطلوب.");

            var product = await _productRepo.GetByBarcodeAsync(barcode.Trim(), ct);
            if (product == null)
                return ServiceResult<PosProductLookupDto>.Failure($"لم يتم العثور على صنف بالباركود: {barcode}");

            if (product.Status != ProductStatus.Active)
                return ServiceResult<PosProductLookupDto>.Failure($"الصنف ({product.NameAr}) غير نشط.");

            return ServiceResult<PosProductLookupDto>.Success(PosMapper.ToProductLookupDto(product));
        }

        public async Task<ServiceResult<IReadOnlyList<PosProductLookupDto>>> SearchProductsAsync(string term, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return ServiceResult<IReadOnlyList<PosProductLookupDto>>.Success(new List<PosProductLookupDto>());

            var products = await _productRepo.SearchAsync(term.Trim(), ct);
            var result = products
                .Where(p => p.Status == ProductStatus.Active)
                .Select(PosMapper.ToProductLookupDto)
                .ToList();
            return ServiceResult<IReadOnlyList<PosProductLookupDto>>.Success(result);
        }

        public async Task<ServiceResult<decimal>> GetAvailableStockAsync(int productId, int warehouseId, CancellationToken ct = default)
        {
            var whProduct = await _whProductRepo.GetAsync(warehouseId, productId, ct);
            return ServiceResult<decimal>.Success(whProduct?.Quantity ?? 0);
        }

        // ══════════════════════════════════════════════════════════
        //  COMPLETE SALE — The critical atomic operation
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Completes a POS sale in one Serializable transaction:
        ///   1. Validate fiscal period open
        ///   2. Validate stock for all lines
        ///   3. Create SalesInvoice (Draft) with lines
        ///   4. Generate Revenue Journal (DR Cash/Bank/AR, CR Sales, CR VAT Output)
        ///   5. Generate COGS Journal (DR COGS, CR Inventory)
        ///   6. Deduct stock &amp; create InventoryMovements
        ///   7. Post the invoice
        ///   8. Record POS payments
        ///   9. Update session totals
        ///   10. Commit — full rollback on any failure
        /// </summary>
        public async Task<ServiceResult<SalesInvoiceDto>> CompleteSaleAsync(CompletePoseSaleDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<SalesInvoiceDto>(_currentUser, PermissionKeys.PosAccess);
            if (authCheck != null) return authCheck;

            var vr = await _completeSaleValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<SalesInvoiceDto>.Failure(string.Join(" | ", vr.Errors.Select(e => e.ErrorMessage)));

            var sessionResult = await GetOpenSessionAsync(dto.SessionId, ct);
            if (!sessionResult.IsSuccess)
                return ServiceResult<SalesInvoiceDto>.Failure(sessionResult.ErrorMessage);

            var paymentResult = ParsePayments(dto);
            if (!paymentResult.IsSuccess)
                return ServiceResult<SalesInvoiceDto>.Failure(paymentResult.ErrorMessage);

            try
            {
                var result = await ExecuteCompleteSaleAsync(dto, sessionResult.Data, paymentResult.Data, ct);
                return ServiceResult<SalesInvoiceDto>.Success(result);
            }
            catch (SalesInvoiceDomainException ex)
            {
                return ServiceResult<SalesInvoiceDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<SalesInvoiceDto>.Failure($"خطأ أثناء إتمام عملية البيع: {ex.Message}");
            }
        }

        private async Task<ServiceResult<PosSession>> GetOpenSessionAsync(int sessionId, CancellationToken ct)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
            if (session == null)
                return ServiceResult<PosSession>.Failure("جلسة نقطة البيع غير موجودة.");

            if (!session.IsOpen)
                return ServiceResult<PosSession>.Failure("جلسة نقطة البيع مغلقة.");

            return ServiceResult<PosSession>.Success(session);
        }

        private ServiceResult<PosPaymentBreakdown> ParsePayments(CompletePoseSaleDto dto)
        {
            decimal totalCash = 0;
            decimal totalCard = 0;
            decimal totalOnAccount = 0;
            var parsedPayments = new List<PosParsedPayment>();

            foreach (var p in dto.Payments)
            {
                if (!Enum.TryParse<PaymentMethod>(p.PaymentMethod, true, out var method))
                    return ServiceResult<PosPaymentBreakdown>.Failure($"طريقة الدفع غير صالحة: {p.PaymentMethod}");

                parsedPayments.Add(new PosParsedPayment(method, p.Amount, p.ReferenceNumber));

                switch (method)
                {
                    case PaymentMethod.Cash:
                        totalCash += p.Amount;
                        break;
                    case PaymentMethod.Card:
                        totalCard += p.Amount;
                        break;
                    case PaymentMethod.OnAccount:
                        totalOnAccount += p.Amount;
                        break;
                }
            }

            var customerId = dto.CustomerId ?? 1;
            if (totalOnAccount > 0 && (dto.CustomerId == null || dto.CustomerId <= 0))
                return ServiceResult<PosPaymentBreakdown>.Failure("البيع بالآجل يتطلب تحديد عميل.");

            return ServiceResult<PosPaymentBreakdown>.Success(new PosPaymentBreakdown
            {
                TotalCash = totalCash,
                TotalCard = totalCard,
                TotalOnAccount = totalOnAccount,
                CustomerId = customerId,
                Payments = parsedPayments
            });
        }

        private async Task<SalesInvoiceDto> ExecuteCompleteSaleAsync(
            CompletePoseSaleDto dto,
            PosSession session,
            PosPaymentBreakdown payments,
            CancellationToken ct)
        {
            SalesInvoiceDto result = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var today = _dateTime.Today;
                var now = _dateTime.UtcNow;
                var username = _currentUser.Username ?? "POS";

                var (fiscalYear, period) = await GetPosPostingPeriodAsync(today, ct);

                var lineProducts = await LoadLineProductsAsync(session, dto.Lines, ct);

                var invoice = await CreateDraftInvoiceAsync(dto, session, payments.CustomerId, today, lineProducts, ct);

                EnsurePaymentTotal(payments.TotalPaid, invoice.NetTotal);

                var accounts = await ResolvePosAccountsAsync(ct);

                var journalContext = new PosJournalContext
                {
                    FiscalYear = fiscalYear,
                    Period = period,
                    Now = now,
                    Username = username
                };

                var revenueJournal = await CreateRevenueJournalAsync(
                    invoice,
                    payments,
                    accounts,
                    journalContext,
                    ct);

                var cogsResult = await CreateCogsJournalAsync(
                    invoice,
                    lineProducts,
                    accounts,
                    journalContext,
                    ct);

                await _unitOfWork.SaveChangesAsync(ct);

                await DeductStockAsync(invoice, session, cogsResult.lineCosts, today, ct);

                invoice.Post(revenueJournal.Id, cogsResult.journal.Id);

                // Mark invoice payment based on POS payment methods
                var paidNow = payments.TotalCash + payments.TotalCard;
                if (paidNow > 0)
                    invoice.ApplyPayment(paidNow);

                _invoiceRepo.Update(invoice);

                await RecordPosPaymentsAsync(invoice, session, payments, ct);

                session.RecordSale(invoice.NetTotal, payments.TotalCash, payments.TotalCard, payments.TotalOnAccount);
                _sessionRepo.Update(session);

                await _unitOfWork.SaveChangesAsync(ct);

                var saved = await _invoiceRepo.GetWithLinesAsync(invoice.Id, ct);
                result = SalesInvoiceMapper.ToDto(saved);

            }, IsolationLevel.Serializable, ct);

            return result;
        }

        private async Task<(FiscalYear fiscalYear, FiscalPeriod period)> GetPosPostingPeriodAsync(DateTime today, CancellationToken ct)
        {
            var fiscalYear = await _fiscalYearRepo.GetActiveYearAsync(ct);
            if (fiscalYear == null)
                throw new SalesInvoiceDomainException("لا توجد سنة مالية نشطة.");

            var yearWithPeriods = await _fiscalYearRepo.GetWithPeriodsAsync(fiscalYear.Id, ct);
            var period = yearWithPeriods.GetPeriod(today.Month);
            if (period == null || !period.IsOpen)
                throw new SalesInvoiceDomainException("الفترة المالية مقفلة — لا يمكن إجراء عمليات بيع.");

            return (fiscalYear, period);
        }

        private async Task<Dictionary<int, Product>> LoadLineProductsAsync(
            PosSession session,
            IReadOnlyList<PosSaleLineDto> lines,
            CancellationToken ct)
        {
            var lineProducts = new Dictionary<int, Product>();

            foreach (var line in lines)
            {
                var product = await _productRepo.GetByIdWithUnitsAsync(line.ProductId, ct);
                if (product == null)
                    throw new SalesInvoiceDomainException($"الصنف برقم {line.ProductId} غير موجود.");

                if (product.Status != ProductStatus.Active)
                    throw new SalesInvoiceDomainException($"الصنف ({product.NameAr}) غير نشط.");

                var pu = product.ProductUnits.FirstOrDefault(u => u.UnitId == line.UnitId);
                if (pu == null)
                    throw new SalesInvoiceDomainException($"الوحدة المحددة غير مرتبطة بالصنف ({product.NameAr}).");

                var baseQty = Math.Round(line.Quantity * pu.ConversionFactor, 4);

                var whProduct = await _whProductRepo.GetAsync(session.WarehouseId, line.ProductId, ct);
                if (whProduct == null || whProduct.Quantity < baseQty)
                {
                    var available = whProduct?.Quantity ?? 0;
                    throw new SalesInvoiceDomainException(
                        $"الكمية المتاحة للصنف ({product.NameAr}) = {available:N2} أقل من المطلوب ({baseQty:N2}).");
                }

                lineProducts[line.ProductId] = product;
            }

            return lineProducts;
        }

        private async Task<SalesInvoice> CreateDraftInvoiceAsync(
            CompletePoseSaleDto dto,
            PosSession session,
            int customerId,
            DateTime today,
            IReadOnlyDictionary<int, Product> lineProducts,
            CancellationToken ct)
        {
            var invoiceNumber = await _invoiceRepo.GetNextNumberAsync(ct);

            var invoice = new SalesInvoice(
                invoiceNumber,
                today,
                customerId,
                session.WarehouseId,
                dto.Notes ?? $"POS - جلسة {session.SessionNumber}");

            foreach (var lineDto in dto.Lines)
            {
                var product = lineProducts[lineDto.ProductId];
                var pu = product.ProductUnits.First(u => u.UnitId == lineDto.UnitId);

                invoice.AddLine(
                    lineDto.ProductId,
                    lineDto.UnitId,
                    lineDto.Quantity,
                    lineDto.UnitPrice,
                    pu.ConversionFactor,
                    lineDto.DiscountPercent,
                    product.VatRate);
            }

            await _invoiceRepo.AddAsync(invoice, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return invoice;
        }

        private static void EnsurePaymentTotal(decimal totalPaid, decimal netTotal)
        {
            if (totalPaid < netTotal)
                throw new SalesInvoiceDomainException(
                    $"إجمالي المدفوع ({totalPaid:N2}) أقل من إجمالي الفاتورة ({netTotal:N2}).");
        }

        private async Task<(Account cash, Account card, Account ar, Account sales, Account vat, Account cogs, Account inventory)>
            ResolvePosAccountsAsync(CancellationToken ct)
        {
            var cashAccount = await _accountRepo.GetByCodeAsync(CashAccountCode, ct);
            var cardAccount = await _accountRepo.GetByCodeAsync(CardAccountCode, ct);
            var arAccount = await _accountRepo.GetByCodeAsync(ArAccountCode, ct);
            var salesAccount = await _accountRepo.GetByCodeAsync(SalesAccountCode, ct);
            var vatOutAccount = await _accountRepo.GetByCodeAsync(VatOutputAccountCode, ct);
            var cogsAccount = await _accountRepo.GetByCodeAsync(CogsAccountCode, ct);
            var invAccount = await _accountRepo.GetByCodeAsync(InventoryAccountCode, ct);

            if (cashAccount == null || arAccount == null || salesAccount == null
                || vatOutAccount == null || cogsAccount == null || invAccount == null)
            {
                throw new SalesInvoiceDomainException("حسابات النظام المطلوبة غير موجودة. تأكد من تشغيل Seed.");
            }

            // Card account is optional — falls back to cash if not seeded
            cardAccount ??= cashAccount;

            return (cashAccount, cardAccount, arAccount, salesAccount, vatOutAccount, cogsAccount, invAccount);
        }

        private async Task<JournalEntry> CreateRevenueJournalAsync(
            SalesInvoice invoice,
            PosPaymentBreakdown payments,
            (Account cash, Account card, Account ar, Account sales, Account vat, Account cogs, Account inventory) accounts,
            PosJournalContext context,
            CancellationToken ct)
        {
            var revenueJournal = JournalEntry.CreateDraft(
                invoice.InvoiceDate,
                $"نقطة بيع — فاتورة {invoice.InvoiceNumber}",
                SourceType.SalesInvoice,
                context.FiscalYear.Id,
                context.Period.Id,
                referenceNumber: invoice.InvoiceNumber,
                sourceId: invoice.Id);

            if (payments.TotalCash > 0)
                revenueJournal.AddLine(accounts.cash.Id, payments.TotalCash, 0, context.Now,
                    $"نقدي — POS {invoice.InvoiceNumber}");

            if (payments.TotalCard > 0)
                revenueJournal.AddLine(accounts.card.Id, payments.TotalCard, 0, context.Now,
                    $"بطاقة — POS {invoice.InvoiceNumber}");

            if (payments.TotalOnAccount > 0)
                revenueJournal.AddLine(accounts.ar.Id, payments.TotalOnAccount, 0, context.Now,
                    $"آجل — POS {invoice.InvoiceNumber}");

            var netSalesRevenue = invoice.Subtotal - invoice.DiscountTotal;
            if (netSalesRevenue > 0)
                revenueJournal.AddLine(accounts.sales.Id, 0, netSalesRevenue, context.Now,
                    $"مبيعات — POS {invoice.InvoiceNumber}");

            if (invoice.VatTotal > 0)
                revenueJournal.AddLine(accounts.vat.Id, 0, invoice.VatTotal, context.Now,
                    $"ضريبة مخرجات — POS {invoice.InvoiceNumber}");

            var revenueJournalNumber = _journalNumberGen.NextNumber(context.FiscalYear.Id);
            revenueJournal.Post(revenueJournalNumber, context.Username, context.Now);
            await _journalRepo.AddAsync(revenueJournal, ct);

            return revenueJournal;
        }

        private async Task<(JournalEntry journal, Dictionary<int, decimal> lineCosts)> CreateCogsJournalAsync(
            SalesInvoice invoice,
            IReadOnlyDictionary<int, Product> lineProducts,
            (Account cash, Account card, Account ar, Account sales, Account vat, Account cogs, Account inventory) accounts,
            PosJournalContext context,
            CancellationToken ct)
        {
            var cogsJournal = JournalEntry.CreateDraft(
                invoice.InvoiceDate,
                $"تكلفة بضاعة مباعة — POS {invoice.InvoiceNumber}",
                SourceType.SalesInvoice,
                context.FiscalYear.Id,
                context.Period.Id,
                referenceNumber: invoice.InvoiceNumber,
                sourceId: invoice.Id);

            decimal totalCogs = 0;
            var lineCosts = new Dictionary<int, decimal>();

            foreach (var line in invoice.Lines)
            {
                var product = lineProducts[line.ProductId];
                var costPerBaseUnit = product.WeightedAverageCost;
                var lineCost = Math.Round(line.BaseQuantity * costPerBaseUnit, 4);
                totalCogs += lineCost;
                lineCosts[line.Id] = costPerBaseUnit;
            }

            if (totalCogs > 0)
            {
                cogsJournal.AddLine(accounts.cogs.Id, totalCogs, 0, context.Now,
                    $"تكلفة بضاعة — POS {invoice.InvoiceNumber}");
                cogsJournal.AddLine(accounts.inventory.Id, 0, totalCogs, context.Now,
                    $"مخزون — تكلفة POS {invoice.InvoiceNumber}");
            }

            var cogsJournalNumber = _journalNumberGen.NextNumber(context.FiscalYear.Id);
            cogsJournal.Post(cogsJournalNumber, context.Username, context.Now);
            await _journalRepo.AddAsync(cogsJournal, ct);

            return (cogsJournal, lineCosts);
        }

        private async Task DeductStockAsync(
            SalesInvoice invoice,
            PosSession session,
            IReadOnlyDictionary<int, decimal> lineCosts,
            DateTime today,
            CancellationToken ct)
        {
            foreach (var line in invoice.Lines)
            {
                var whProduct = await _whProductRepo.GetAsync(session.WarehouseId, line.ProductId, ct);
                whProduct.DecreaseStock(line.BaseQuantity);
                _whProductRepo.Update(whProduct);

                var costPerBaseUnit = lineCosts.TryGetValue(line.Id, out var unitCost) ? unitCost : 0;
                var lineCost = Math.Round(line.BaseQuantity * costPerBaseUnit, 4);

                var movement = new InventoryMovement(
                    line.ProductId,
                    session.WarehouseId,
                    line.UnitId,
                    MovementType.SalesOut,
                    line.Quantity,
                    line.BaseQuantity,
                    costPerBaseUnit,
                    lineCost,
                    today,
                    invoice.InvoiceNumber,
                    SourceType.SalesInvoice,
                    sourceId: invoice.Id,
                    notes: $"POS — جلسة {session.SessionNumber}");

                movement.SetBalanceAfter(whProduct.Quantity);
                await _movementRepo.AddAsync(movement, ct);
            }
        }

        private async Task RecordPosPaymentsAsync(
            SalesInvoice invoice,
            PosSession session,
            PosPaymentBreakdown payments,
            CancellationToken ct)
        {
            foreach (var payment in payments.Payments)
            {
                var posPayment = new PosPayment(
                    invoice.Id,
                    session.Id,
                    payment.Method,
                    payment.Amount,
                    _dateTime.UtcNow,
                    payment.Reference);

                await _paymentRepo.AddAsync(posPayment, ct);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  CANCEL SALE
        // ══════════════════════════════════════════════════════════

        public async Task<ServiceResult> CancelSaleAsync(int salesInvoiceId, int sessionId, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.PosAccess);
            if (authCheck != null) return authCheck;

            var invoice = await _invoiceRepo.GetWithLinesAsync(salesInvoiceId, ct);
            if (invoice == null)
                return ServiceResult.Failure("فاتورة البيع غير موجودة.");

            if (invoice.Status != InvoiceStatus.Posted)
                return ServiceResult.Failure("لا يمكن إلغاء إلا الفواتير المرحّلة.");

            var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
            if (session == null)
                return ServiceResult.Failure("جلسة نقطة البيع غير موجودة.");

            if (!session.IsOpen)
                return ServiceResult.Failure("لا يمكن إلغاء فاتورة في جلسة مغلقة.");

            try
            {
                await ExecuteCancelSaleAsync(invoice, session, ct);
                return ServiceResult.Success();
            }
            catch (SalesInvoiceDomainException ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"خطأ أثناء إلغاء عملية البيع: {ex.Message}");
            }
        }

        private async Task ExecuteCancelSaleAsync(SalesInvoice invoice, PosSession session, CancellationToken ct)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var context = await GetCancelContextAsync(ct);

                await ReverseRevenueJournalAsync(invoice, context, ct);
                await ReverseCogsJournalAsync(invoice, context, ct);
                await ReverseStockAsync(invoice, session, context.Today, ct);

                // Reverse any payment allocation before cancelling
                if (invoice.PaidAmount > 0)
                    invoice.ReversePayment(invoice.PaidAmount);

                invoice.Cancel();
                _invoiceRepo.Update(invoice);

                await ReverseSessionTotalsAsync(invoice, session, ct);

                await _unitOfWork.SaveChangesAsync(ct);

            }, IsolationLevel.Serializable, ct);
        }

        private async Task<(FiscalYear FiscalYear, FiscalPeriod Period, DateTime Today, DateTime Now, string Username)>
            GetCancelContextAsync(CancellationToken ct)
        {
            var now = _dateTime.UtcNow;
            var today = _dateTime.Today;
            var username = _currentUser.Username ?? "POS";

            var fiscalYear = await _fiscalYearRepo.GetActiveYearAsync(ct);
            if (fiscalYear == null)
                throw new SalesInvoiceDomainException("لا توجد سنة مالية نشطة.");

            var yearWithPeriods = await _fiscalYearRepo.GetWithPeriodsAsync(fiscalYear.Id, ct);
            var period = yearWithPeriods.GetPeriod(today.Month);
            if (period == null || !period.IsOpen)
                throw new SalesInvoiceDomainException("الفترة المالية مقفلة.");

            return (fiscalYear, period, today, now, username);
        }

        private async Task ReverseRevenueJournalAsync(
            SalesInvoice invoice,
            (FiscalYear FiscalYear, FiscalPeriod Period, DateTime Today, DateTime Now, string Username) context,
            CancellationToken ct)
        {
            if (!invoice.JournalEntryId.HasValue)
                return;

            var revenueJournal = await _journalRepo.GetWithLinesAsync(invoice.JournalEntryId.Value, ct);
            if (revenueJournal == null)
                return;

            var reversalRevenue = revenueJournal.CreateReversal(
                context.Today,
                $"إلغاء POS — فاتورة {invoice.InvoiceNumber}",
                context.FiscalYear.Id,
                context.Period.Id);

            var revNumber = _journalNumberGen.NextNumber(context.FiscalYear.Id);
            reversalRevenue.Post(revNumber, context.Username, context.Now);
            await _journalRepo.AddAsync(reversalRevenue, ct);

            revenueJournal.MarkAsReversed(reversalRevenue.Id);
            _journalRepo.Update(revenueJournal);
        }

        private async Task ReverseCogsJournalAsync(
            SalesInvoice invoice,
            (FiscalYear FiscalYear, FiscalPeriod Period, DateTime Today, DateTime Now, string Username) context,
            CancellationToken ct)
        {
            if (!invoice.CogsJournalEntryId.HasValue)
                return;

            var cogsJournal = await _journalRepo.GetWithLinesAsync(invoice.CogsJournalEntryId.Value, ct);
            if (cogsJournal == null)
                return;

            var reversalCogs = cogsJournal.CreateReversal(
                context.Today,
                $"إلغاء تكلفة POS — فاتورة {invoice.InvoiceNumber}",
                context.FiscalYear.Id,
                context.Period.Id);

            var cogsRevNumber = _journalNumberGen.NextNumber(context.FiscalYear.Id);
            reversalCogs.Post(cogsRevNumber, context.Username, context.Now);
            await _journalRepo.AddAsync(reversalCogs, ct);

            cogsJournal.MarkAsReversed(reversalCogs.Id);
            _journalRepo.Update(cogsJournal);
        }

        private async Task ReverseStockAsync(
            SalesInvoice invoice,
            PosSession session,
            DateTime today,
            CancellationToken ct)
        {
            foreach (var line in invoice.Lines)
            {
                var whProduct = await _whProductRepo.GetOrCreateAsync(
                    invoice.WarehouseId, line.ProductId, ct);

                whProduct.IncreaseStock(line.BaseQuantity);
                _whProductRepo.Update(whProduct);

                var product = await _productRepo.GetByIdWithUnitsAsync(line.ProductId, ct);
                var costPerBaseUnit = product.WeightedAverageCost;
                var lineCost = Math.Round(line.BaseQuantity * costPerBaseUnit, 4);

                var movement = new InventoryMovement(
                    line.ProductId,
                    invoice.WarehouseId,
                    line.UnitId,
                    MovementType.SalesReturn,
                    line.Quantity,
                    line.BaseQuantity,
                    costPerBaseUnit,
                    lineCost,
                    today,
                    invoice.InvoiceNumber,
                    SourceType.SalesReturn,
                    sourceId: invoice.Id,
                    notes: $"إلغاء POS — جلسة {session.SessionNumber}");

                movement.SetBalanceAfter(whProduct.Quantity);
                await _movementRepo.AddAsync(movement, ct);
            }
        }

        private async Task ReverseSessionTotalsAsync(SalesInvoice invoice, PosSession session, CancellationToken ct)
        {
            var payments = await _paymentRepo.GetByInvoiceAsync(invoice.Id, ct);
            var cashReversed = payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
            var cardReversed = payments.Where(p => p.PaymentMethod == PaymentMethod.Card).Sum(p => p.Amount);
            var onAccountReversed = payments.Where(p => p.PaymentMethod == PaymentMethod.OnAccount).Sum(p => p.Amount);

            session.ReverseSale(invoice.NetTotal, cashReversed, cardReversed, onAccountReversed);
            _sessionRepo.Update(session);
        }

        // ══════════════════════════════════════════════════════════
        //  REPORTS
        // ══════════════════════════════════════════════════════════

        public async Task<ServiceResult<PosDailyReportDto>> GetDailyReportAsync(DateTime date, CancellationToken ct = default)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var sessions = await _sessionRepo.GetByDateRangeAsync(startOfDay, endOfDay, ct);

            var report = new PosDailyReportDto
            {
                Date = date.Date,
                TotalTransactions = sessions.Sum(s => s.TransactionCount),
                TotalSales = sessions.Sum(s => s.TotalSales),
                TotalCash = sessions.Sum(s => s.TotalCashReceived),
                TotalCard = sessions.Sum(s => s.TotalCardReceived),
                TotalOnAccount = sessions.Sum(s => s.TotalOnAccount),
                // COGS and profit would require line-level queries; simplified here
                TotalCogs = 0,
                GrossProfit = 0
            };

            return ServiceResult<PosDailyReportDto>.Success(report);
        }

        public async Task<ServiceResult<PosSessionReportDto>> GetSessionReportAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepo.GetWithPaymentsAsync(sessionId, ct);
            if (session == null)
                return ServiceResult<PosSessionReportDto>.Failure("الجلسة غير موجودة.");

            var payments = await _paymentRepo.GetBySessionAsync(sessionId, ct);
            var invoiceIds = payments.Select(p => p.SalesInvoiceId).Distinct().ToList();

            var sales = new List<PosSessionSaleDto>();
            foreach (var invoiceId in invoiceIds)
            {
                var invoice = await _invoiceRepo.GetWithLinesAsync(invoiceId, ct);
                if (invoice == null) continue;

                var invoicePayments = payments.Where(p => p.SalesInvoiceId == invoiceId).ToList();
                var methodNames = string.Join(" + ", invoicePayments.Select(p => p.PaymentMethod.ToString()).Distinct());

                sales.Add(new PosSessionSaleDto
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    CustomerNameAr = invoice.Customer?.NameAr ?? "عميل نقدي",
                    NetTotal = invoice.NetTotal,
                    PaymentMethods = methodNames
                });
            }

            var report = new PosSessionReportDto
            {
                Session = PosMapper.ToSessionDto(session),
                Sales = sales
            };

            return ServiceResult<PosSessionReportDto>.Success(report);
        }

        public async Task<ServiceResult<PosProfitReportDto>> GetProfitReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            // Simplified: Full implementation would use direct SQL for performance
            var report = new PosProfitReportDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalRevenue = 0,
                TotalCogs = 0,
                GrossProfit = 0,
                GrossProfitMarginPercent = 0,
                Lines = new List<PosProfitLineDto>()
            };

            var sessions = await _sessionRepo.GetByDateRangeAsync(fromDate, toDate.AddDays(1), ct);
            report.TotalRevenue = sessions.Sum(s => s.TotalSales);

            return ServiceResult<PosProfitReportDto>.Success(report);
        }

        public async Task<ServiceResult<CashVarianceReportDto>> GetCashVarianceReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var sessions = await _sessionRepo.GetByDateRangeAsync(fromDate, toDate.AddDays(1), ct);

            var lines = sessions
                .Where(s => s.Status == PosSessionStatus.Closed)
                .Select(s => new CashVarianceLineDto
                {
                    SessionNumber = s.SessionNumber,
                    OpenedAt = s.OpenedAt,
                    ClosedAt = s.ClosedAt,
                    OpeningBalance = s.OpeningBalance,
                    TotalCashReceived = s.TotalCashReceived,
                    ExpectedBalance = s.OpeningBalance + s.TotalCashReceived,
                    ClosingBalance = s.ClosingBalance,
                    Variance = s.Variance
                })
                .ToList();

            var report = new CashVarianceReportDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                Lines = lines,
                TotalVariance = lines.Sum(l => l.Variance)
            };

            return ServiceResult<CashVarianceReportDto>.Success(report);
        }

        private sealed class PosPaymentBreakdown
        {
            public decimal TotalCash { get; init; }
            public decimal TotalCard { get; init; }
            public decimal TotalOnAccount { get; init; }
            public int CustomerId { get; init; }
            public List<PosParsedPayment> Payments { get; init; } = new();
            public decimal TotalPaid => TotalCash + TotalCard + TotalOnAccount;
        }

        private sealed class PosJournalContext
        {
            public FiscalYear FiscalYear { get; init; } = default!;
            public FiscalPeriod Period { get; init; } = default!;
            public DateTime Now { get; init; }
            public string Username { get; init; } = string.Empty;
        }

        private sealed record PosParsedPayment(PaymentMethod Method, decimal Amount, string Reference);
    }

    public sealed class PosRepositories
    {
        public PosRepositories(
            PosSalesRepositories salesRepos,
            PosInventoryRepositories inventoryRepos,
            PosAccountingRepositories accountingRepos)
        {
            if (salesRepos == null) throw new ArgumentNullException(nameof(salesRepos));
            if (inventoryRepos == null) throw new ArgumentNullException(nameof(inventoryRepos));
            if (accountingRepos == null) throw new ArgumentNullException(nameof(accountingRepos));

            SessionRepo = salesRepos.SessionRepo;
            PaymentRepo = salesRepos.PaymentRepo;
            InvoiceRepo = salesRepos.InvoiceRepo;

            ProductRepo = inventoryRepos.ProductRepo;
            WhProductRepo = inventoryRepos.WhProductRepo;
            MovementRepo = inventoryRepos.MovementRepo;

            JournalRepo = accountingRepos.JournalRepo;
            AccountRepo = accountingRepos.AccountRepo;
        }

        public IPosSessionRepository SessionRepo { get; }
        public IPosPaymentRepository PaymentRepo { get; }
        public ISalesInvoiceRepository InvoiceRepo { get; }
        public IProductRepository ProductRepo { get; }
        public IWarehouseProductRepository WhProductRepo { get; }
        public IInventoryMovementRepository MovementRepo { get; }
        public IJournalEntryRepository JournalRepo { get; }
        public IAccountRepository AccountRepo { get; }
    }

    public sealed class PosSalesRepositories
    {
        public PosSalesRepositories(
            IPosSessionRepository sessionRepo,
            IPosPaymentRepository paymentRepo,
            ISalesInvoiceRepository invoiceRepo)
        {
            SessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));
            PaymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            InvoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
        }

        public IPosSessionRepository SessionRepo { get; }
        public IPosPaymentRepository PaymentRepo { get; }
        public ISalesInvoiceRepository InvoiceRepo { get; }
    }

    public sealed class PosInventoryRepositories
    {
        public PosInventoryRepositories(
            IProductRepository productRepo,
            IWarehouseProductRepository whProductRepo,
            IInventoryMovementRepository movementRepo)
        {
            ProductRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            WhProductRepo = whProductRepo ?? throw new ArgumentNullException(nameof(whProductRepo));
            MovementRepo = movementRepo ?? throw new ArgumentNullException(nameof(movementRepo));
        }

        public IProductRepository ProductRepo { get; }
        public IWarehouseProductRepository WhProductRepo { get; }
        public IInventoryMovementRepository MovementRepo { get; }
    }

    public sealed class PosAccountingRepositories
    {
        public PosAccountingRepositories(
            IJournalEntryRepository journalRepo,
            IAccountRepository accountRepo)
        {
            JournalRepo = journalRepo ?? throw new ArgumentNullException(nameof(journalRepo));
            AccountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
        }

        public IJournalEntryRepository JournalRepo { get; }
        public IAccountRepository AccountRepo { get; }
    }

    public sealed class PosServices
    {
        public PosServices(
            IFiscalYearRepository fiscalYearRepo,
            IJournalNumberGenerator journalNumberGen,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDateTimeProvider dateTime)
        {
            FiscalYearRepo = fiscalYearRepo ?? throw new ArgumentNullException(nameof(fiscalYearRepo));
            JournalNumberGen = journalNumberGen ?? throw new ArgumentNullException(nameof(journalNumberGen));
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            DateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public IFiscalYearRepository FiscalYearRepo { get; }
        public IJournalNumberGenerator JournalNumberGen { get; }
        public IUnitOfWork UnitOfWork { get; }
        public ICurrentUserService CurrentUser { get; }
        public IDateTimeProvider DateTime { get; }
    }

    public sealed class PosValidators
    {
        public PosValidators(
            IValidator<OpenPosSessionDto> openSessionValidator,
            IValidator<ClosePosSessionDto> closeSessionValidator,
            IValidator<CompletePoseSaleDto> completeSaleValidator)
        {
            OpenSessionValidator = openSessionValidator ?? throw new ArgumentNullException(nameof(openSessionValidator));
            CloseSessionValidator = closeSessionValidator ?? throw new ArgumentNullException(nameof(closeSessionValidator));
            CompleteSaleValidator = completeSaleValidator ?? throw new ArgumentNullException(nameof(completeSaleValidator));
        }

        public IValidator<OpenPosSessionDto> OpenSessionValidator { get; }
        public IValidator<ClosePosSessionDto> CloseSessionValidator { get; }
        public IValidator<CompletePoseSaleDto> CompleteSaleValidator { get; }
    }
}
