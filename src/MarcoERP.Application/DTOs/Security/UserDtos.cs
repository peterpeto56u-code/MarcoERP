using System;

namespace MarcoERP.Application.DTOs.Security
{
    // ════════════════════════════════════════════════════════════
    //  User DTOs (إدارة المستخدمين)
    // ════════════════════════════════════════════════════════════

    /// <summary>Full user details for display.</summary>
    public sealed class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullNameAr { get; set; }
        public string FullNameEn { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public string RoleNameAr { get; set; }
        public string RoleNameEn { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    /// <summary>Lightweight user for list views.</summary>
    public sealed class UserListDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullNameAr { get; set; }
        public string RoleNameAr { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>DTO for creating a new user.</summary>
    public sealed class CreateUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FullNameAr { get; set; }
        public string FullNameEn { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
    }

    /// <summary>DTO for updating an existing user (no password change).</summary>
    public sealed class UpdateUserDto
    {
        public int Id { get; set; }
        public string FullNameAr { get; set; }
        public string FullNameEn { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
    }

    /// <summary>DTO for changing a user's own password.</summary>
    public sealed class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    /// <summary>DTO for admin resetting a user's password.</summary>
    public sealed class ResetPasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
