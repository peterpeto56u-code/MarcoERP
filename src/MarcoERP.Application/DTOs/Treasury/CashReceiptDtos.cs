using System;

namespace MarcoERP.Application.DTOs.Treasury
{
    // ════════════════════════════════════════════════════════════
    //  CashReceipt DTOs (سند قبض)
    // ════════════════════════════════════════════════════════════

    /// <summary>Full cash receipt details.</summary>
    public sealed class CashReceiptDto
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int CashboxId { get; set; }
        public string CashboxName { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? SalesInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public int? JournalEntryId { get; set; }
    }

    /// <summary>DTO for creating a new cash receipt.</summary>
    public sealed class CreateCashReceiptDto
    {
        public DateTime ReceiptDate { get; set; }
        public int CashboxId { get; set; }
        public int AccountId { get; set; }
        public int? CustomerId { get; set; }
        public int? SalesInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>DTO for updating an existing cash receipt.</summary>
    public sealed class UpdateCashReceiptDto
    {
        public int Id { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int CashboxId { get; set; }
        public int AccountId { get; set; }
        public int? CustomerId { get; set; }
        public int? SalesInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>Lightweight DTO for list views.</summary>
    public sealed class CashReceiptListDto
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string CashboxName { get; set; }
        public string AccountName { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
