using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImpactAnalyzerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AffectsAccounting",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsData",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsInventory",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsReporting",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImpactDescription",
                table: "Features",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresMigration",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffectsAccounting",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "AffectsData",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "AffectsInventory",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "AffectsReporting",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "ImpactDescription",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "RequiresMigration",
                table: "Features");
        }
    }
}
