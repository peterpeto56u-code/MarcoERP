using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Security;
using MarcoERP.Domain.Interfaces.Security;

namespace MarcoERP.Persistence.Repositories.Security
{
    /// <summary>
    /// EF Core implementation of IUserRepository.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly MarcoDbContext _context;

        public UserRepository(MarcoDbContext context) => _context = context;

        // ── IRepository<User> ────────────────────────────────────

        public async Task<User> GetByIdAsync(int id, CancellationToken ct = default)
            => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
            => await _context.Users.OrderBy(u => u.Username).ToListAsync(ct);

        public async Task AddAsync(User entity, CancellationToken ct = default)
            => await _context.Users.AddAsync(entity, ct);

        public void Update(User entity) => _context.Users.Update(entity);
        public void Remove(User entity) => _context.Users.Remove(entity);

        // ── IUserRepository ──────────────────────────────────────

        public async Task<User> GetByUsernameAsync(string username, CancellationToken ct = default)
            => await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username, ct);

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Users.Where(u => u.Username == username);
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);
            return await query.AnyAsync(ct);
        }

        public async Task<IReadOnlyList<User>> GetAllWithRolesAsync(CancellationToken ct = default)
            => await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Username)
                .ToListAsync(ct);

        public async Task<User> GetByIdWithRoleAsync(int id, CancellationToken ct = default)
            => await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}
