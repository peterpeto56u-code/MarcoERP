using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Security;

namespace MarcoERP.Domain.Interfaces.Security
{
    /// <summary>
    /// Repository contract for User entity.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>Gets a user by username (case-insensitive).</summary>
        Task<User> GetByUsernameAsync(string username, CancellationToken ct = default);

        /// <summary>Checks if a username already exists.</summary>
        Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken ct = default);

        /// <summary>Gets all users with their role navigation loaded.</summary>
        Task<IReadOnlyList<User>> GetAllWithRolesAsync(CancellationToken ct = default);

        /// <summary>Gets a user by ID with role navigation loaded.</summary>
        Task<User> GetByIdWithRoleAsync(int id, CancellationToken ct = default);
    }
}
