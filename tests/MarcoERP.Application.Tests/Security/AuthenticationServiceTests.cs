using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Services.Security;
using MarcoERP.Domain.Entities.Security;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Security;

namespace MarcoERP.Application.Tests.Security
{
    public sealed class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRoleRepository> _roleRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;
        private readonly AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 2, 9, 12, 0, 0, DateTimeKind.Utc));

            _sut = new AuthenticationService(
                _userRepoMock.Object,
                _roleRepoMock.Object,
                _passwordHasherMock.Object,
                _dateTimeProviderMock.Object,
                _unitOfWorkMock.Object,
                _auditLoggerMock.Object);
        }

        // ── Helpers ─────────────────────────────────────────────

        private static User CreateActiveUser(int id = 1, string username = "testuser", string passwordHash = "hashed123")
        {
            var user = new User(username, passwordHash, "مستخدم تجريبي", "Test User", "test@test.com", "0500000000", 1);
            typeof(User).GetProperty("Id").SetValue(user, id);
            return user;
        }

        private static User CreateInactiveUser(int id = 2)
        {
            var user = CreateActiveUser(id, "inactive", "hashed123");
            user.Deactivate();
            return user;
        }

        private static User CreateLockedUser(int id = 3)
        {
            var user = CreateActiveUser(id, "locked", "hashed123");
            user.Lock(DateTime.UtcNow);
            return user;
        }

        private static Role CreateRoleWithPermissions(int id = 1)
        {
            var role = new Role("مستخدم", "User", "Test role");
            typeof(Role).GetProperty("Id").SetValue(role, id);
            role.AddPermission("sales.view");
            role.AddPermission("sales.create");
            return role;
        }

        // ═══════════════════════════════════════════════════════
        //  LOGIN TESTS
        // ═══════════════════════════════════════════════════════

        [Fact]
        public async Task LoginAsync_WhenEmptyCredentials_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { Username = "", Password = "" };

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("اسم المستخدم وكلمة المرور مطلوبين");
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { Username = "unknown", Password = "pass123" };
            _userRepoMock.Setup(r => r.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        [Fact]
        public async Task LoginAsync_WhenUserInactive_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { Username = "inactive", Password = "pass123" };
            var user = CreateInactiveUser();
            _userRepoMock.Setup(r => r.GetByUsernameAsync("inactive", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("الحساب معطّل");
        }

        [Fact]
        public async Task LoginAsync_WhenUserLocked_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { Username = "locked", Password = "pass123" };
            var user = CreateLockedUser();
            _userRepoMock.Setup(r => r.GetByUsernameAsync("locked", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("الحساب مقفل");
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordInvalid_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "wrongpass" };
            var user = CreateActiveUser();
            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("wrongpass", "hashed123"))
                .Returns(false);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordInvalid_RecordsFailedAttempt()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "wrongpass" };
            var user = CreateActiveUser();
            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("wrongpass", "hashed123"))
                .Returns(false);

            // Act
            await _sut.LoginAsync(dto);

            // Assert
            user.FailedLoginAttempts.Should().Be(1);
            _userRepoMock.Verify(r => r.Update(user), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordValid_ReturnsSuccessWithPermissions()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "correctpass" };
            var user = CreateActiveUser(1, "testuser", "hashed_correct");
            var role = CreateRoleWithPermissions(1);

            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("correctpass", "hashed_correct"))
                .Returns(true);
            _roleRepoMock.Setup(r => r.GetByIdWithPermissionsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.UserId.Should().Be(1);
            result.Data.Username.Should().Be("testuser");
            result.Data.FullNameAr.Should().Be("مستخدم تجريبي");
            result.Data.RoleId.Should().Be(1);
            result.Data.RoleNameAr.Should().Be("مستخدم");
            result.Data.RoleNameEn.Should().Be("User");
            result.Data.Permissions.Should().Contain("sales.view");
            result.Data.Permissions.Should().Contain("sales.create");
            result.Data.Permissions.Should().HaveCount(2);
        }

        [Fact]
        public async Task LoginAsync_WhenAuditLoggingFailsOnSuccess_StillReturnsSuccess()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "correctpass" };
            var user = CreateActiveUser(1, "testuser", "hashed_correct");
            var role = CreateRoleWithPermissions(1);

            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("correctpass", "hashed_correct"))
                .Returns(true);
            _roleRepoMock.Setup(r => r.GetByIdWithPermissionsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);
            _auditLoggerMock.Setup(a => a.LogAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Audit storage unavailable"));

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task LoginAsync_AfterFiveFailedAttempts_LocksAccount()
        {
            // Arrange
            var user = CreateActiveUser(1, "testuser", "hashed123");
            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("wrongpass", "hashed123"))
                .Returns(false);

            var dto = new LoginDto { Username = "testuser", Password = "wrongpass" };

            // Act — simulate 5 consecutive failed login attempts
            for (int i = 0; i < 5; i++)
            {
                await _sut.LoginAsync(dto);
            }

            // Assert
            user.IsLocked.Should().BeTrue();
            user.FailedLoginAttempts.Should().Be(5);
        }

        [Fact]
        public async Task LoginAsync_WhenAuditLoggingFailsOnFailure_StillReturnsInvalidCredentials()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "wrongpass" };
            var user = CreateActiveUser();

            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("wrongpass", "hashed123"))
                .Returns(false);
            _auditLoggerMock.Setup(a => a.LogAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Audit storage unavailable"));

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("اسم المستخدم أو كلمة المرور غير صحيحة");
            user.FailedLoginAttempts.Should().Be(1);
        }

        // ═══════════════════════════════════════════════════════
        //  CHANGE PASSWORD TESTS
        // ═══════════════════════════════════════════════════════

        [Fact]
        public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "old123",
                NewPassword = "new456",
                ConfirmNewPassword = "new456"
            };
            _userRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _sut.ChangePasswordAsync(999, dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("المستخدم غير موجود");
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCurrentPasswordWrong_ReturnsFailure()
        {
            // Arrange
            var user = CreateActiveUser(1, "testuser", "hashed_old");
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "wrong_old",
                NewPassword = "new456",
                ConfirmNewPassword = "new456"
            };
            _userRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("wrong_old", "hashed_old"))
                .Returns(false);

            // Act
            var result = await _sut.ChangePasswordAsync(1, dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("كلمة المرور الحالية غير صحيحة");
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenPasswordsDontMatch_ReturnsFailure()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "old123",
                NewPassword = "new456",
                ConfirmNewPassword = "different789"
            };

            // Act
            var result = await _sut.ChangePasswordAsync(1, dto);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Contain("كلمة المرور الجديدة وتأكيدها غير متطابقتين");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var user = CreateActiveUser(1, "testuser", "hashed_old");
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "old_plain",
                NewPassword = "new_plain",
                ConfirmNewPassword = "new_plain"
            };
            _userRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword("old_plain", "hashed_old"))
                .Returns(true);
            _passwordHasherMock.Setup(h => h.HashPassword("new_plain"))
                .Returns("hashed_new");

            // Act
            var result = await _sut.ChangePasswordAsync(1, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.PasswordHash.Should().Be("hashed_new");
            user.MustChangePassword.Should().BeFalse();
            _userRepoMock.Verify(r => r.Update(user), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
