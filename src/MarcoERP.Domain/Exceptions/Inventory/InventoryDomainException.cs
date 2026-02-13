using System;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Exceptions.Inventory
{
    /// <summary>
    /// Thrown when an inventory-related domain invariant is violated.
    /// </summary>
    public sealed class InventoryDomainException : Exception
    {
        public InventoryDomainException(string message)
            : base(message) { }

        public InventoryDomainException(string message, Exception inner)
            : base(message, inner) { }
    }
}
