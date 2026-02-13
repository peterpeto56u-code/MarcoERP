namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Provides the current company context for queries and operations.
    /// Currently returns DefaultCompany (Id=1).
    /// Future: will support company switching for Multi-Company.
    /// </summary>
    public interface ICompanyContext
    {
        /// <summary>The Id of the currently active company.</summary>
        int CurrentCompanyId { get; }
    }
}
