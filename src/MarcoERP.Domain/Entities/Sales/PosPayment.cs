using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Sales;

namespace MarcoERP.Domain.Entities.Sales
{
    /// <summary>
    /// Records a payment method used for a POS invoice.
    /// A single POS invoice can have multiple payments (split/mixed payment).
    /// </summary>
    public sealed class PosPayment : BaseEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private PosPayment() { }

        /// <summary>
        /// Creates a POS payment record.
        /// </summary>
        public PosPayment(
            int salesInvoiceId,
            int posSessionId,
            PaymentMethod paymentMethod,
            decimal amount,
            DateTime paidAtUtc,
            string referenceNumber = null)
        {
            if (salesInvoiceId <= 0)
                throw new SalesInvoiceDomainException("فاتورة البيع مطلوبة.");
            if (posSessionId <= 0)
                throw new SalesInvoiceDomainException("جلسة نقطة البيع مطلوبة.");
            if (amount <= 0)
                throw new SalesInvoiceDomainException("مبلغ الدفع يجب أن يكون أكبر من صفر.");

            SalesInvoiceId = salesInvoiceId;
            PosSessionId = posSessionId;
            PaymentMethod = paymentMethod;
            Amount = amount;
            ReferenceNumber = referenceNumber?.Trim();
            PaidAt = paidAtUtc;
            SalesInvoice = null;
            PosSession = null;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>FK to SalesInvoice.</summary>
        public int SalesInvoiceId { get; private set; }

        /// <summary>FK to PosSession.</summary>
        public int PosSessionId { get; private set; }

        /// <summary>Payment method used.</summary>
        public PaymentMethod PaymentMethod { get; private set; }

        /// <summary>Amount paid in this method.</summary>
        public decimal Amount { get; private set; }

        /// <summary>Reference number (card transaction ID, etc.).</summary>
        public string ReferenceNumber { get; private set; }

        /// <summary>UTC timestamp of payment.</summary>
        public DateTime PaidAt { get; private set; }

        // ── Navigation Properties ────────────────────────────────

        /// <summary>Related sales invoice.</summary>
        public SalesInvoice SalesInvoice { get; private set; }

        /// <summary>Related POS session.</summary>
        public PosSession PosSession { get; private set; }
    }
}
