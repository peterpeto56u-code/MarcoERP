using System;

namespace MarcoERP.Application.DTOs.Treasury
{
    // ════════════════════════════════════════════════════════════
    //  CashPayment DTOs (سند صرف)
    // ════════════════════════════════════════════════════════════

    /// <summary>Full cash payment details.</summary>
    public sealed class CashPaymentDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public int CashboxId { get; set; }
        public string CashboxName { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public int? JournalEntryId { get; set; }
    }

    /// <summary>DTO for creating a new cash payment.</summary>
    public sealed class CreateCashPaymentDto
    {
        public DateTime PaymentDate { get; set; }
        public int CashboxId { get; set; }
        public int AccountId { get; set; }
        public int? SupplierId { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>DTO for updating an existing cash payment.</summary>
    public sealed class UpdateCashPaymentDto
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public int CashboxId { get; set; }
        public int AccountId { get; set; }
        public int? SupplierId { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>Lightweight DTO for list views.</summary>
    public sealed class CashPaymentListDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CashboxName { get; set; }
        public string AccountName { get; set; }
        public string SupplierName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
