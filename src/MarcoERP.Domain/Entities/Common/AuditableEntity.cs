using System;

namespace MarcoERP.Domain.Entities.Common
{
    /// <summary>
    /// Extends BaseEntity with standard audit tracking fields.
    /// All mutable domain entities should inherit from this.
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>UTC timestamp of record creation.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Username of the creator.</summary>
        public string CreatedBy { get; set; }

        /// <summary>UTC timestamp of last modification (null if never modified).</summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>Username of last modifier (null if never modified).</summary>
        public string ModifiedBy { get; set; }
    }
}
