namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Type of inventory movement — dictates stock increase or decrease.
    /// </summary>
    public enum MovementType
    {
        /// <summary>Stock increase from purchase receipt.</summary>
        PurchaseIn = 0,

        /// <summary>Stock decrease from sales delivery.</summary>
        SalesOut = 1,

        /// <summary>Stock increase from sales return.</summary>
        SalesReturn = 2,

        /// <summary>Stock decrease from purchase return.</summary>
        PurchaseReturn = 3,

        /// <summary>Manual positive adjustment (e.g., stock count surplus).</summary>
        AdjustmentIn = 4,

        /// <summary>Manual negative adjustment (e.g., damage, shrinkage).</summary>
        AdjustmentOut = 5,

        /// <summary>Stock transfer out from source warehouse.</summary>
        TransferOut = 6,

        /// <summary>Stock transfer in to destination warehouse.</summary>
        TransferIn = 7,

        /// <summary>Opening balance — initial stock setup.</summary>
        OpeningBalance = 8
    }
}
