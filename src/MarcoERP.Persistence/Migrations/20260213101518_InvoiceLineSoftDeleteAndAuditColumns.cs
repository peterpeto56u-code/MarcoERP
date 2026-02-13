using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceLineSoftDeleteAndAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SalesInvoiceLines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SalesInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SalesInvoiceLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SalesInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SalesInvoiceLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "SalesInvoiceLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "SalesInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PurchaseInvoiceLines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PurchaseInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PurchaseInvoiceLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PurchaseInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PurchaseInvoiceLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "PurchaseInvoiceLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "PurchaseInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChangedColumns",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValues",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ChangedColumns",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValues",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OldValues",
                table: "AuditLogs");
        }
    }
}
