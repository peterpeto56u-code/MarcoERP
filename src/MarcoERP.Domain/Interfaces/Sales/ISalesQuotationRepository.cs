using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Domain.Interfaces.Sales
{
    /// <summary>
    /// Repository contract for SalesQuotation aggregate.
    /// </summary>
    public interface ISalesQuotationRepository : IRepository<SalesQuotation>
    {
        /// <summary>Gets quotation with all lines eagerly loaded.</summary>
        Task<SalesQuotation> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Gets quotation by number.</summary>
        Task<SalesQuotation> GetByNumberAsync(string quotationNumber, CancellationToken cancellationToken = default);

        /// <summary>Checks if a quotation number already exists.</summary>
        Task<bool> NumberExistsAsync(string quotationNumber, CancellationToken cancellationToken = default);

        /// <summary>Gets quotations filtered by status.</summary>
        Task<IReadOnlyList<SalesQuotation>> GetByStatusAsync(QuotationStatus status, CancellationToken cancellationToken = default);

        /// <summary>Gets quotations for a specific customer.</summary>
        Task<IReadOnlyList<SalesQuotation>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);

        /// <summary>Generates the next quotation number (SQ-YYYYMM-####).</summary>
        Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default);
    }
}
