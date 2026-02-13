using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Settings;
using MarcoERP.Domain.Interfaces.Settings;

namespace MarcoERP.Persistence.Repositories.Settings
{
    /// <summary>
    /// EF Core implementation of IProfileRepository.
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public sealed class ProfileRepository : IProfileRepository
    {
        private readonly MarcoDbContext _context;

        public ProfileRepository(MarcoDbContext context) => _context = context;

        // ── IRepository<SystemProfile> ───────────────────────────

        public async Task<SystemProfile> GetByIdAsync(int id, CancellationToken ct = default)
            => await _context.SystemProfiles.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<SystemProfile>> GetAllAsync(CancellationToken ct = default)
            => await _context.SystemProfiles
                .OrderBy(p => p.ProfileName)
                .ToListAsync(ct);

        public async Task AddAsync(SystemProfile entity, CancellationToken ct = default)
            => await _context.SystemProfiles.AddAsync(entity, ct);

        public void Update(SystemProfile entity) => _context.SystemProfiles.Update(entity);
        public void Remove(SystemProfile entity) => _context.SystemProfiles.Remove(entity);

        // ── IProfileRepository ───────────────────────────────────

        public async Task<SystemProfile> GetByNameAsync(string profileName, CancellationToken ct = default)
            => await _context.SystemProfiles.FirstOrDefaultAsync(p => p.ProfileName == profileName, ct);

        public async Task<SystemProfile> GetActiveProfileAsync(CancellationToken ct = default)
            => await _context.SystemProfiles.FirstOrDefaultAsync(p => p.IsActive, ct);

        public async Task<IReadOnlyList<string>> GetFeatureKeysForProfileAsync(int profileId, CancellationToken ct = default)
            => await _context.ProfileFeatures
                .Where(pf => pf.ProfileId == profileId)
                .Select(pf => pf.FeatureKey)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<string>> GetFeatureKeysForProfileAsync(string profileName, CancellationToken ct = default)
        {
            var profile = await GetByNameAsync(profileName, ct);
            if (profile == null)
                return new List<string>();

            return await GetFeatureKeysForProfileAsync(profile.Id, ct);
        }
    }
}
