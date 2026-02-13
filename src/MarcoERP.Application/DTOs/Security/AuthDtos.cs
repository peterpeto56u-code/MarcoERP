using System;
using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Security
{
    // ════════════════════════════════════════════════════════════
    //  Authentication DTOs (تسجيل الدخول)
    // ════════════════════════════════════════════════════════════

    /// <summary>Login request DTO.</summary>
    public sealed class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>Login result returned on successful authentication.</summary>
    public sealed class LoginResultDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullNameAr { get; set; }
        public int RoleId { get; set; }
        public string RoleNameAr { get; set; }
        public string RoleNameEn { get; set; }
        public bool MustChangePassword { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime LoginAt { get; set; }
    }
}
