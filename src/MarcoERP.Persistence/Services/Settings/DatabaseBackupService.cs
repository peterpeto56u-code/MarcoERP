using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.Persistence.Services.Settings
{
    /// <summary>
    /// Phase 6: Wraps existing IBackupService for pre-migration backups.
    /// Creates backups in a dedicated "Migrations" subfolder.
    /// </summary>
    public sealed class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly IBackupService _backupService;

        public DatabaseBackupService(IBackupService backupService)
        {
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        }

        public async Task<ServiceResult<string>> CreatePreMigrationBackupAsync(
            string reason, CancellationToken ct = default)
        {
            try
            {
                var backupDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Backups",
                    "Migrations");

                Directory.CreateDirectory(backupDir);

                var result = await _backupService.BackupAsync(backupDir, ct);

                if (result.IsFailure)
                    return ServiceResult<string>.Failure($"فشل النسخ الاحتياطي قبل الترحيل: {result.ErrorMessage}");

                return ServiceResult<string>.Success(result.Data.FilePath);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure($"خطأ أثناء النسخ الاحتياطي: {ex.Message}");
            }
        }
    }
}
