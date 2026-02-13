using System;
using System.Linq;
using FluentAssertions;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;
using Xunit;

namespace MarcoERP.Domain.Tests.Accounting
{
    public class AccountTests
    {
        // Constructor: (code, nameAr, nameEn, type, parentId, level, isSystem, currencyCode, description)
        // AllowPosting = (level == 4)

        private Account CreateLeafAccount()
        {
            return new Account("1111", "نقدية", "Cash", AccountType.Asset, 3, 4, false, "SAR");
        }

        private Account CreateLevel1Account()
        {
            return new Account("1000", "أصول", "Assets", AccountType.Asset, null, 1, false, "SAR");
        }

        [Fact]
        public void Constructor_ValidLevel4_CreatesPostableLeaf()
        {
            var account = CreateLeafAccount();
            account.AccountCode.Should().Be("1111");
            account.AccountNameAr.Should().Be("نقدية");
            account.AccountNameEn.Should().Be("Cash");
            account.AccountType.Should().Be(AccountType.Asset);
            account.NormalBalance.Should().Be(NormalBalance.Debit);
            account.Level.Should().Be(4);
            account.IsLeaf.Should().BeTrue();
            account.AllowPosting.Should().BeTrue();
            account.IsActive.Should().BeTrue();
            account.CurrencyCode.Should().Be("SAR");
        }

        [Fact]
        public void Constructor_Level1_NotPostable()
        {
            var account = CreateLevel1Account();
            account.Level.Should().Be(1);
            account.AllowPosting.Should().BeFalse();
        }

        [Theory]
        [InlineData(AccountType.Asset, NormalBalance.Debit)]
        [InlineData(AccountType.COGS, NormalBalance.Debit)]
        [InlineData(AccountType.Expense, NormalBalance.Debit)]
        [InlineData(AccountType.OtherExpense, NormalBalance.Debit)]
        [InlineData(AccountType.Liability, NormalBalance.Credit)]
        [InlineData(AccountType.Equity, NormalBalance.Credit)]
        [InlineData(AccountType.Revenue, NormalBalance.Credit)]
        [InlineData(AccountType.OtherIncome, NormalBalance.Credit)]
        public void DeriveNormalBalance_AllTypes_CorrectBalance(AccountType type, NormalBalance expected)
        {
            Account.DeriveNormalBalance(type).Should().Be(expected);
        }

        [Fact]
        public void CanReceivePostings_ActiveLeafLevel4_ReturnsTrue()
        {
            var account = CreateLeafAccount();
            account.CanReceivePostings().Should().BeTrue();
        }

        [Fact]
        public void CanReceivePostings_InactiveAccount_ReturnsFalse()
        {
            var account = CreateLeafAccount();
            account.Deactivate();
            account.CanReceivePostings().Should().BeFalse();
        }

        [Fact]
        public void CanReceivePostings_ParentAccount_ReturnsFalse()
        {
            var account = CreateLeafAccount();
            account.MarkAsParent();
            account.CanReceivePostings().Should().BeFalse();
        }

        [Fact]
        public void Deactivate_NormalAccount_Succeeds()
        {
            var account = CreateLeafAccount();
            account.Deactivate();
            account.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Deactivate_SystemAccount_ThrowsException()
        {
            var account = new Account("1111", "نقدية", "Cash", AccountType.Asset, 3, 4, true, "SAR");
            Action act = () => account.Deactivate();
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Activate_DeactivatedAccount_Succeeds()
        {
            var account = CreateLeafAccount();
            account.Deactivate();
            account.Activate();
            account.IsActive.Should().BeTrue();
        }

        [Fact]
        public void MarkAsParent_SetsIsLeafFalseAndAllowPostingFalse()
        {
            var account = CreateLeafAccount();
            account.MarkAsParent();
            account.IsLeaf.Should().BeFalse();
            account.AllowPosting.Should().BeFalse();
        }

        [Fact]
        public void MarkAsUsed_SetsHasPostingsTrue()
        {
            var account = CreateLeafAccount();
            account.MarkAsUsed();
            account.HasPostings.Should().BeTrue();
        }

        [Fact]
        public void ChangeType_WithPostings_ThrowsException()
        {
            var account = CreateLeafAccount();
            account.MarkAsUsed();
            Action act = () => account.ChangeType(AccountType.Liability);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChangeType_SystemAccount_ThrowsException()
        {
            var account = new Account("1111", "نقدية", "Cash", AccountType.Asset, 3, 4, true, "SAR");
            Action act = () => account.ChangeType(AccountType.Liability);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChangeType_NoPostings_UpdatesTypeAndNormalBalance()
        {
            var account = CreateLeafAccount();
            account.ChangeType(AccountType.Liability);
            account.AccountType.Should().Be(AccountType.Liability);
            account.NormalBalance.Should().Be(NormalBalance.Credit);
        }

        // Test ValidateAccountCode through the constructor
        [Fact]
        public void Constructor_EmptyCode_ThrowsException()
        {
            Action act = () => new Account("", "نقدية", "Cash", AccountType.Asset, null, 1, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_NonNumericCode_ThrowsException()
        {
            Action act = () => new Account("abcd", "نقدية", "Cash", AccountType.Asset, null, 1, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_ShortCode_ThrowsException()
        {
            Action act = () => new Account("12", "نقدية", "Cash", AccountType.Asset, null, 1, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_LongCode_ThrowsException()
        {
            Action act = () => new Account("12345", "نقدية", "Cash", AccountType.Asset, null, 1, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_EmptyNameAr_ThrowsException()
        {
            Action act = () => new Account("1111", "", "Cash", AccountType.Asset, 3, 4, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_InvalidCurrencyCode_ThrowsException()
        {
            Action act = () => new Account("1111", "نقدية", "Cash", AccountType.Asset, 3, 4, false, "X");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_Level1WithParent_ThrowsException()
        {
            Action act = () => new Account("1000", "أصول", "Assets", AccountType.Asset, 99, 1, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_Level2WithoutParent_ThrowsException()
        {
            Action act = () => new Account("1100", "أصول متداولة", "Current Assets", AccountType.Asset, null, 2, false, "SAR");
            act.Should().Throw<Exception>();
        }

        [Theory]
        [InlineData(AccountType.Asset, true)]
        [InlineData(AccountType.Liability, true)]
        [InlineData(AccountType.Equity, true)]
        [InlineData(AccountType.Revenue, false)]
        [InlineData(AccountType.Expense, false)]
        [InlineData(AccountType.COGS, false)]
        public void IsBalanceSheetType_CorrectClassification(AccountType type, bool expected)
        {
            Account.IsBalanceSheetType(type).Should().Be(expected);
        }

        [Theory]
        [InlineData(AccountType.Revenue, true)]
        [InlineData(AccountType.Expense, true)]
        [InlineData(AccountType.COGS, true)]
        [InlineData(AccountType.OtherIncome, true)]
        [InlineData(AccountType.OtherExpense, true)]
        [InlineData(AccountType.Asset, false)]
        [InlineData(AccountType.Liability, false)]
        public void IsIncomeStatementType_CorrectClassification(AccountType type, bool expected)
        {
            Account.IsIncomeStatementType(type).Should().Be(expected);
        }

        [Fact]
        public void ChangeNameAr_ValidName_Updates()
        {
            var account = CreateLeafAccount();
            account.ChangeNameAr("بنك");
            account.AccountNameAr.Should().Be("بنك");
        }

        [Fact]
        public void ChangeNameAr_EmptyName_ThrowsException()
        {
            var account = CreateLeafAccount();
            Action act = () => account.ChangeNameAr("");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChangeNameAr_SystemAccount_ThrowsException()
        {
            var account = new Account("1111", "نقدية", "Cash", AccountType.Asset, 3, 4, true, "SAR");
            Action act = () => account.ChangeNameAr("اسم جديد");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SoftDelete_SystemAccount_ThrowsException()
        {
            var account = new Account("1111", "نقدية", "Cash", AccountType.Asset, 3, 4, true, "SAR");
            Action act = () => account.SoftDelete("admin", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SoftDelete_AccountWithPostings_ThrowsException()
        {
            var account = CreateLeafAccount();
            account.MarkAsUsed();
            Action act = () => account.SoftDelete("admin", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SoftDelete_ParentAccount_ThrowsException()
        {
            var account = CreateLeafAccount();
            account.MarkAsParent();
            Action act = () => account.SoftDelete("admin", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SoftDelete_LeafNoPostings_Succeeds()
        {
            var account = CreateLeafAccount();
            account.SoftDelete("admin", DateTime.UtcNow);
            account.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void ValidateChildCode_ValidChild_DoesNotThrow()
        {
            Action act = () => Account.ValidateChildCode("1000", "1100", 1);
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateChildCode_OutOfRange_ThrowsException()
        {
            Action act = () => Account.ValidateChildCode("1000", "2100", 1);
            act.Should().Throw<Exception>();
        }
    }
}
