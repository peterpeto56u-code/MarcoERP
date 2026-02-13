using System;
using System.Collections.Generic;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Treasury;

namespace MarcoERP.Domain.Entities.Treasury
{
    /// <summary>
    /// Represents a bank reconciliation session (تسوية بنكية).
    /// Tracks matching of bank statement lines to system GL transactions.
    /// </summary>
    public sealed class BankReconciliation : AuditableEntity
    {
        private readonly List<BankReconciliationItem> _items = new();

        /// <summary>EF Core only.</summary>
        private BankReconciliation() { }

        /// <summary>
        /// Creates a new bank reconciliation session.
        /// </summary>
        public BankReconciliation(
            int bankAccountId,
            DateTime reconciliationDate,
            decimal statementBalance,
            string notes = null)
        {
            if (bankAccountId <= 0)
                throw new TreasuryDomainException("الحساب البنكي مطلوب.");

            BankAccountId = bankAccountId;
            ReconciliationDate = reconciliationDate;
            StatementBalance = statementBalance;
            Notes = notes?.Trim();
            IsCompleted = false;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>FK to BankAccount.</summary>
        public int BankAccountId { get; private set; }

        /// <summary>Date of the reconciliation.</summary>
        public DateTime ReconciliationDate { get; private set; }

        /// <summary>Bank statement ending balance.</summary>
        public decimal StatementBalance { get; private set; }

        /// <summary>System (GL) balance at reconciliation date.</summary>
        public decimal SystemBalance { get; private set; }

        /// <summary>Difference = StatementBalance - SystemBalance - Adjustments.</summary>
        public decimal Difference { get; private set; }

        /// <summary>Whether reconciliation is finalized.</summary>
        public bool IsCompleted { get; private set; }

        /// <summary>Optional notes.</summary>
        public string Notes { get; private set; }

        /// <summary>Reconciliation line items.</summary>
        public IReadOnlyList<BankReconciliationItem> Items => _items.AsReadOnly();

        /// <summary>Navigation property to BankAccount.</summary>
        public BankAccount BankAccount { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates the reconciliation header.</summary>
        public void Update(DateTime reconciliationDate, decimal statementBalance, string notes)
        {
            if (IsCompleted)
                throw new TreasuryDomainException("لا يمكن تعديل تسوية مكتملة.");

            ReconciliationDate = reconciliationDate;
            StatementBalance = statementBalance;
            Notes = notes?.Trim();
        }

        /// <summary>Sets the system balance and computes difference.</summary>
        public void SetSystemBalance(decimal systemBalance)
        {
            SystemBalance = systemBalance;
            RecalculateDifference();
        }

        /// <summary>Adds a reconciliation item.</summary>
        public void AddItem(BankReconciliationItem item)
        {
            if (IsCompleted)
                throw new TreasuryDomainException("لا يمكن إضافة بنود لتسوية مكتملة.");
            _items.Add(item);
            RecalculateDifference();
        }

        /// <summary>Removes a reconciliation item.</summary>
        public void RemoveItem(BankReconciliationItem item)
        {
            if (IsCompleted)
                throw new TreasuryDomainException("لا يمكن حذف بنود من تسوية مكتملة.");
            _items.Remove(item);
            RecalculateDifference();
        }

        /// <summary>Marks the reconciliation as complete when balanced.</summary>
        public void Complete()
        {
            if (IsCompleted)
                throw new TreasuryDomainException("التسوية مكتملة بالفعل.");

            if (Difference != 0)
                throw new TreasuryDomainException(
                    $"لا يمكن إتمام التسوية والفرق لا يساوي صفراً (الفرق الحالي: {Difference:N2}).");

            IsCompleted = true;
        }

        /// <summary>Reopens a completed reconciliation.</summary>
        public void Reopen()
        {
            if (!IsCompleted)
                throw new TreasuryDomainException("التسوية غير مكتملة.");
            IsCompleted = false;
        }

        private void RecalculateDifference()
        {
            decimal adjustmentTotal = 0;
            foreach (var item in _items)
                adjustmentTotal += item.Amount;

            Difference = StatementBalance - SystemBalance - adjustmentTotal;
        }
    }
}
