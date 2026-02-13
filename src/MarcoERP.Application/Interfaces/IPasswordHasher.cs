namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Interface for password hashing operations.
    /// Defined in Application layer so services can depend on it.
    /// Implemented in Infrastructure layer (BCrypt).
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>Hashes a plain-text password.</summary>
        string HashPassword(string plainTextPassword);

        /// <summary>Verifies a plain-text password against a hash.</summary>
        bool VerifyPassword(string plainTextPassword, string hashedPassword);
    }
}
