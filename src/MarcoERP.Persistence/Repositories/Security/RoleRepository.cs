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
    /// EF Core implementation of IRoleRepository.
    /// </summary>
    public sealed class RoleRepository : IRoleRepository
    {
        private readonly MarcoDbContext _context;

        public RoleRepository(MarcoDbContext context) => _context = context;

        // ── IRepository<Role> ────────────────────────────────────

        public async Task<Role> GetByIdAsync(int id, CancellationToken ct = default)
            => await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
            => await _context.Roles.OrderBy(r => r.Id).ToListAsync(ct);

        public async Task AddAsync(Role entity, CancellationToken ct = default)
            => await _context.Roles.AddAsync(entity, ct);

        public void Update(Role entity) => _context.Roles.Update(entity);
        public void Remove(Role entity) => _context.Roles.Remove(entity);

        // ── IRoleRepository ──────────────────────────────────────

        public async Task<Role> GetByIdWithPermissionsAsync(int id, CancellationToken ct = default)
            => await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IReadOnlyList<Role>> GetAllWithPermissionsAsync(CancellationToken ct = default)
            => await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .OrderBy(r => r.Id)
                .ToListAsync(ct);

        public async Task<Role> GetByNameEnAsync(string nameEn, CancellationToken ct = default)
            => await _context.Roles.FirstOrDefaultAsync(r => r.NameEn == nameEn, ct);

        public async Task<bool> NameExistsAsync(string nameEn, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Roles.Where(r => r.NameEn == nameEn);
            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);
            return await query.AnyAsync(ct);
        }
    }
}
