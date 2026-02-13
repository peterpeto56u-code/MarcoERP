using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Purchases;
using MarcoERP.Application.Mappers.Purchases;
using MarcoERP.Domain.Entities.Purchases;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Purchases;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Inventory;
using MarcoERP.Domain.Interfaces.Purchases;

namespace MarcoERP.Application.Services.Purchases
{
    /// <summary>
    /// Implements purchase quotation lifecycle: Create → Send → Accept/Reject → Convert to Invoice.
    /// </summary>
    [Module(SystemModule.Purchases)]
    public sealed class PurchaseQuotationService : IPurchaseQuotationService
    {
        private readonly IPurchaseQuotationRepository _quotationRepo;
        private readonly IPurchaseInvoiceRepository _invoiceRepo;
        private readonly IProductRepository _productRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDateTimeProvider _dateTime;
        private readonly IValidator<CreatePurchaseQuotationDto> _createValidator;
        private readonly IValidator<UpdatePurchaseQuotationDto> _updateValidator;

        private const string QuotationNotFoundMessage = "طلب الشراء غير موجود.";

        public PurchaseQuotationService(
            IPurchaseQuotationRepository quotationRepo,
            IPurchaseInvoiceRepository invoiceRepo,
            IProductRepository productRepo,
            ISupplierRepository supplierRepo,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDateTimeProvider dateTime,
            IValidator<CreatePurchaseQuotationDto> createValidator,
            IValidator<UpdatePurchaseQuotationDto> updateValidator)
        {
            _quotationRepo = quotationRepo ?? throw new ArgumentNullException(nameof(quotationRepo));
            _invoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _supplierRepo = supplierRepo ?? throw new ArgumentNullException(nameof(supplierRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<ServiceResult<IReadOnlyList<PurchaseQuotationListDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await _quotationRepo.GetAllAsync(ct);
            return ServiceResult<IReadOnlyList<PurchaseQuotationListDto>>.Success(
                entities.Select(PurchaseQuotationMapper.ToListDto).ToList());
        }

        public async Task<ServiceResult<PurchaseQuotationDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quotationRepo.GetWithLinesAsync(id, ct);
            if (entity == null)
                return ServiceResult<PurchaseQuotationDto>.Failure(QuotationNotFoundMessage);
            return ServiceResult<PurchaseQuotationDto>.Success(PurchaseQuotationMapper.ToDto(entity));
        }

        public async Task<ServiceResult<string>> GetNextNumberAsync(CancellationToken ct = default)
        {
            var next = await _quotationRepo.GetNextNumberAsync(ct);
            return ServiceResult<string>.Success(next);
        }

        public async Task<ServiceResult<PurchaseQuotationDto>> CreateAsync(CreatePurchaseQuotationDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<PurchaseQuotationDto>(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var vr = await _createValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<PurchaseQuotationDto>.Failure(
                    string.Join(" | ", vr.Errors.Select(e => e.ErrorMessage)));

            try
            {
                var quotationNumber = await _quotationRepo.GetNextNumberAsync(ct);
                var quotation = new PurchaseQuotation(
                    quotationNumber, dto.QuotationDate, dto.ValidUntil,
                    dto.SupplierId, dto.WarehouseId, dto.Notes);

                foreach (var lineDto in dto.Lines)
                {
                    var product = await _productRepo.GetByIdWithUnitsAsync(lineDto.ProductId, ct);
                    if (product == null)
                        return ServiceResult<PurchaseQuotationDto>.Failure($"الصنف برقم {lineDto.ProductId} غير موجود.");

                    var productUnit = product.ProductUnits.FirstOrDefault(pu => pu.UnitId == lineDto.UnitId);
                    if (productUnit == null)
                        return ServiceResult<PurchaseQuotationDto>.Failure(
                            $"الوحدة المحددة غير مرتبطة بالصنف ({product.NameAr}).");

                    quotation.AddLine(
                        lineDto.ProductId, lineDto.UnitId, lineDto.Quantity,
                        lineDto.UnitPrice, productUnit.ConversionFactor,
                        lineDto.DiscountPercent, product.VatRate);
                }

                await _quotationRepo.AddAsync(quotation, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                var saved = await _quotationRepo.GetWithLinesAsync(quotation.Id, ct);
                return ServiceResult<PurchaseQuotationDto>.Success(PurchaseQuotationMapper.ToDto(saved));
            }
            catch (PurchaseQuotationDomainException ex)
            {
                return ServiceResult<PurchaseQuotationDto>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult<PurchaseQuotationDto>> UpdateAsync(UpdatePurchaseQuotationDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<PurchaseQuotationDto>(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var vr = await _updateValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<PurchaseQuotationDto>.Failure(
                    string.Join(" | ", vr.Errors.Select(e => e.ErrorMessage)));

            var quotation = await _quotationRepo.GetWithLinesAsync(dto.Id, ct);
            if (quotation == null)
                return ServiceResult<PurchaseQuotationDto>.Failure(QuotationNotFoundMessage);

            try
            {
                quotation.UpdateHeader(dto.QuotationDate, dto.ValidUntil,
                    dto.SupplierId, dto.WarehouseId, dto.Notes);

                var newLines = new List<PurchaseQuotationLine>();
                foreach (var lineDto in dto.Lines)
                {
                    var product = await _productRepo.GetByIdWithUnitsAsync(lineDto.ProductId, ct);
                    if (product == null)
                        return ServiceResult<PurchaseQuotationDto>.Failure($"الصنف برقم {lineDto.ProductId} غير موجود.");

                    var productUnit = product.ProductUnits.FirstOrDefault(pu => pu.UnitId == lineDto.UnitId);
                    if (productUnit == null)
                        return ServiceResult<PurchaseQuotationDto>.Failure(
                            $"الوحدة المحددة غير مرتبطة بالصنف ({product.NameAr}).");

                    newLines.Add(new PurchaseQuotationLine(
                        lineDto.ProductId, lineDto.UnitId, lineDto.Quantity,
                        lineDto.UnitPrice, productUnit.ConversionFactor,
                        lineDto.DiscountPercent, product.VatRate));
                }

                quotation.ReplaceLines(newLines);
                _quotationRepo.Update(quotation);
                await _unitOfWork.SaveChangesAsync(ct);

                var saved = await _quotationRepo.GetWithLinesAsync(quotation.Id, ct);
                return ServiceResult<PurchaseQuotationDto>.Success(PurchaseQuotationMapper.ToDto(saved));
            }
            catch (PurchaseQuotationDomainException ex)
            {
                return ServiceResult<PurchaseQuotationDto>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult> SendAsync(int id, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var quotation = await _quotationRepo.GetWithLinesAsync(id, ct);
            if (quotation == null) return ServiceResult.Failure(QuotationNotFoundMessage);

            try { quotation.Send(); _quotationRepo.Update(quotation); await _unitOfWork.SaveChangesAsync(ct); return ServiceResult.Success(); }
            catch (PurchaseQuotationDomainException ex) { return ServiceResult.Failure(ex.Message); }
        }

        public async Task<ServiceResult> AcceptAsync(int id, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var quotation = await _quotationRepo.GetByIdAsync(id, ct);
            if (quotation == null) return ServiceResult.Failure(QuotationNotFoundMessage);

            try { quotation.Accept(_dateTime.UtcNow); _quotationRepo.Update(quotation); await _unitOfWork.SaveChangesAsync(ct); return ServiceResult.Success(); }
            catch (PurchaseQuotationDomainException ex) { return ServiceResult.Failure(ex.Message); }
        }

        public async Task<ServiceResult> RejectAsync(int id, string reason = null, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var quotation = await _quotationRepo.GetByIdAsync(id, ct);
            if (quotation == null) return ServiceResult.Failure(QuotationNotFoundMessage);

            try { quotation.Reject(reason); _quotationRepo.Update(quotation); await _unitOfWork.SaveChangesAsync(ct); return ServiceResult.Success(); }
            catch (PurchaseQuotationDomainException ex) { return ServiceResult.Failure(ex.Message); }
        }

        public async Task<ServiceResult> CancelAsync(int id, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var quotation = await _quotationRepo.GetByIdAsync(id, ct);
            if (quotation == null) return ServiceResult.Failure(QuotationNotFoundMessage);

            try { quotation.Cancel(); _quotationRepo.Update(quotation); await _unitOfWork.SaveChangesAsync(ct); return ServiceResult.Success(); }
            catch (PurchaseQuotationDomainException ex) { return ServiceResult.Failure(ex.Message); }
        }

        public async Task<ServiceResult> DeleteDraftAsync(int id, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.PurchaseQuotationCreate);
            if (authCheck != null) return authCheck;

            var quotation = await _quotationRepo.GetByIdAsync(id, ct);
            if (quotation == null) return ServiceResult.Failure(QuotationNotFoundMessage);

            try { quotation.SoftDelete(_currentUser.Username, _dateTime.UtcNow); _quotationRepo.Update(quotation); await _unitOfWork.SaveChangesAsync(ct); return ServiceResult.Success(); }
            catch (PurchaseQuotationDomainException ex) { return ServiceResult.Failure(ex.Message); }
        }

        public async Task<ServiceResult<int>> ConvertToInvoiceAsync(int quotationId, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<int>(_currentUser, PermissionKeys.PurchasesCreate);
            if (authCheck != null) return authCheck;

            var quotation = await _quotationRepo.GetWithLinesAsync(quotationId, ct);
            if (quotation == null)
                return ServiceResult<int>.Failure(QuotationNotFoundMessage);

            if (quotation.Status != QuotationStatus.Accepted)
                return ServiceResult<int>.Failure("يجب قبول طلب الشراء قبل التحويل لفاتورة.");

            if (quotation.IsExpired(_dateTime.UtcNow))
                return ServiceResult<int>.Failure("طلب الشراء منتهي الصلاحية.");

            try
            {
                var invoiceNumber = await _invoiceRepo.GetNextNumberAsync(ct);

                var invoice = new PurchaseInvoice(
                    invoiceNumber,
                    _dateTime.UtcNow.Date,
                    quotation.SupplierId,
                    quotation.WarehouseId,
                    $"محوّل من طلب شراء رقم {quotation.QuotationNumber}");

                foreach (var qLine in quotation.Lines)
                {
                    invoice.AddLine(
                        qLine.ProductId, qLine.UnitId, qLine.Quantity,
                        qLine.UnitPrice, qLine.ConversionFactor,
                        qLine.DiscountPercent, qLine.VatRate);
                }

                await _invoiceRepo.AddAsync(invoice, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                quotation.MarkAsConverted(invoice.Id, _dateTime.UtcNow);
                _quotationRepo.Update(quotation);
                await _unitOfWork.SaveChangesAsync(ct);

                return ServiceResult<int>.Success(invoice.Id);
            }
            catch (PurchaseQuotationDomainException ex) { return ServiceResult<int>.Failure(ex.Message); }
            catch (PurchaseInvoiceDomainException ex) { return ServiceResult<int>.Failure(ex.Message); }
        }
    }
}
