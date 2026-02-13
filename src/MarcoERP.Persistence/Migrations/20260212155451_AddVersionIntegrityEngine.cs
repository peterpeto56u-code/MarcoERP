using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionIntegrityEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeatureVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IntroducedInVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureVersions_FeatureKey",
                table: "FeatureVersions",
                column: "FeatureKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemVersions_VersionNumber",
                table: "SystemVersions",
                column: "VersionNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureVersions");

            migrationBuilder.DropTable(
                name: "SystemVersions");
        }
    }
}
