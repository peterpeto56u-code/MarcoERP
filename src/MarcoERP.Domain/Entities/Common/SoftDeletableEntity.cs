using System;

namespace MarcoERP.Domain.Entities.Common
{
    /// <summary>
    /// Extends AuditableEntity with soft-delete support.
    /// Financial entities use soft deletes — no hard deletes per RECORD_PROTECTION_POLICY.
    /// </summary>
    public abstract class SoftDeletableEntity : AuditableEntity
    {
        /// <summary>True if the record has been soft-deleted.</summary>
        public bool IsDeleted { get; private set; }

        /// <summary>UTC timestamp of soft deletion.</summary>
        public DateTime? DeletedAt { get; private set; }

        /// <summary>Username of the deleting user.</summary>
        public string DeletedBy { get; private set; }

        /// <summary>
        /// Marks the entity as soft-deleted.
        /// </summary>
        public virtual void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            if (string.IsNullOrWhiteSpace(deletedBy))
                throw new InvalidOperationException("DeletedBy is required for soft delete.");

            if (deletedAt == default)
                throw new InvalidOperationException("DeletedAt is required for soft delete.");

            IsDeleted = true;
            DeletedAt = deletedAt;
            DeletedBy = deletedBy.Trim();
        }

        /// <summary>
        /// Restores a soft-deleted entity. Admin use only.
        /// </summary>
        public virtual void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }
    }
}
