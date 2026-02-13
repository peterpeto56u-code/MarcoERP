using System;
using System.Linq;
using FluentAssertions;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;
using Xunit;

namespace MarcoERP.Domain.Tests.Accounting
{
    public class JournalEntryTests
    {
        private static readonly DateTime TestDate = new DateTime(2026, 2, 1);

        private JournalEntry CreateValidDraft()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "اختبار قيد", SourceType.Manual, 1, 1);

            entry.AddLine(1, 1000m, 0m, TestDate, "مدين");
            entry.AddLine(2, 0m, 1000m, TestDate, "دائن");
            return entry;
        }

        // ── Factory Method ──────────────────────────────────────

        [Fact]
        public void CreateDraft_ValidParameters_CreatesDraftEntry()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد اختبار", SourceType.Manual, 1, 1);

            entry.Status.Should().Be(JournalEntryStatus.Draft);
            entry.JournalDate.Should().Be(TestDate);
            entry.Description.Should().Be("قيد اختبار");
            entry.SourceType.Should().Be(SourceType.Manual);
            entry.DraftCode.Should().StartWith("DRAFT-");
            entry.JournalNumber.Should().BeNull();
        }

        [Fact]
        public void CreateDraft_EmptyDescription_ThrowsException()
        {
            Action act = () => JournalEntry.CreateDraft(
                TestDate, "", SourceType.Manual, 1, 1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void CreateOpeningBalanceDraft_SetsSourceTypeToOpening()
        {
            var entry = JournalEntry.CreateOpeningBalanceDraft(
                TestDate, "أرصدة افتتاحية", 1, 1);
            entry.SourceType.Should().Be(SourceType.Opening);
        }

        [Fact]
        public void CreateAdjustment_SetsAdjustedEntryId()
        {
            var entry = JournalEntry.CreateAdjustment(
                TestDate, "تعديل", 1, 1, 42);
            entry.AdjustedEntryId.Should().Be(42);
        }

        // ── Line Management ─────────────────────────────────────

        [Fact]
        public void AddLine_ValidLine_AddsToCollection()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);

            entry.AddLine(1, 500m, 0m, TestDate, "مدين");
            entry.Lines.Count.Should().Be(1);
            entry.TotalDebit.Should().Be(500m);
        }

        [Fact]
        public void AddLine_NegativeAmount_ThrowsException()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);

            Action act = () => entry.AddLine(1, -100m, 0m, TestDate);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddLine_BothDebitAndCredit_ThrowsException()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);

            Action act = () => entry.AddLine(1, 100m, 100m, TestDate);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddLine_BothZero_ThrowsException()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);

            Action act = () => entry.AddLine(1, 0m, 0m, TestDate);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void RemoveLine_ExistingLine_RemovesAndRecalculates()
        {
            var entry = CreateValidDraft();
            var lineCount = entry.Lines.Count;
            entry.RemoveLine(1);
            entry.Lines.Count.Should().Be(lineCount - 1);
        }

        [Fact]
        public void RemoveLine_NonExistentLine_ThrowsException()
        {
            var entry = CreateValidDraft();
            Action act = () => entry.RemoveLine(999);
            act.Should().Throw<Exception>();
        }

        // ── Validate ────────────────────────────────────────────

        [Fact]
        public void Validate_BalancedEntry_ReturnsNoErrors()
        {
            var entry = CreateValidDraft();
            entry.Validate().Should().BeEmpty();
        }

        [Fact]
        public void Validate_UnbalancedEntry_ReturnsErrors()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);
            entry.AddLine(1, 1000m, 0m, TestDate);
            entry.AddLine(2, 0m, 500m, TestDate);

            entry.Validate().Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_SingleLine_ReturnsErrors()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);
            entry.AddLine(1, 1000m, 0m, TestDate);

            entry.Validate().Should().NotBeEmpty();
        }

        // ── Post ────────────────────────────────────────────────

        [Fact]
        public void Post_ValidDraft_ChangesStatusToPosted()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-2026-00001", "admin", TestDate);

            entry.Status.Should().Be(JournalEntryStatus.Posted);
            entry.JournalNumber.Should().Be("JV-2026-00001");
            entry.PostedBy.Should().Be("admin");
            entry.PostingDate.Should().NotBeNull();
        }

        [Fact]
        public void Post_AlreadyPosted_ThrowsException()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-2026-00001", "admin", TestDate);

            Action act = () => entry.Post("JV-2026-00002", "admin", TestDate);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Post_UnbalancedEntry_ThrowsException()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);
            entry.AddLine(1, 1000m, 0m, TestDate);
            entry.AddLine(2, 0m, 500m, TestDate);

            Action act = () => entry.Post("JV-001", "admin", TestDate);
            act.Should().Throw<Exception>();
        }

        // ── Reversal ────────────────────────────────────────────

        [Fact]
        public void CreateReversal_PostedEntry_CreatesMirrorEntry()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-2026-00001", "admin", TestDate);

            var reversal = entry.CreateReversal(TestDate, "خطأ في القيد", 1, 1);

            reversal.Should().NotBeNull();
            reversal.Status.Should().Be(JournalEntryStatus.Draft);
            reversal.Lines.Count.Should().Be(entry.Lines.Count);

            var originalLine = entry.Lines.First();
            var reversalLine = reversal.Lines.First();
            reversalLine.DebitAmount.Should().Be(originalLine.CreditAmount);
            reversalLine.CreditAmount.Should().Be(originalLine.DebitAmount);
        }

        [Fact]
        public void CreateReversal_DraftEntry_ThrowsException()
        {
            var entry = CreateValidDraft();
            Action act = () => entry.CreateReversal(TestDate, "سبب", 1, 1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void CreateReversal_EmptyReason_ThrowsException()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-001", "admin", TestDate);
            Action act = () => entry.CreateReversal(TestDate, "", 1, 1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void MarkAsReversed_PostedEntry_ChangesStatusToReversed()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-2026-00001", "admin", TestDate);
            entry.MarkAsReversed(99);

            entry.Status.Should().Be(JournalEntryStatus.Reversed);
            entry.ReversalEntryId.Should().Be(99);
        }

        // ── Soft Delete ─────────────────────────────────────────

        [Fact]
        public void SoftDelete_DraftEntry_Succeeds()
        {
            var entry = CreateValidDraft();
            entry.SoftDelete("admin", TestDate);
            entry.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void SoftDelete_PostedEntry_ThrowsException()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-001", "admin", TestDate);

            Action act = () => entry.SoftDelete("admin", TestDate);
            act.Should().Throw<Exception>();
        }

        // ── UpdateDraft ─────────────────────────────────────────

        [Fact]
        public void UpdateDraft_ValidUpdate_UpdatesFields()
        {
            var entry = CreateValidDraft();
            entry.UpdateDraft("وصف محدّث", "REF-001");
            entry.Description.Should().Be("وصف محدّث");
            entry.ReferenceNumber.Should().Be("REF-001");
        }

        [Fact]
        public void UpdateDraft_EmptyDescription_ThrowsException()
        {
            var entry = CreateValidDraft();
            Action act = () => entry.UpdateDraft("");
            act.Should().Throw<Exception>();
        }

        // ── Totals & Events ─────────────────────────────────────

        [Fact]
        public void TotalDebit_SumOfAllDebitLines()
        {
            var entry = JournalEntry.CreateDraft(
                TestDate, "قيد", SourceType.Manual, 1, 1);
            entry.AddLine(1, 300m, 0m, TestDate);
            entry.AddLine(2, 200m, 0m, TestDate);
            entry.AddLine(3, 0m, 500m, TestDate);

            entry.TotalDebit.Should().Be(500m);
            entry.TotalCredit.Should().Be(500m);
        }

        [Fact]
        public void DomainEvents_AfterPost_ContainsPostedEvent()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-001", "admin", TestDate);
            entry.DomainEvents.Should().NotBeEmpty();
        }

        [Fact]
        public void ClearDomainEvents_AfterPost_ClearsEvents()
        {
            var entry = CreateValidDraft();
            entry.Post("JV-001", "admin", TestDate);
            entry.ClearDomainEvents();
            entry.DomainEvents.Should().BeEmpty();
        }
    }
}
