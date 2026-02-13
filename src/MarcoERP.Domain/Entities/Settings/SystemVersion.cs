using System;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Tracks registered system versions.
    /// Phase 5: Version &amp; Integrity Engine — tracking only, no behavioral changes.
    /// </summary>
    public sealed class SystemVersion : BaseEntity
    {
        /// <summary>EF Core only.</summary>
        private SystemVersion() { }

        /// <summary>
        /// Registers a new system version.
        /// </summary>
        public SystemVersion(string versionNumber, string appliedBy, string description, DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(versionNumber))
                throw new ArgumentException("رقم الإصدار مطلوب.", nameof(versionNumber));

            VersionNumber = versionNumber.Trim();
            AppliedAt = utcNow;
            AppliedBy = appliedBy?.Trim() ?? DomainConstants.SystemUser;
            Description = description?.Trim();
        }

        /// <summary>Semantic version string, e.g. "1.0.0".</summary>
        public string VersionNumber { get; private set; }

        /// <summary>UTC timestamp when this version was applied.</summary>
        public DateTime AppliedAt { get; private set; }

        /// <summary>Username who registered this version.</summary>
        public string AppliedBy { get; private set; }

        /// <summary>Description of changes in this version.</summary>
        public string Description { get; private set; }
    }
}
