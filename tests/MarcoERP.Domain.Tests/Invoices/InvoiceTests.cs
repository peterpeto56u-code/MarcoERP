using System;
using FluentAssertions;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Entities.Purchases;
using MarcoERP.Domain.Enums;
using Xunit;

namespace MarcoERP.Domain.Tests.Invoices
{
    public class SalesInvoiceTests
    {
        private SalesInvoice CreateValidDraftInvoice()
        {
            return new SalesInvoice("SI-001", new DateTime(2026, 2, 1), 1, 1, "ملاحظات");
        }

        // ── Constructor ─────────────────────────────────────────

        [Fact]
        public void Constructor_ValidParameters_CreatesDraftInvoice()
        {
            var inv = CreateValidDraftInvoice();
            inv.InvoiceNumber.Should().Be("SI-001");
            inv.Status.Should().Be(InvoiceStatus.Draft);
            inv.CustomerId.Should().Be(1);
            inv.WarehouseId.Should().Be(1);
            inv.NetTotal.Should().Be(0m);
        }

        [Fact]
        public void Constructor_EmptyNumber_ThrowsException()
        {
            Action act = () => new SalesInvoice("", DateTime.Now, 1, 1, null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_ZeroCustomerId_ThrowsException()
        {
            Action act = () => new SalesInvoice("SI-001", DateTime.Now, 0, 1, null);
            act.Should().Throw<Exception>();
        }

        // ── AddLine ─────────────────────────────────────────────

        [Fact]
        public void AddLine_ValidLine_AddsAndRecalculates()
        {
            var inv = CreateValidDraftInvoice();
            var line = inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);

            inv.Lines.Count.Should().Be(1);
            line.Should().NotBeNull();
            inv.Subtotal.Should().BeGreaterThan(0);
        }

        [Fact]
        public void AddLine_ZeroQuantity_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            Action act = () => inv.AddLine(1, 1, 0m, 100m, 1m, 0m, 15m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddLine_NegativeUnitPrice_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            Action act = () => inv.AddLine(1, 1, 5m, -100m, 1m, 0m, 15m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddLine_DiscountOver100_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            Action act = () => inv.AddLine(1, 1, 5m, 100m, 1m, 101m, 15m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddLine_PostedInvoice_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);
            inv.Post(1, 2);
            Action act = () => inv.AddLine(2, 1, 3m, 50m, 1m, 0m, 15m);
            act.Should().Throw<Exception>();
        }

        // ── RemoveLine ──────────────────────────────────────────

        [Fact]
        public void RemoveLine_ExistingLine_Removes()
        {
            var inv = CreateValidDraftInvoice();
            var line = inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);
            inv.RemoveLine(line);
            inv.Lines.Count.Should().Be(0);
            inv.NetTotal.Should().Be(0m);
        }

        // ── UpdateHeader ────────────────────────────────────────

        [Fact]
        public void UpdateHeader_DraftInvoice_UpdatesFields()
        {
            var inv = CreateValidDraftInvoice();
            inv.UpdateHeader(new DateTime(2026, 3, 1), 2, 2, "ملاحظات جديدة");
            inv.InvoiceDate.Should().Be(new DateTime(2026, 3, 1));
            inv.CustomerId.Should().Be(2);
            inv.WarehouseId.Should().Be(2);
        }

        [Fact]
        public void UpdateHeader_PostedInvoice_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);
            inv.Post(1, 2);
            Action act = () => inv.UpdateHeader(DateTime.Now, 2, 2, null);
            act.Should().Throw<Exception>();
        }

        // ── Post ────────────────────────────────────────────────

        [Fact]
        public void Post_DraftWithLines_ChangesToPosted()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);
            inv.Post(1, 2);
            inv.Status.Should().Be(InvoiceStatus.Posted);
            inv.JournalEntryId.Should().Be(1);
            inv.CogsJournalEntryId.Should().Be(2);
        }

        [Fact]
        public void Post_DraftWithoutLines_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            Action act = () => inv.Post(1, 2);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Post_AlreadyPosted_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);
            inv.Post(1, 2);
            Action act = () => inv.Post(3, 4);
            act.Should().Throw<Exception>();
        }

        // ── Cancel ──────────────────────────────────────────────

        [Fact]
        public void Cancel_PostedInvoice_ChangesToCancelled()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);
            inv.Post(1, 2);
            inv.Cancel();
            inv.Status.Should().Be(InvoiceStatus.Cancelled);
        }

        [Fact]
        public void Cancel_DraftInvoice_ThrowsException()
        {
            var inv = CreateValidDraftInvoice();
            Action act = () => inv.Cancel();
            act.Should().Throw<Exception>();
        }

        // ── ReplaceLines ────────────────────────────────────────

        [Fact]
        public void ReplaceLines_DraftInvoice_ReplacesAllLines()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 5m, 100m, 1m, 0m, 15m);

            var newLine = new SalesInvoiceLine(2, 1, 10m, 200m, 1m, 5m, 15m);
            inv.ReplaceLines(new[] { newLine });

            inv.Lines.Count.Should().Be(1);
        }

        // ── Totals Calculation ──────────────────────────────────

        [Fact]
        public void Totals_MultipleLines_CalculatedCorrectly()
        {
            var inv = CreateValidDraftInvoice();
            inv.AddLine(1, 1, 10m, 100m, 1m, 10m, 15m);
            // Subtotal per line: 10*100 = 1000
            // Discount: 1000 * 10% = 100
            // After discount: 900
            // VAT: 900 * 15% = 135
            // Net: 900 + 135 = 1035

            inv.Subtotal.Should().Be(1000m);
            inv.DiscountTotal.Should().Be(100m);
            inv.VatTotal.Should().Be(135m);
            inv.NetTotal.Should().Be(1035m);
        }
    }

    public class PurchaseInvoiceTests
    {
        private PurchaseInvoice CreateValidDraft()
        {
            return new PurchaseInvoice("PI-001", new DateTime(2026, 2, 1), 1, 1, "ملاحظات");
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesDraft()
        {
            var inv = CreateValidDraft();
            inv.InvoiceNumber.Should().Be("PI-001");
            inv.Status.Should().Be(InvoiceStatus.Draft);
            inv.SupplierId.Should().Be(1);
        }

        [Fact]
        public void Constructor_EmptyNumber_ThrowsException()
        {
            Action act = () => new PurchaseInvoice("", DateTime.Now, 1, 1, null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddLine_ValidLine_AddsSuccessfully()
        {
            var inv = CreateValidDraft();
            var line = inv.AddLine(1, 1, 5m, 200m, 1m, 0m, 15m);
            inv.Lines.Count.Should().Be(1);
            line.Should().NotBeNull();
        }

        [Fact]
        public void Post_DraftWithLines_ChangesToPosted()
        {
            var inv = CreateValidDraft();
            inv.AddLine(1, 1, 5m, 200m, 1m, 0m, 15m);
            inv.Post(1); // Single JE ID for purchase
            inv.Status.Should().Be(InvoiceStatus.Posted);
            inv.JournalEntryId.Should().Be(1);
        }

        [Fact]
        public void Post_EmptyLines_ThrowsException()
        {
            var inv = CreateValidDraft();
            Action act = () => inv.Post(1);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Cancel_PostedInvoice_ChangesToCancelled()
        {
            var inv = CreateValidDraft();
            inv.AddLine(1, 1, 5m, 200m, 1m, 0m, 15m);
            inv.Post(1);
            inv.Cancel();
            inv.Status.Should().Be(InvoiceStatus.Cancelled);
        }

        [Fact]
        public void UpdateHeader_PostedInvoice_ThrowsException()
        {
            var inv = CreateValidDraft();
            inv.AddLine(1, 1, 5m, 200m, 1m, 0m, 15m);
            inv.Post(1);
            Action act = () => inv.UpdateHeader(DateTime.Now, 2, 2, null);
            act.Should().Throw<Exception>();
        }
    }

    public class SalesReturnTests
    {
        private SalesReturn CreateValidDraft()
        {
            return new SalesReturn("SR-001", new DateTime(2026, 2, 1), 1, 1, null, "ملاحظات");
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesDraft()
        {
            var ret = CreateValidDraft();
            ret.ReturnNumber.Should().Be("SR-001");
            ret.Status.Should().Be(InvoiceStatus.Draft);
        }

        [Fact]
        public void AddLine_ValidLine_AddsSuccessfully()
        {
            var ret = CreateValidDraft();
            ret.AddLine(1, 1, 3m, 100m, 1m, 0m, 15m);
            ret.Lines.Count.Should().Be(1);
        }

        [Fact]
        public void Post_DraftWithLines_ChangesToPosted()
        {
            var ret = CreateValidDraft();
            ret.AddLine(1, 1, 3m, 100m, 1m, 0m, 15m);
            ret.Post(1, 2);
            ret.Status.Should().Be(InvoiceStatus.Posted);
        }

        [Fact]
        public void Cancel_PostedReturn_ChangesToCancelled()
        {
            var ret = CreateValidDraft();
            ret.AddLine(1, 1, 3m, 100m, 1m, 0m, 15m);
            ret.Post(1, 2);
            ret.Cancel();
            ret.Status.Should().Be(InvoiceStatus.Cancelled);
        }

        [Fact]
        public void UpdateHeader_WithOriginalInvoice_UpdatesReference()
        {
            var ret = CreateValidDraft();
            ret.UpdateHeader(new DateTime(2026, 3, 1), 2, 2, 42, "ملاحظات محدثة");
            ret.OriginalInvoiceId.Should().Be(42);
            ret.CustomerId.Should().Be(2);
        }
    }

    public class PurchaseReturnTests
    {
        private PurchaseReturn CreateValidDraft()
        {
            return new PurchaseReturn("PR-001", new DateTime(2026, 2, 1), 1, 1, null, "ملاحظات");
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesDraft()
        {
            var ret = CreateValidDraft();
            ret.ReturnNumber.Should().Be("PR-001");
            ret.Status.Should().Be(InvoiceStatus.Draft);
        }

        [Fact]
        public void AddLine_ValidLine_AddsSuccessfully()
        {
            var ret = CreateValidDraft();
            ret.AddLine(1, 1, 3m, 200m, 1m, 0m, 15m);
            ret.Lines.Count.Should().Be(1);
        }

        [Fact]
        public void Post_DraftWithLines_ChangesToPosted()
        {
            var ret = CreateValidDraft();
            ret.AddLine(1, 1, 3m, 200m, 1m, 0m, 15m);
            ret.Post(1);
            ret.Status.Should().Be(InvoiceStatus.Posted);
        }

        [Fact]
        public void Cancel_PostedReturn_Cancels()
        {
            var ret = CreateValidDraft();
            ret.AddLine(1, 1, 3m, 200m, 1m, 0m, 15m);
            ret.Post(1);
            ret.Cancel();
            ret.Status.Should().Be(InvoiceStatus.Cancelled);
        }
    }
}
