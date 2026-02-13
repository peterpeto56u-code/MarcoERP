using System;

namespace MarcoERP.Domain.Exceptions
{
    /// <summary>
    /// Domain exception for security-related invariant violations
    /// (user management, authentication, authorization).
    /// </summary>
    public class SecurityDomainException : Exception
    {
        public SecurityDomainException(string message) : base(message) { }
        public SecurityDomainException(string message, Exception inner) : base(message, inner) { }
    }
}
