namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Status of a user account.
    /// Active = can log in. Inactive = disabled by admin. Locked = auto-locked after failed attempts.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>User account is active and can authenticate.</summary>
        Active = 0,

        /// <summary>User account has been deactivated by an administrator.</summary>
        Inactive = 1,

        /// <summary>User account is temporarily locked due to excessive failed login attempts.</summary>
        Locked = 2
    }
}
