using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MarcoDbContext))]
    [Migration("20260213101000_AddAuditLogChangeColumns")]
    public partial class AddAuditLogChangeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('AuditLogs', 'OldValues') IS NULL
BEGIN
    ALTER TABLE [AuditLogs] ADD [OldValues] nvarchar(max) NULL;
END

IF COL_LENGTH('AuditLogs', 'NewValues') IS NULL
BEGIN
    ALTER TABLE [AuditLogs] ADD [NewValues] nvarchar(max) NULL;
END

IF COL_LENGTH('AuditLogs', 'ChangedColumns') IS NULL
BEGIN
    ALTER TABLE [AuditLogs] ADD [ChangedColumns] nvarchar(max) NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('AuditLogs', 'ChangedColumns') IS NOT NULL
BEGIN
    ALTER TABLE [AuditLogs] DROP COLUMN [ChangedColumns];
END

IF COL_LENGTH('AuditLogs', 'NewValues') IS NOT NULL
BEGIN
    ALTER TABLE [AuditLogs] DROP COLUMN [NewValues];
END

IF COL_LENGTH('AuditLogs', 'OldValues') IS NOT NULL
BEGIN
    ALTER TABLE [AuditLogs] DROP COLUMN [OldValues];
END");
        }
    }
}
