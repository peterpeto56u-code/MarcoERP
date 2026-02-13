using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Persistence.Services.Settings
{
    /// <summary>
    /// Phase 6: Controlled Migration Engine — orchestrates backup → migrate → log.
    /// Does NOT replace EF Core; only wraps MigrateAsync with safety and tracking.
    /// </summary>
    public sealed class MigrationExecutionService : IMigrationExecutionService
    {
        private readonly MarcoDbContext _dbContext;
        private readonly IDatabaseBackupService _backupService;
        private readonly ICurrentUserService _currentUser;

        public MigrationExecutionService(
            MarcoDbContext dbContext,
            IDatabaseBackupService backupService,
            ICurrentUserService currentUser)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken ct = default)
        {
            var pending = await _dbContext.Database.GetPendingMigrationsAsync(ct);
            return pending.ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ExecuteMigrationsAsync(CancellationToken ct = default)
        {
            var pending = await GetPendingMigrationsAsync(ct);
            if (pending.Count == 0)
                return ServiceResult.Success();

            var executedBy = _currentUser?.Username ?? "System";

            // ── Step 1: Create pre-migration backup ──
            var backupResult = await _backupService.CreatePreMigrationBackupAsync(
                $"Pre-migration backup ({pending.Count} pending)", ct);

            if (backupResult.IsFailure)
                return ServiceResult.Failure($"توقف الترحيل: فشل النسخ الاحتياطي — {backupResult.ErrorMessage}");

            var backupPath = backupResult.Data;

            // ── Step 2: Record execution for each pending migration ──
            var executions = new List<MigrationExecution>();
            foreach (var migrationName in pending)
            {
                var execution = new MigrationExecution(migrationName, executedBy, DateTime.UtcNow);
                execution.SetBackupPath(backupPath);
                executions.Add(execution);
            }

            foreach (var exec in executions)
                _dbContext.MigrationExecutions.Add(exec);

            await _dbContext.SaveChangesAsync(ct);

            // ── Step 3: Execute MigrateAsync ──
            try
            {
                await _dbContext.Database.MigrateAsync(ct);

                // Mark all as successful
                foreach (var exec in executions)
                    exec.MarkSuccess(DateTime.UtcNow);

                await _dbContext.SaveChangesAsync(ct);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                // Mark all as failed
                foreach (var exec in executions)
                    exec.MarkFailed(ex.Message, DateTime.UtcNow);

                try
                {
                    await _dbContext.SaveChangesAsync(ct);
                }
                catch
                {
                    // If saving the failure record also fails, we can't do much
                }

                return ServiceResult.Failure($"فشل تنفيذ الترحيل: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MigrationExecutionDto>> GetExecutionHistoryAsync(CancellationToken ct = default)
        {
            var history = await _dbContext.MigrationExecutions
                .AsNoTracking()
                .OrderByDescending(m => m.StartedAt)
                .Select(m => new MigrationExecutionDto
                {
                    Id = m.Id,
                    MigrationName = m.MigrationName,
                    StartedAt = m.StartedAt,
                    CompletedAt = m.CompletedAt,
                    IsSuccessful = m.IsSuccessful,
                    ExecutedBy = m.ExecutedBy,
                    ErrorMessage = m.ErrorMessage,
                    BackupPath = m.BackupPath
                })
                .ToListAsync(ct);

            return history.AsReadOnly();
        }
    }
}
