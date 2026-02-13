using System;
using System.Linq;
using FluentAssertions;
using MarcoERP.Domain.Entities.Security;
using Xunit;

namespace MarcoERP.Domain.Tests.Security
{
    public class UserTests
    {
        private User CreateValidUser()
        {
            return new User("testuser", "hashedpass123", "مستخدم اختبار", "Test User",
                "test@example.com", "0500000000", 2);
        }

        // ── Constructor ─────────────────────────────────────────

        [Fact]
        public void Constructor_ValidParameters_CreatesActiveUser()
        {
            var user = CreateValidUser();
            user.Username.Should().Be("testuser");
            user.FullNameAr.Should().Be("مستخدم اختبار");
            user.IsActive.Should().BeTrue();
            user.IsLocked.Should().BeFalse();
            user.FailedLoginAttempts.Should().Be(0);
            user.MustChangePassword.Should().BeTrue();
        }

        [Fact]
        public void Constructor_ShortUsername_ThrowsException()
        {
            Action act = () => new User("ab", "hash", "اسم", "Name", null, null, 1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_EmptyUsername_ThrowsException()
        {
            Action act = () => new User("", "hash", "اسم", "Name", null, null, 1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_EmptyPasswordHash_ThrowsException()
        {
            Action act = () => new User("testuser", "", "اسم", "Name", null, null, 1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_ZeroRoleId_ThrowsException()
        {
            Action act = () => new User("testuser", "hash", "اسم", "Name", null, null, 0);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_LowercasesUsername()
        {
            var user = new User("TestUser", "hash", "اسم", "Name", null, null, 1);
            user.Username.Should().Be("testuser");
        }

        // ── Login Tracking ──────────────────────────────────────

        [Fact]
        public void RecordSuccessfulLogin_ResetsFailedAttempts()
        {
            var user = CreateValidUser();
            user.RecordFailedLogin(DateTime.UtcNow);
            user.RecordFailedLogin(DateTime.UtcNow);
            user.RecordSuccessfulLogin(DateTime.UtcNow);

            user.FailedLoginAttempts.Should().Be(0);
            user.IsLocked.Should().BeFalse();
            user.LastLoginAt.Should().NotBeNull();
        }

        [Fact]
        public void RecordFailedLogin_IncreasesCounter()
        {
            var user = CreateValidUser();
            user.RecordFailedLogin(DateTime.UtcNow);
            user.FailedLoginAttempts.Should().Be(1);
        }

        [Fact]
        public void RecordFailedLogin_AtMaxAttempts_LocksUser()
        {
            var user = CreateValidUser();
            for (int i = 0; i < 5; i++)
                user.RecordFailedLogin(DateTime.UtcNow);

            user.IsLocked.Should().BeTrue();
            user.FailedLoginAttempts.Should().Be(5);
        }

        [Fact]
        public void RecordFailedLogin_CustomMaxAttempts_LocksAtThreshold()
        {
            var user = CreateValidUser();
            for (int i = 0; i < 3; i++)
                user.RecordFailedLogin(DateTime.UtcNow, 3);

            user.IsLocked.Should().BeTrue();
        }

        // ── Lock/Unlock ─────────────────────────────────────────

        [Fact]
        public void Lock_ActiveUser_LocksUser()
        {
            var user = CreateValidUser();
            user.Lock(DateTime.UtcNow);
            user.IsLocked.Should().BeTrue();
        }

        [Fact]
        public void Unlock_LockedUser_UnlocksAndResets()
        {
            var user = CreateValidUser();
            user.RecordFailedLogin(DateTime.UtcNow);
            user.RecordFailedLogin(DateTime.UtcNow);
            user.Lock(DateTime.UtcNow);
            user.Unlock();

            user.IsLocked.Should().BeFalse();
            user.FailedLoginAttempts.Should().Be(0);
        }

        // ── Activate/Deactivate ─────────────────────────────────

        [Fact]
        public void Deactivate_RegularUser_Deactivates()
        {
            var user = CreateValidUser();
            user.Deactivate();
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Deactivate_AdminUser_ThrowsException()
        {
            var user = new User("admin", "hash", "المدير", "Admin", null, null, 1);
            Action act = () => user.Deactivate();
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Activate_InactiveUser_Activates()
        {
            var user = CreateValidUser();
            user.Deactivate();
            user.Activate();
            user.IsActive.Should().BeTrue();
        }

        // ── Profile & Password ──────────────────────────────────

        [Fact]
        public void UpdateProfile_ValidParameters_Updates()
        {
            var user = CreateValidUser();
            user.UpdateProfile("اسم جديد", "New Name", "new@example.com", "0511111111");
            user.FullNameAr.Should().Be("اسم جديد");
            user.Email.Should().Be("new@example.com");
        }

        [Fact]
        public void UpdateProfile_EmptyNameAr_ThrowsException()
        {
            var user = CreateValidUser();
            Action act = () => user.UpdateProfile("", "Name", null, null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChangePassword_NewHash_SetsMustChangePasswordFalse()
        {
            var user = CreateValidUser();
            user.ChangePassword("newhash456");
            user.PasswordHash.Should().Be("newhash456");
            user.MustChangePassword.Should().BeFalse();
        }

        [Fact]
        public void ResetPassword_NewHash_SetsMustChangePasswordTrue()
        {
            var user = CreateValidUser();
            user.ChangePassword("newhash456"); // MustChangePassword = false
            user.ResetPassword("resethash789");
            user.PasswordHash.Should().Be("resethash789");
            user.MustChangePassword.Should().BeTrue();
        }

        [Fact]
        public void ChangeRole_ValidRoleId_Changes()
        {
            var user = CreateValidUser();
            user.ChangeRole(3);
            user.RoleId.Should().Be(3);
        }

        [Fact]
        public void ChangeRole_ZeroRoleId_ThrowsException()
        {
            var user = CreateValidUser();
            Action act = () => user.ChangeRole(0);
            act.Should().Throw<Exception>();
        }
    }

    public class RoleTests
    {
        private Role CreateValidRole()
        {
            return new Role("محاسب", "Accountant", "صلاحيات المحاسب");
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesRole()
        {
            var role = CreateValidRole();
            role.NameAr.Should().Be("محاسب");
            role.NameEn.Should().Be("Accountant");
            role.IsSystem.Should().BeFalse();
        }

        [Fact]
        public void Constructor_SystemRole_SetsIsSystemTrue()
        {
            var role = new Role("مدير النظام", "Admin", "المدير", true);
            role.IsSystem.Should().BeTrue();
        }

        [Fact]
        public void Constructor_EmptyNameAr_ThrowsException()
        {
            Action act = () => new Role("", "Accountant", "وصف");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_EmptyNameEn_ThrowsException()
        {
            Action act = () => new Role("محاسب", "", "وصف");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Update_ValidParameters_Updates()
        {
            var role = CreateValidRole();
            role.Update("مدير مبيعات", "Sales Manager", "وصف جديد");
            role.NameAr.Should().Be("مدير مبيعات");
            role.NameEn.Should().Be("Sales Manager");
        }

        [Fact]
        public void AddPermission_NewKey_AddsPermission()
        {
            var role = CreateValidRole();
            role.AddPermission("accounting.journal.create");
            role.HasPermission("accounting.journal.create").Should().BeTrue();
        }

        [Fact]
        public void AddPermission_DuplicateKey_NoOp()
        {
            var role = CreateValidRole();
            role.AddPermission("accounting.journal.create");
            role.AddPermission("accounting.journal.create");
            role.Permissions.Count.Should().Be(1);
        }

        [Fact]
        public void AddPermission_EmptyKey_ThrowsException()
        {
            var role = CreateValidRole();
            Action act = () => role.AddPermission("");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void RemovePermission_ExistingKey_Removes()
        {
            var role = CreateValidRole();
            role.AddPermission("accounting.journal.create");
            role.RemovePermission("accounting.journal.create");
            role.HasPermission("accounting.journal.create").Should().BeFalse();
        }

        [Fact]
        public void HasPermission_NonExistentKey_ReturnsFalse()
        {
            var role = CreateValidRole();
            role.HasPermission("nonexistent.key").Should().BeFalse();
        }

        [Fact]
        public void SetPermissions_ReplacesAll()
        {
            var role = CreateValidRole();
            role.AddPermission("old.key");
            role.SetPermissions(new[] { "new.key1", "new.key2" });

            role.Permissions.Count.Should().Be(2);
            role.HasPermission("old.key").Should().BeFalse();
            role.HasPermission("new.key1").Should().BeTrue();
            role.HasPermission("new.key2").Should().BeTrue();
        }

        [Fact]
        public void SetPermissions_SkipsBlankKeys()
        {
            var role = CreateValidRole();
            role.SetPermissions(new[] { "valid.key", "", "  ", "another.key" });
            role.Permissions.Count.Should().Be(2);
        }
    }
}
