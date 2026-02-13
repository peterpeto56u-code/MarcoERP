using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Application.Common;
using MarcoERP.Application.Interfaces.Treasury;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Persistence.Services
{
    public sealed class TreasuryInvoicePaymentQueryService : ITreasuryInvoicePaymentQueryService
    {
        private readonly MarcoDbContext _db;

        public TreasuryInvoicePaymentQueryService(MarcoDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<ServiceResult<decimal>> GetPostedReceiptsTotalForSalesInvoiceAsync(int salesInvoiceId, CancellationToken ct = default)
        {
            if (salesInvoiceId <= 0)
                return ServiceResult<decimal>.Failure("SalesInvoiceId غير صالح.");

            var total = await _db.CashReceipts
                .AsNoTracking()
                .Where(x => x.SalesInvoiceId == salesInvoiceId && x.Status == InvoiceStatus.Posted)
                .Select(x => (decimal?)x.Amount)
                .SumAsync(ct) ?? 0m;

            return ServiceResult<decimal>.Success(total);
        }

        public async Task<ServiceResult<decimal>> GetPostedPaymentsTotalForPurchaseInvoiceAsync(int purchaseInvoiceId, CancellationToken ct = default)
        {
            if (purchaseInvoiceId <= 0)
                return ServiceResult<decimal>.Failure("PurchaseInvoiceId غير صالح.");

            var total = await _db.CashPayments
                .AsNoTracking()
                .Where(x => x.PurchaseInvoiceId == purchaseInvoiceId && x.Status == InvoiceStatus.Posted)
                .Select(x => (decimal?)x.Amount)
                .SumAsync(ct) ?? 0m;

            return ServiceResult<decimal>.Success(total);
        }
    }
}
