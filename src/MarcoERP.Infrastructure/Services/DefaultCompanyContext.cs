using MarcoERP.Application.Interfaces;

namespace MarcoERP.Infrastructure.Services
{
    /// <summary>
    /// Default company context — always returns CompanyId = 1.
    /// This is the single-company implementation.
    /// Future: replace with a context-aware implementation for Multi-Company.
    /// </summary>
    public sealed class DefaultCompanyContext : ICompanyContext
    {
        /// <inheritdoc />
        public int CurrentCompanyId => 1;
    }
}
