namespace MarcoERP.Domain.Entities.Common
{
    /// <summary>
    /// Base class for all business entities that belong to a specific company.
    /// Extends SoftDeletableEntity with CompanyId for multi-company isolation.
    /// Currently all entities default to CompanyId = 1 (DefaultCompany).
    /// </summary>
    public abstract class CompanyAwareEntity : SoftDeletableEntity
    {
        /// <summary>
        /// FK to the owning Company. All transactional and master-data
        /// entities that are company-specific must inherit this class.
        /// Defaults to 1 (DefaultCompany) during single-company operation.
        /// </summary>
        public int CompanyId { get; protected set; } = 1;
    }
}
