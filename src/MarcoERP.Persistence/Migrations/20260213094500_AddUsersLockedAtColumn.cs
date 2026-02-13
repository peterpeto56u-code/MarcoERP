using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MarcoDbContext))]
    [Migration("20260213094500_AddUsersLockedAtColumn")]
    public partial class AddUsersLockedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Users', 'LockedAt') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [LockedAt] datetime2 NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Users', 'LockedAt') IS NOT NULL
BEGIN
    ALTER TABLE [Users] DROP COLUMN [LockedAt];
END");
        }
    }
}
