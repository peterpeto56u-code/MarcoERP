using System;

namespace MarcoERP.Domain.Entities.Common
{
    /// <summary>
    /// Base class for all domain entities.
    /// Provides identity (int PK) and optimistic concurrency token.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>Primary key — identity column in database.</summary>
        public int Id { get; protected set; }

        /// <summary>Optimistic concurrency token (SQL Server rowversion).</summary>
        public byte[] RowVersion { get; set; }
    }
}
