using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Represents a complexity profile (Simple / Standard / Advanced).
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public sealed class SystemProfile : AuditableEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private SystemProfile() { }

        /// <summary>
        /// Creates a new system profile.
        /// </summary>
        public SystemProfile(string profileName, string description, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new System.ArgumentException("اسم البروفايل مطلوب.", nameof(profileName));

            ProfileName = profileName.Trim();
            Description = description?.Trim();
            IsActive = isActive;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Profile name: Simple / Standard / Advanced.</summary>
        public string ProfileName { get; private set; }

        /// <summary>Arabic description of the profile.</summary>
        public string Description { get; private set; }

        /// <summary>Whether this profile is currently active.</summary>
        public bool IsActive { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>Activates this profile.</summary>
        public void Activate() => IsActive = true;

        /// <summary>Deactivates this profile.</summary>
        public void Deactivate() => IsActive = false;
    }
}
