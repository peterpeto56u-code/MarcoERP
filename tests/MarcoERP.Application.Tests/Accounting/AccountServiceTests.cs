using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Services.Accounting;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces;

namespace MarcoERP.Application.Tests.Accounting
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();
        private readonly Mock<IAuditLogger> _auditLoggerMock = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly Mock<IValidator<CreateAccountDto>> _createValidatorMock = new();
        private readonly Mock<IValidator<UpdateAccountDto>> _updateValidatorMock = new();

        public AccountServiceTests()
        {
            _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            _dateTimeProviderMock.Setup(d => d.Today).Returns(new DateTime(2026, 1, 1));

            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(
                    It.IsAny<Func<Task>>(),
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, IsolationLevel, CancellationToken>((op, _, __) => op());

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _auditLoggerMock
                .Setup(a => a.LogAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private AccountService CreateService() => new AccountService(
            _accountRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _auditLoggerMock.Object,
            _dateTimeProviderMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);

        private void SetupAuth(bool isAuthenticated = true, bool hasPermission = true)
        {
            _currentUserMock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
            _currentUserMock.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(hasPermission);
            _currentUserMock.Setup(x => x.Username).Returns("admin");
            _currentUserMock.Setup(x => x.UserId).Returns(1);
        }

        private void SetupValidValidation()
        {
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private static Account CreateTestAccount(
            string code = "1111",
            string nameAr = "نقدية",
            string nameEn = "Cash",
            AccountType type = AccountType.Asset,
            int? parentId = 1,
            int level = 4,
            bool isSystemAccount = false,
            string currencyCode = "SAR")
        {
            return new Account(code, nameAr, nameEn, type, parentId, level, isSystemAccount, currencyCode);
        }

        private static CreateAccountDto CreateValidCreateDto() => new CreateAccountDto
        {
            AccountCode = "1111",
            AccountNameAr = "نقدية",
            AccountNameEn = "Cash",
            AccountType = AccountType.Asset,
            ParentAccountId = null,
            Level = 4,
            CurrencyCode = "SAR"
        };

        private static UpdateAccountDto CreateValidUpdateDto() => new UpdateAccountDto
        {
            Id = 1,
            AccountNameAr = "نقدية محدثة",
            AccountNameEn = "Updated Cash",
            Description = "Updated"
        };

        // =====================================================================
        // 1. Authorization Tests
        // =====================================================================

        [Fact]
        public async Task CreateAsync_WhenNotAuthenticated_ReturnsFailure()
        {
            // Arrange
            SetupAuth(isAuthenticated: false);
            var service = CreateService();
            var dto = CreateValidCreateDto();

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenNoPermission_ReturnsFailure()
        {
            // Arrange
            SetupAuth(isAuthenticated: true, hasPermission: false);
            var service = CreateService();
            var dto = CreateValidCreateDto();

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenNoPermission_ReturnsFailure()
        {
            // Arrange
            SetupAuth(isAuthenticated: true, hasPermission: false);
            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(1, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            _accountRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenNoPermission_ReturnsFailure()
        {
            // Arrange
            SetupAuth(isAuthenticated: true, hasPermission: false);
            var service = CreateService();
            var dto = CreateValidUpdateDto();

            // Act
            var result = await service.UpdateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            _accountRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // =====================================================================
        // 2. GetByIdAsync Tests
        // =====================================================================

        [Fact]
        public async Task GetByIdAsync_WhenAccountExists_ReturnsSuccess()
        {
            // Arrange
            var account = CreateTestAccount();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.AccountCode.Should().Be("1111");
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
        {
            // Arrange
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        // =====================================================================
        // 3. CreateAsync Tests
        // =====================================================================

        [Fact]
        public async Task CreateAsync_WhenValidationFails_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Code", "Code is required")
            };
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var service = CreateService();
            var dto = CreateValidCreateDto();

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Contains("Code"));
            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenCodeExists_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            _accountRepoMock
                .Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = CreateService();
            var dto = CreateValidCreateDto();

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenParentNotFound_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            _accountRepoMock
                .Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var service = CreateService();
            var dto = CreateValidCreateDto();
            dto.ParentAccountId = 999;

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_WhenLevelMismatch_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            _accountRepoMock
                .Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var parent = CreateTestAccount(code: "1100", nameEn: "Parent", parentId: 1, level: 2);
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parent);

            var service = CreateService();
            var dto = CreateValidCreateDto();
            dto.ParentAccountId = 1;
            dto.Level = 4; // parent is level 2, child should be level 3

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            _accountRepoMock
                .Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _accountRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var dto = CreateValidCreateDto();
            dto.AccountCode = "1000";
            dto.Level = 1;
            dto.ParentAccountId = null;

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithParent_MarksParentAsParent()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            _accountRepoMock
                .Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var parent = CreateTestAccount(code: "1110", nameEn: "Parent", parentId: 1, level: 3);
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parent);
            _accountRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var dto = CreateValidCreateDto();
            dto.ParentAccountId = 1;
            dto.Level = 4;

            // Act
            var result = await service.CreateAsync(dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            parent.AllowPosting.Should().BeFalse("parent accounts should not allow posting");
            _accountRepoMock.Verify(r => r.Update(parent), Times.Once);
        }

        // =====================================================================
        // 4. UpdateAsync Tests
        // =====================================================================

        [Fact]
        public async Task UpdateAsync_WhenValidationFails_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("NameAr", "Arabic name is required")
            };
            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var service = CreateService();
            var dto = CreateValidUpdateDto();

            // Act
            var result = await service.UpdateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Contains("name") || e.Contains("Name"));
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var service = CreateService();
            var dto = CreateValidUpdateDto();

            // Act
            var result = await service.UpdateAsync(dto, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            SetupAuth();
            SetupValidValidation();
            var account = CreateTestAccount();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            var service = CreateService();
            var dto = CreateValidUpdateDto();

            // Act
            var result = await service.UpdateAsync(dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            _accountRepoMock.Verify(r => r.Update(account), Times.Once);
        }

        // =====================================================================
        // 5. DeactivateAsync / ActivateAsync Tests
        // =====================================================================

        [Fact]
        public async Task DeactivateAsync_WhenNotFound_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var service = CreateService();

            // Act
            var result = await service.DeactivateAsync(999, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task ActivateAsync_WhenExists_ReturnsSuccess()
        {
            // Arrange
            SetupAuth();
            var account = CreateTestAccount();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            var service = CreateService();

            // Act
            var result = await service.ActivateAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        // =====================================================================
        // 6. DeleteAsync Tests
        // =====================================================================

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(999, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenHasChildren_ReturnsFailure()
        {
            // Arrange
            SetupAuth();
            var account = CreateTestAccount();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);
            _accountRepoMock
                .Setup(r => r.HasChildrenAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(1, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WithNoChildren_ReturnsSuccess()
        {
            // Arrange
            SetupAuth();
            var account = CreateTestAccount();
            _accountRepoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);
            _accountRepoMock
                .Setup(r => r.HasChildrenAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}
