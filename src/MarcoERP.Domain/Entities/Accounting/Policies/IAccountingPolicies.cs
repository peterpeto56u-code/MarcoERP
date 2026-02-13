using System;

namespace MarcoERP.Domain.Entities.Accounting.Policies
{
    /// <summary>
    /// Policy interface for account validation during posting.
    /// Implemented in the Application/Persistence layer.
    /// </summary>
    public interface IAccountPostingPolicy
    {
        /// <summary>Loads an account by its Id.</summary>
        Account GetAccount(int accountId);

        /// <summary>Returns true if the account is a leaf (no children).</summary>
        bool IsLeaf(int accountId);

        /// <summary>Marks the account as having been used in a posting.</summary>
        void MarkAsUsed(int accountId);
    }

    /// <summary>
    /// Policy interface for fiscal period/year validation.
    /// </summary>
    public interface IFiscalPeriodPolicy
    {
        /// <summary>Returns true if the period is open for posting.</summary>
        bool IsPeriodOpen(int fiscalYearId, int fiscalPeriodId);

        /// <summary>Returns true if the fiscal year is still active (not closed).</summary>
        bool IsYearOpen(int fiscalYearId);

        /// <summary>Returns true if the period is locked.</summary>
        bool IsPeriodLocked(int fiscalYearId, int fiscalPeriodId);
    }

    /// <summary>
    /// Generates sequential journal numbers per fiscal year.
    /// Implemented in Infrastructure layer using DB SEQUENCE.
    /// </summary>
    public interface IJournalNumberGenerator
    {
        /// <summary>Returns the next sequential journal number for the given fiscal year.</summary>
        string NextNumber(int fiscalYearId);
    }

    /// <summary>
    /// Optimistic concurrency guard.
    /// </summary>
    public interface IConcurrencyGuard
    {
        /// <summary>Throws if current and expected row versions do not match.</summary>
        void EnsureRowVersionMatches(byte[] current, byte[] expected);
    }

    /// <summary>
    /// Hook for VAT calculation during posting.
    /// </summary>
    public interface IVatCalculationHook
    {
        /// <summary>Applies VAT lines to the journal entry if needed.</summary>
        void ApplyVat(JournalEntry entry);
    }

    /// <summary>
    /// Hook for inventory integration during posting (COGS auto-generation).
    /// </summary>
    public interface IInventoryIntegrationHook
    {
        /// <summary>Called before posting — validates stock availability.</summary>
        void OnBeforePost(JournalEntry entry);

        /// <summary>Called after posting — updates inventory balances.</summary>
        void OnAfterPost(JournalEntry entry);
    }

    /// <summary>
    /// Hook for year-end closing operations.
    /// </summary>
    public interface IYearEndClosingHook
    {
        /// <summary>Creates the retained earnings closing entry.</summary>
        JournalEntry CreateRetainedEarningsEntry(int fiscalYearId, int fiscalPeriodId, DateTime date);
    }
}
