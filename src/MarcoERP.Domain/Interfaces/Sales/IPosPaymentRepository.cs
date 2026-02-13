using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Domain.Interfaces.Sales
{
    /// <summary>
    /// Repository contract for PosPayment entity.
    /// </summary>
    public interface IPosPaymentRepository : IRepository<PosPayment>
    {
        /// <summary>Gets all payments for a specific invoice.</summary>
        Task<IReadOnlyList<PosPayment>> GetByInvoiceAsync(int salesInvoiceId, CancellationToken ct = default);

        /// <summary>Gets all payments for a specific session.</summary>
        Task<IReadOnlyList<PosPayment>> GetBySessionAsync(int posSessionId, CancellationToken ct = default);

        /// <summary>Gets total by payment method for a session.</summary>
        Task<decimal> GetSessionTotalByMethodAsync(int posSessionId, PaymentMethod method, CancellationToken ct = default);
    }
}
