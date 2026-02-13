using System;

namespace MarcoERP.Domain.Exceptions
{
    /// <summary>
    /// يُطرح عند حدوث تعارض في التزامن — Thrown when a concurrency conflict is detected.
    /// </summary>
    public class ConcurrencyConflictException : Exception
    {
        public string EntityName { get; }

        public ConcurrencyConflictException(string entityName)
            : base($"تعارض في التزامن: تم تعديل '{entityName}' بواسطة مستخدم آخر.")
        {
            EntityName = entityName;
        }

        public ConcurrencyConflictException(string entityName, Exception innerException)
            : base($"تعارض في التزامن: تم تعديل '{entityName}' بواسطة مستخدم آخر.", innerException)
        {
            EntityName = entityName;
        }
    }
}
