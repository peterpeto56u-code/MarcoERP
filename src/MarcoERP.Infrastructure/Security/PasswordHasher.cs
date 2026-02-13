using MarcoERP.Application.Interfaces;

namespace MarcoERP.Infrastructure.Security
{
    /// <summary>
    /// BCrypt-based password hashing implementation.
    /// Uses BCrypt.Net-Next with work factor 12 (SECURITY_POLICY compliant).
    /// Implements Application.Interfaces.IPasswordHasher for clean architecture.
    /// </summary>
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        /// <inheritdoc />
        public string HashPassword(string plainTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
        }

        /// <inheritdoc />
        public bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }
    }
}
