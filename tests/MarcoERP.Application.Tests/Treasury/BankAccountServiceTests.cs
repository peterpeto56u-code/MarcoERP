using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Services.Treasury;
using MarcoERP.Domain.Entities.Treasury;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Treasury;

namespace MarcoERP.Application.Tests.Treasury
{
    public class BankAccountServiceTests
    {
        private readonly Mock<IBankAccountRepository> _repoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();
        private readonly Mock<IValidator<CreateBankAccountDto>> _createValidatorMock = new();
        private readonly Mock<IValidator<UpdateBankAccountDto>> _updateValidatorMock = new();

        public BankAccountServiceTests()
        {
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        private BankAccountService CreateService() => new BankAccountService(
            _repoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);

        private void SetupAuth(bool authenticated = true, bool hasPermission = true)
        {
            _currentUserMock.Setup(u => u.IsAuthenticated).Returns(authenticated);
            _currentUserMock.Setup(u => u.HasPermission(It.IsAny<string>())).Returns(hasPermission);
        }

        private void SetupValidValidation()
        {
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateBankAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateBankAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private static BankAccount CreateTestBankAccount(
            string code = "BNK-0001",
            string nameAr = "حساب بنك الرياض",
            string nameEn = "Riyad Bank")
        {
            return new BankAccount(code, nameAr, nameEn, "بنك الرياض", "SA1234", "SA0380000000608010167519", 1);
        }

        // =====================================================================
        // 1. Authorization Tests
        // =====================================================================

        [Fact]
        public async Task CreateAsync_NotAuthenticated_ReturnsFailure()
        {
            SetupAuth(authenticated: false);
            var service = CreateService();
            var dto = new CreateBankAccountDto { NameAr = "test" };

            var result = await service.CreateAsync(dto, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_NoPermission_ReturnsFailure()
        {
            SetupAuth(hasPermission: false);
            var service = CreateService();
            var dto = new CreateBankAccountDto { NameAr = "test" };

            var result = await service.CreateAsync(dto, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        // =====================================================================
        // 2. GetAllAsync Tests
        // =====================================================================

        [Fact]
        public async Task GetAllAsync_ReturnsAllBankAccounts()
        {
            var entities = new List<BankAccount> { CreateTestBankAccount() };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            var service = CreateService();

            var result = await service.GetAllAsync(CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetActiveAsync_ReturnsOnlyActive()
        {
            var entities = new List<BankAccount> { CreateTestBankAccount() };
            _repoMock.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            var service = CreateService();

            var result = await service.GetActiveAsync(CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        // =====================================================================
        // 3. GetByIdAsync Tests
        // =====================================================================

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsFailure()
        {
            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((BankAccount)null);
            var service = CreateService();

            var result = await service.GetByIdAsync(999, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_Found_ReturnsDto()
        {
            var entity = CreateTestBankAccount();
            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            var service = CreateService();

            var result = await service.GetByIdAsync(1, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.NameAr.Should().Be("حساب بنك الرياض");
        }

        // =====================================================================
        // 4. CreateAsync Tests
        // =====================================================================

        [Fact]
        public async Task CreateAsync_ValidationFails_ReturnsFailure()
        {
            SetupAuth();
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("NameAr", "الاسم العربي مطلوب")
            });
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateBankAccountDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            var service = CreateService();

            var result = await service.CreateAsync(new CreateBankAccountDto(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ValidData_ReturnsSuccess()
        {
            SetupAuth();
            SetupValidValidation();
            _repoMock.Setup(r => r.GetNextCodeAsync(It.IsAny<CancellationToken>())).ReturnsAsync("BNK-0001");
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<BankAccount>());
            var service = CreateService();
            var dto = new CreateBankAccountDto
            {
                NameAr = "حساب بنكي جديد",
                NameEn = "New Bank",
                BankName = "البنك الأهلي",
                AccountNumber = "12345",
                IBAN = "SA02000001",
                AccountId = 1
            };

            var result = await service.CreateAsync(dto, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // =====================================================================
        // 5. UpdateAsync Tests
        // =====================================================================

        [Fact]
        public async Task UpdateAsync_NotFound_ReturnsFailure()
        {
            SetupAuth();
            SetupValidValidation();
            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((BankAccount)null);
            var service = CreateService();

            var result = await service.UpdateAsync(new UpdateBankAccountDto { Id = 999, NameAr = "x" }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ValidData_ReturnsSuccess()
        {
            SetupAuth();
            SetupValidValidation();
            var entity = CreateTestBankAccount();
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            var service = CreateService();

            var result = await service.UpdateAsync(new UpdateBankAccountDto
            {
                Id = 1,
                NameAr = "اسم محدّث",
                NameEn = "Updated",
                BankName = "بنك",
                AccountNumber = "999",
                IBAN = "SA111",
                AccountId = 2
            }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // =====================================================================
        // 6. SetDefaultAsync Tests
        // =====================================================================

        [Fact]
        public async Task SetDefaultAsync_NotAuthenticated_ReturnsFailure()
        {
            SetupAuth(authenticated: false);
            var service = CreateService();

            var result = await service.SetDefaultAsync(1, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task SetDefaultAsync_NotFound_ReturnsFailure()
        {
            SetupAuth();
            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((BankAccount)null);
            var service = CreateService();

            var result = await service.SetDefaultAsync(999, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task SetDefaultAsync_ClearsOldDefault_SetsNew()
        {
            SetupAuth();
            var oldDefault = CreateTestBankAccount("BNK-0001");
            oldDefault.SetAsDefault();
            var newTarget = CreateTestBankAccount("BNK-0002");
            typeof(BankAccount).BaseType?.BaseType?.GetProperty("Id")?.SetValue(newTarget, 2);

            _repoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(newTarget);
            _repoMock.Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(oldDefault);
            var service = CreateService();

            var result = await service.SetDefaultAsync(2, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Update(It.IsAny<BankAccount>()), Times.AtLeast(1));
        }

        // =====================================================================
        // 7. Activate / Deactivate Tests
        // =====================================================================

        [Fact]
        public async Task ActivateAsync_NotFound_ReturnsFailure()
        {
            SetupAuth();
            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((BankAccount)null);
            var service = CreateService();

            var result = await service.ActivateAsync(999, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task DeactivateAsync_Success()
        {
            SetupAuth();
            var entity = CreateTestBankAccount();
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            var service = CreateService();

            var result = await service.DeactivateAsync(1, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
