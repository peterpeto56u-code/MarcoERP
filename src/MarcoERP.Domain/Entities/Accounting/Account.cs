using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Accounting;

namespace MarcoERP.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a single account in the Chart of Accounts.
    /// Uses Egyptian-style 4-digit hierarchical numbering (Levels 1–4).
    /// Only leaf accounts with AllowPosting = true may receive journal postings.
    /// </summary>
    public sealed class Account : SoftDeletableEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Account() { }

        /// <summary>
        /// Creates a new Account with full invariant validation.
        /// </summary>
        public Account(
            string accountCode,
            string accountNameAr,
            string accountNameEn,
            AccountType accountType,
            int? parentAccountId,
            int level,
            bool isSystemAccount,
            string currencyCode,
            string description = null)
        {
            // ── ACC-INV-01: AccountCode must be exactly 4 numeric characters ──
            ValidateAccountCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountNameAr))
                throw new AccountDomainException("اسم الحساب بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
                throw new AccountDomainException("Currency code must be a 3-letter ISO 4217 code.");

            // ── ACC-INV-04: Level must be 1–4 ──
            if (level < 1 || level > 4)
                throw new AccountDomainException("مستوى الحساب يجب أن يكون بين 1 و 4.");

            // ── ACC-INV-05 / ACC-INV-06: Parent validation ──
            if (level == 1 && parentAccountId.HasValue)
                throw new AccountDomainException("حسابات المستوى الأول لا يمكن أن يكون لها حساب أب.");

            if (level > 1 && !parentAccountId.HasValue)
                throw new AccountDomainException("الحسابات أسفل المستوى الأول يجب أن يكون لها حساب أب.");

            AccountCode = accountCode.Trim();
            AccountNameAr = accountNameAr.Trim();
            AccountNameEn = accountNameEn?.Trim();
            AccountType = accountType;
            NormalBalance = DeriveNormalBalance(accountType);
            ParentAccountId = parentAccountId;
            Level = level;
            IsLeaf = true;           // New accounts start as leaves
            AllowPosting = (level == 4); // Only level 4 can post; others are categories
            IsActive = true;
            IsSystemAccount = isSystemAccount;
            CurrencyCode = currencyCode.Trim().ToUpperInvariant();
            Description = description?.Trim();
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique 4-digit hierarchical code (e.g., "1111").</summary>
        public string AccountCode { get; private set; }

        /// <summary>Arabic account name (required).</summary>
        public string AccountNameAr { get; private set; }

        /// <summary>English account name (optional).</summary>
        public string AccountNameEn { get; private set; }

        /// <summary>Account classification.</summary>
        public AccountType AccountType { get; private set; }

        /// <summary>
        /// ACC-INV-03: Derived from AccountType — never set independently.
        /// </summary>
        public NormalBalance NormalBalance { get; private set; }

        /// <summary>FK to parent account (null for Level 1 root accounts).</summary>
        public int? ParentAccountId { get; private set; }

        /// <summary>Navigation property to parent account.</summary>
        public Account ParentAccount { get; private set; }

        /// <summary>Hierarchy depth: 1 (Category), 2 (Group), 3 (Sub-group), 4 (Leaf).</summary>
        public int Level { get; private set; }

        /// <summary>ACC-INV-09: True if no children exist.</summary>
        public bool IsLeaf { get; private set; }

        /// <summary>ACC-INV-07: True only when IsLeaf = true. Only these accounts receive postings.</summary>
        public bool AllowPosting { get; private set; }

        /// <summary>ACC-INV-11: Deactivated accounts cannot receive new postings.</summary>
        public bool IsActive { get; private set; }

        /// <summary>ACC-INV-08: System accounts cannot be deleted or have type changed.</summary>
        public bool IsSystemAccount { get; private set; }

        /// <summary>ISO 4217 currency code (system-wide single currency).</summary>
        public string CurrencyCode { get; private set; }

        /// <summary>Optional description.</summary>
        public string Description { get; private set; }

        /// <summary>Tracks if the account has been used in journal postings.</summary>
        public bool HasPostings { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Derives normal balance from account type per ACCOUNTING_PRINCIPLES §2.1.
        /// </summary>
        public static NormalBalance DeriveNormalBalance(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Asset => NormalBalance.Debit,
                AccountType.COGS => NormalBalance.Debit,
                AccountType.Expense => NormalBalance.Debit,
                AccountType.OtherExpense => NormalBalance.Debit,
                AccountType.Liability => NormalBalance.Credit,
                AccountType.Equity => NormalBalance.Credit,
                AccountType.Revenue => NormalBalance.Credit,
                AccountType.OtherIncome => NormalBalance.Credit,
                _ => throw new AccountDomainException($"Unknown account type: {accountType}.")
            };
        }

        /// <summary>
        /// Returns true if this account can receive journal postings.
        /// POST-01, POST-02, POST-03.
        /// </summary>
        public bool CanReceivePostings()
        {
            return IsActive && IsLeaf && AllowPosting && !IsDeleted;
        }

        /// <summary>
        /// ACC-INV-10: Deactivates the account. System accounts cannot be deactivated.
        /// </summary>
        public void Deactivate()
        {
            if (IsSystemAccount)
                throw new AccountDomainException("لا يمكن تعطيل حسابات النظام.");

            IsActive = false;
        }

        /// <summary>
        /// Reactivates a previously deactivated account.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Changes the Arabic name. System account names cannot be changed.
        /// </summary>
        public void ChangeNameAr(string newNameAr)
        {
            if (string.IsNullOrWhiteSpace(newNameAr))
                throw new AccountDomainException("اسم الحساب بالعربي مطلوب.");

            if (IsSystemAccount)
                throw new AccountDomainException("لا يمكن تغيير اسم حسابات النظام.");

            AccountNameAr = newNameAr.Trim();
        }

        /// <summary>
        /// Changes the English name.
        /// </summary>
        public void ChangeNameEn(string newNameEn)
        {
            AccountNameEn = newNameEn?.Trim();
        }

        /// <summary>
        /// Changes the description.
        /// </summary>
        public void ChangeDescription(string newDescription)
        {
            Description = newDescription?.Trim();
        }

        /// <summary>
        /// ACC-INV-02: Account type cannot change once any posting references this account.
        /// ACC-INV-08: System accounts cannot have their type changed.
        /// </summary>
        public void ChangeType(AccountType newType)
        {
            if (IsSystemAccount)
                throw new AccountDomainException("لا يمكن تغيير نوع حسابات النظام.");

            if (HasPostings)
                throw new AccountDomainException("لا يمكن تغيير نوع الحساب بعد وجود قيود مسجلة عليه.");

            AccountType = newType;
            NormalBalance = DeriveNormalBalance(newType);
        }

        /// <summary>
        /// HIER-05: When this leaf gets its first child, it becomes a parent.
        /// AllowPosting flips to false.
        /// </summary>
        public void MarkAsParent()
        {
            IsLeaf = false;
            AllowPosting = false;
        }

        /// <summary>
        /// Marks that this account has been used in at least one posted journal entry.
        /// </summary>
        public void MarkAsUsed()
        {
            HasPostings = true;
        }

        /// <summary>
        /// Override soft delete — system accounts and accounts with postings cannot be deleted.
        /// ACC-INV-08, ACC-INV-10.
        /// </summary>
        public override void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            if (IsSystemAccount)
                throw new AccountDomainException("لا يمكن حذف حسابات النظام.");

            if (HasPostings)
                throw new AccountDomainException("لا يمكن حذف حساب عليه قيود مسجلة — يمكن تعطيله فقط.");

            if (!IsLeaf)
                throw new AccountDomainException("لا يمكن حذف حساب له حسابات فرعية.");

            base.SoftDelete(deletedBy, deletedAt);
        }

        // ── Validation Helpers ──────────────────────────────────

        /// <summary>
        /// ACC-INV-01: Validates that code is exactly 4 numeric characters.
        /// </summary>
        private static void ValidateAccountCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new AccountDomainException("كود الحساب مطلوب.");

            var trimmed = code.Trim();
            if (trimmed.Length != 4)
                throw new AccountDomainException("كود الحساب يجب أن يكون 4 أرقام بالضبط.");

            foreach (var ch in trimmed)
            {
                if (ch < '0' || ch > '9')
                    throw new AccountDomainException("كود الحساب يجب أن يحتوي على أرقام فقط.");
            }
        }

        /// <summary>
        /// Validates that the child code falls within the parent's numeric range.
        /// HIER-03: A child's AccountCode must fall within the parent's numeric range.
        /// </summary>
        public static void ValidateChildCode(string parentCode, string childCode, int parentLevel)
        {
            if (string.IsNullOrWhiteSpace(parentCode) || string.IsNullOrWhiteSpace(childCode))
                throw new AccountDomainException("Parent and child codes are required for validation.");

            ValidateAccountCode(parentCode);
            ValidateAccountCode(childCode);

            // The child's code must start with the parent's significant digits
            // Level 1 (X000): child must be X_00 pattern (e.g., parent 1000 → child 1100, 1200)
            // Level 2 (XX00): child must be XX_0 pattern (e.g., parent 1100 → child 1110, 1120)
            // Level 3 (XXX0): child must be XXX_ pattern (e.g., parent 1110 → child 1111, 1112)

            switch (parentLevel)
            {
                case 1:
                    if (childCode[0] != parentCode[0])
                        throw new AccountDomainException($"كود الحساب الفرعي {childCode} لا يقع ضمن نطاق الحساب الأب {parentCode}.");
                    break;
                case 2:
                    if (childCode[0] != parentCode[0] || childCode[1] != parentCode[1])
                        throw new AccountDomainException($"كود الحساب الفرعي {childCode} لا يقع ضمن نطاق الحساب الأب {parentCode}.");
                    break;
                case 3:
                    if (childCode[0] != parentCode[0] || childCode[1] != parentCode[1] || childCode[2] != parentCode[2])
                        throw new AccountDomainException($"كود الحساب الفرعي {childCode} لا يقع ضمن نطاق الحساب الأب {parentCode}.");
                    break;
                default:
                    throw new AccountDomainException("لا يمكن إضافة حسابات فرعية تحت المستوى الرابع.");
            }
        }

        /// <summary>
        /// Returns true if the account type is a balance sheet type (carries forward at year-end).
        /// </summary>
        public static bool IsBalanceSheetType(AccountType accountType)
        {
            return accountType == AccountType.Asset
                || accountType == AccountType.Liability
                || accountType == AccountType.Equity;
        }

        /// <summary>
        /// Returns true if the account type is an income statement type (closed at year-end).
        /// </summary>
        public static bool IsIncomeStatementType(AccountType accountType)
        {
            return accountType == AccountType.Revenue
                || accountType == AccountType.COGS
                || accountType == AccountType.Expense
                || accountType == AccountType.OtherIncome
                || accountType == AccountType.OtherExpense;
        }
    }
}
