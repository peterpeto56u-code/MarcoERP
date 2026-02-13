using System.Collections.Generic;
using MarcoERP.Application.Reporting.Interfaces;
using MarcoERP.Domain.Enums;

namespace MarcoERP.WpfUI.Reporting
{
    /// <summary>
    /// Resolves <see cref="SourceType"/> → navigation view key for drill-down.
    /// Centralized mapping so all reports share the same navigation resolution logic.
    /// </summary>
    public sealed class DrillDownResolver : IDrillDownResolver
    {
        private static readonly Dictionary<SourceType, (string viewKey, string label)> _map = new()
        {
            [SourceType.Manual]            = ("JournalEntry",      "عرض القيد"),
            [SourceType.SalesInvoice]      = ("SalesInvoice",      "عرض فاتورة المبيعات"),
            [SourceType.PurchaseInvoice]   = ("PurchaseInvoice",   "عرض فاتورة المشتريات"),
            [SourceType.CashReceipt]       = ("CashReceipts",      "عرض سند القبض"),
            [SourceType.CashPayment]       = ("CashPayments",      "عرض سند الصرف"),
            [SourceType.Inventory]         = ("InventoryReport",   "عرض تقرير المخزون"),
            [SourceType.Adjustment]        = ("InventoryAdjustment","عرض تسوية المخزون"),
            [SourceType.Opening]           = ("JournalEntry",      "عرض قيد الافتتاح"),
            [SourceType.Closing]           = ("JournalEntry",      "عرض قيد الإقفال"),
            [SourceType.PurchaseReturn]    = ("PurchaseReturn",    "عرض مرتجع المشتريات"),
            [SourceType.SalesReturn]       = ("SalesReturn",       "عرض مرتجع المبيعات"),
            [SourceType.CashTransfer]      = ("CashTransfers",     "عرض التحويل"),
            [SourceType.SalesQuotation]    = ("SalesQuotation",    "عرض عرض السعر"),
            [SourceType.PurchaseQuotation] = ("PurchaseQuotation", "عرض طلب الشراء"),
        };

        public string ResolveNavigationKey(SourceType sourceType)
            => _map.TryGetValue(sourceType, out var entry) ? entry.viewKey : null;

        public string GetActionLabel(SourceType sourceType)
            => _map.TryGetValue(sourceType, out var entry) ? entry.label : "عرض المستند";
    }
}
