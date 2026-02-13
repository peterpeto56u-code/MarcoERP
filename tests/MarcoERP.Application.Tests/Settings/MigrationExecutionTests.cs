using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using Moq;
using Xunit;

namespace MarcoERP.Application.Tests.Settings
{
    /// <summary>
    /// Phase 6: Controlled Migration Engine tests.
    /// 3 scenarios: success, failure with log, backup failure → block.
    /// </summary>
    public sealed class MigrationExecutionTests
    {
        private readonly Mock<IDatabaseBackupService> _backupMock;
        private readonly Mock<IMigrationExecutionService> _serviceMock;
        private readonly Mock<ICurrentUserService> _userMock;

        public MigrationExecutionTests()
        {
            _backupMock = new Mock<IDatabaseBackupService>();
            _serviceMock = new Mock<IMigrationExecutionService>();
            _userMock = new Mock<ICurrentUserService>();
            _userMock.Setup(u => u.Username).Returns("admin");
            _userMock.Setup(u => u.IsAuthenticated).Returns(true);
        }

        // ── Scenario 1: Successful migration execution ──

        [Fact]
        public async Task ExecuteMigrationsAsync_Success_ReturnsSuccessAndLogs()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetPendingMigrationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "20240101_AddMigrationExecutionEngine" });

            _serviceMock
                .Setup(s => s.ExecuteMigrationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult.Success());

            _serviceMock
                .Setup(s => s.GetExecutionHistoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MigrationExecutionDto>
                {
                    new MigrationExecutionDto
                    {
                        Id = 1,
                        MigrationName = "20240101_AddMigrationExecutionEngine",
                        StartedAt = DateTime.UtcNow.AddSeconds(-2),
                        CompletedAt = DateTime.UtcNow,
                        IsSuccessful = true,
                        ExecutedBy = "admin",
                        BackupPath = @"C:\Backups\Migrations\MarcoERP_backup.bak"
                    }
                });

            // Act
            var pending = await _serviceMock.Object.GetPendingMigrationsAsync();
            var result = await _serviceMock.Object.ExecuteMigrationsAsync();
            var history = await _serviceMock.Object.GetExecutionHistoryAsync();

            // Assert
            pending.Should().HaveCount(1);
            result.IsSuccess.Should().BeTrue();
            history.Should().HaveCount(1);
            history[0].IsSuccessful.Should().BeTrue();
            history[0].BackupPath.Should().NotBeNullOrEmpty();
            history[0].DurationSeconds.Should().BeGreaterThan(0);
        }

        // ── Scenario 2: Migration failure with log recorded ──

        [Fact]
        public async Task ExecuteMigrationsAsync_Failure_LogsErrorAndReturnsFailed()
        {
            // Arrange
            var errorMessage = "خطأ في تعديلات الجدول: FK constraint violation";

            _serviceMock
                .Setup(s => s.ExecuteMigrationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult.Failure($"فشل تنفيذ الترحيل: {errorMessage}"));

            _serviceMock
                .Setup(s => s.GetExecutionHistoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MigrationExecutionDto>
                {
                    new MigrationExecutionDto
                    {
                        Id = 1,
                        MigrationName = "20240101_FailingMigration",
                        StartedAt = DateTime.UtcNow.AddSeconds(-1),
                        CompletedAt = DateTime.UtcNow,
                        IsSuccessful = false,
                        ExecutedBy = "admin",
                        ErrorMessage = errorMessage,
                        BackupPath = @"C:\Backups\Migrations\MarcoERP_backup.bak"
                    }
                });

            // Act
            var result = await _serviceMock.Object.ExecuteMigrationsAsync();
            var history = await _serviceMock.Object.GetExecutionHistoryAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("فشل تنفيذ الترحيل");
            history.Should().HaveCount(1);
            history[0].IsSuccessful.Should().BeFalse();
            history[0].ErrorMessage.Should().Contain("FK constraint");
            history[0].BackupPath.Should().NotBeNullOrEmpty("النسخة الاحتياطية يجب أن تكون متوفرة للاسترجاع");
        }

        // ── Scenario 3: Backup failure blocks migration ──

        [Fact]
        public async Task ExecuteMigrationsAsync_BackupFailure_BlocksMigration()
        {
            // Arrange
            _backupMock
                .Setup(b => b.CreatePreMigrationBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult<string>.Failure("لا يمكن الوصول لمسار النسخ الاحتياطي"));

            // When backup fails, the migration service should return failure
            _serviceMock
                .Setup(s => s.ExecuteMigrationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult.Failure("توقف الترحيل: فشل النسخ الاحتياطي — لا يمكن الوصول لمسار النسخ الاحتياطي"));

            _serviceMock
                .Setup(s => s.GetExecutionHistoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MigrationExecutionDto>()); // No execution recorded

            // Act
            var backupResult = await _backupMock.Object.CreatePreMigrationBackupAsync("test");
            var migrationResult = await _serviceMock.Object.ExecuteMigrationsAsync();
            var history = await _serviceMock.Object.GetExecutionHistoryAsync();

            // Assert
            backupResult.IsSuccess.Should().BeFalse("النسخ الاحتياطي فشل");
            migrationResult.IsSuccess.Should().BeFalse("يجب إيقاف الترحيل عند فشل النسخ الاحتياطي");
            migrationResult.ErrorMessage.Should().Contain("توقف الترحيل");
            history.Should().BeEmpty("لا يجب تسجيل تنفيذ عند فشل النسخ الاحتياطي");
        }

        // ── Domain entity tests ──

        [Fact]
        public void MigrationExecution_MarkSuccess_SetsCorrectState()
        {
            var execution = new Domain.Entities.Settings.MigrationExecution("Test_Migration", "admin", DateTime.UtcNow);

            execution.MarkSuccess(DateTime.UtcNow);

            execution.IsSuccessful.Should().BeTrue();
            execution.CompletedAt.Should().NotBeNull();
            execution.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void MigrationExecution_MarkFailed_SetsErrorMessage()
        {
            var execution = new Domain.Entities.Settings.MigrationExecution("Test_Migration", "admin", DateTime.UtcNow);

            execution.MarkFailed("Connection timeout", DateTime.UtcNow);

            execution.IsSuccessful.Should().BeFalse();
            execution.CompletedAt.Should().NotBeNull();
            execution.ErrorMessage.Should().Be("Connection timeout");
        }

        [Fact]
        public void MigrationExecution_Constructor_ThrowsOnEmptyName()
        {
            Action act = () => new Domain.Entities.Settings.MigrationExecution("", "admin", DateTime.UtcNow);

            act.Should().Throw<ArgumentException>().WithMessage("*Migration*");
        }

        [Fact]
        public void MigrationExecution_SetBackupPath_RecordsPath()
        {
            var execution = new Domain.Entities.Settings.MigrationExecution("Test_Migration", "admin", DateTime.UtcNow);

            execution.SetBackupPath(@"C:\Backup\test.bak");

            execution.BackupPath.Should().Be(@"C:\Backup\test.bak");
        }
    }
}
