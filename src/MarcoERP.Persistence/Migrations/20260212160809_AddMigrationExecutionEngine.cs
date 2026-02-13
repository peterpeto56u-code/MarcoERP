using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationExecutionEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MigrationExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MigrationName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ExecutedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BackupPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationExecutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MigrationExecutions_Name",
                table: "MigrationExecutions",
                column: "MigrationName");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationExecutions_StartedAt",
                table: "MigrationExecutions",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MigrationExecutions");
        }
    }
}
