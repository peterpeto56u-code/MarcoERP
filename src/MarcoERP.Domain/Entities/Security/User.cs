using System;
using System.Collections.Generic;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions;

namespace MarcoERP.Domain.Entities.Security
{
    /// <summary>
    /// Represents a system user with authentication and authorization data.
    /// Lifecycle: Active → Locked (auto, after 5 failed attempts) → Unlocked (admin).
    /// Active ↔ Inactive (admin toggle).
    /// </summary>
    public sealed class User : AuditableEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private User() { }

        /// <summary>
        /// Creates a new user with the specified credentials and role.
        /// </summary>
        public User(
            string username,
            string passwordHash,
            string fullNameAr,
            string fullNameEn,
            string email,
            string phone,
            int roleId,
            bool mustChangePassword = true)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new SecurityDomainException("اسم المستخدم مطلوب.");
            if (username.Length < 3 || username.Length > 50)
                throw new SecurityDomainException("اسم المستخدم يجب أن يكون بين 3 و 50 حرفاً.");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new SecurityDomainException("كلمة المرور مطلوبة.");
            if (string.IsNullOrWhiteSpace(fullNameAr))
                throw new SecurityDomainException("الاسم الكامل بالعربية مطلوب.");
            if (roleId <= 0)
                throw new SecurityDomainException("الدور مطلوب.");

            Username = username.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            FullNameAr = fullNameAr.Trim();
            FullNameEn = fullNameEn?.Trim();
            Email = email?.Trim();
            Phone = phone?.Trim();
            RoleId = roleId;
            IsActive = true;
            IsLocked = false;
            FailedLoginAttempts = 0;
            MustChangePassword = mustChangePassword;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique login name (stored lowercase).</summary>
        public string Username { get; private set; }

        /// <summary>BCrypt hashed password.</summary>
        public string PasswordHash { get; private set; }

        /// <summary>Full name in Arabic.</summary>
        public string FullNameAr { get; private set; }

        /// <summary>Full name in English (optional).</summary>
        public string FullNameEn { get; private set; }

        /// <summary>Email address (optional).</summary>
        public string Email { get; private set; }

        /// <summary>Phone number (optional).</summary>
        public string Phone { get; private set; }

        /// <summary>Foreign key to the user's assigned role.</summary>
        public int RoleId { get; private set; }

        /// <summary>Navigation property to the Role entity.</summary>
        public Role Role { get; private set; }

        /// <summary>Whether the account can be used for login.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether the account is locked due to failed login attempts.</summary>
        public bool IsLocked { get; private set; }

        /// <summary>UTC timestamp when the account was locked. Used for timed lockout (AUTH-07).</summary>
        public DateTime? LockedAt { get; private set; }

        /// <summary>Number of consecutive failed login attempts.</summary>
        public int FailedLoginAttempts { get; private set; }

        /// <summary>UTC timestamp of the last successful login.</summary>
        public DateTime? LastLoginAt { get; private set; }

        /// <summary>If true, user must change password on next login.</summary>
        public bool MustChangePassword { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Updates the user's profile information.
        /// </summary>
        public void UpdateProfile(string fullNameAr, string fullNameEn, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(fullNameAr))
                throw new SecurityDomainException("الاسم الكامل بالعربية مطلوب.");

            FullNameAr = fullNameAr.Trim();
            FullNameEn = fullNameEn?.Trim();
            Email = email?.Trim();
            Phone = phone?.Trim();
        }

        /// <summary>
        /// Changes the user's role assignment.
        /// </summary>
        public void ChangeRole(int roleId)
        {
            if (roleId <= 0)
                throw new SecurityDomainException("الدور مطلوب.");
            RoleId = roleId;
        }

        /// <summary>
        /// Sets a new password hash and clears the MustChangePassword flag.
        /// </summary>
        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new SecurityDomainException("كلمة المرور الجديدة مطلوبة.");
            PasswordHash = newPasswordHash;
            MustChangePassword = false;
        }

        /// <summary>
        /// Resets the password (admin action) and forces password change on next login.
        /// </summary>
        public void ResetPassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new SecurityDomainException("كلمة المرور الجديدة مطلوبة.");
            PasswordHash = newPasswordHash;
            MustChangePassword = true;
        }

        /// <summary>
        /// Records a successful login — resets failed attempts and updates timestamp.
        /// </summary>
        public void RecordSuccessfulLogin(DateTime loginAt)
        {
            FailedLoginAttempts = 0;
            IsLocked = false;
            LastLoginAt = loginAt;
        }

        /// <summary>
        /// Records a failed login attempt. Locks the account after maxAttempts.
        /// SEC-AUTH-01: Lock after 5 failed attempts.
        /// AUTH-07: LockedAt timestamp set for timed lockout.
        /// </summary>
        public void RecordFailedLogin(DateTime utcNow, int maxAttempts = 5)
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= maxAttempts)
            {
                IsLocked = true;
                LockedAt = utcNow;
            }
        }

        /// <summary>
        /// AUTH-07: Checks if timed lockout has expired and auto-unlocks the account.
        /// Returns true if the account is still locked after evaluation.
        /// </summary>
        public bool IsLockedAt(DateTime utcNow, int lockoutMinutes = 15)
        {
            if (!IsLocked) return false;

            // If LockedAt is set and lockout period has expired, auto-unlock
            if (LockedAt.HasValue && lockoutMinutes > 0)
            {
                var lockoutExpiry = LockedAt.Value.AddMinutes(lockoutMinutes);
                if (utcNow >= lockoutExpiry)
                {
                    Unlock();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Locks the user account (admin action).
        /// </summary>
        public void Lock(DateTime utcNow)
        {
            IsLocked = true;
            LockedAt = utcNow;
        }

        /// <summary>
        /// Unlocks the user account and resets failed login attempts (admin action).
        /// </summary>
        public void Unlock()
        {
            IsLocked = false;
            LockedAt = null;
            FailedLoginAttempts = 0;
        }

        /// <summary>
        /// Activates the user account.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the user account.
        /// </summary>
        public void Deactivate()
        {
            if (Username == DomainConstants.AdminUsername)
                throw new SecurityDomainException("لا يمكن تعطيل حساب المدير الرئيسي.");
            IsActive = false;
        }
    }
}
