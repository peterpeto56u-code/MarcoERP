using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Warehouses",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Warehouses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Warehouses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SalesReturns",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SalesQuotations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PurchaseReturns",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PurchaseQuotations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PurchaseInvoices",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "JournalEntries",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "InventoryAdjustments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "CashTransfers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "CashReceipts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "CashPayments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Cashboxes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Cashboxes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Cashboxes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Cashboxes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "BankAccounts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BankAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BankAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BankAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_CompanyId",
                table: "Warehouses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CompanyId",
                table: "Suppliers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_CompanyId",
                table: "SalesReturns",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotations_CompanyId",
                table: "SalesQuotations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CompanyId",
                table: "SalesInvoices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_CompanyId",
                table: "PurchaseReturns",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuotations_CompanyId",
                table: "PurchaseQuotations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_CompanyId",
                table: "PurchaseInvoices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId",
                table: "Products",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CompanyId",
                table: "JournalEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_CompanyId",
                table: "InventoryAdjustments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId",
                table: "Customers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_CompanyId",
                table: "CashTransfers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_CompanyId",
                table: "CashReceipts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_CompanyId",
                table: "CashPayments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Cashboxes_CompanyId",
                table: "Cashboxes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CompanyId",
                table: "BankAccounts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Code",
                table: "Companies",
                column: "Code",
                unique: true);

            // Seed the default company BEFORE adding foreign keys
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Id] = 1)
                BEGIN
                    SET IDENTITY_INSERT [Companies] ON;
                    INSERT INTO [Companies] ([Id], [Code], [NameAr], [NameEn], [IsActive], [CreatedAt], [CreatedBy])
                    VALUES (1, N'DEF', N'الشركة الافتراضية', N'Default Company', 1, GETUTCDATE(), N'SYSTEM');
                    SET IDENTITY_INSERT [Companies] OFF;
                END
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Companies_CompanyId",
                table: "BankAccounts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cashboxes_Companies_CompanyId",
                table: "Cashboxes",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashPayments_Companies_CompanyId",
                table: "CashPayments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashReceipts_Companies_CompanyId",
                table: "CashReceipts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashTransfers_Companies_CompanyId",
                table: "CashTransfers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Companies_CompanyId",
                table: "Customers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAdjustments_Companies_CompanyId",
                table: "InventoryAdjustments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Companies_CompanyId",
                table: "JournalEntries",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Companies_CompanyId",
                table: "PurchaseInvoices",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseQuotations_Companies_CompanyId",
                table: "PurchaseQuotations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Companies_CompanyId",
                table: "PurchaseReturns",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Companies_CompanyId",
                table: "SalesInvoices",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesQuotations_Companies_CompanyId",
                table: "SalesQuotations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Companies_CompanyId",
                table: "SalesReturns",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Companies_CompanyId",
                table: "Suppliers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Companies_CompanyId",
                table: "Warehouses",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Companies_CompanyId",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Cashboxes_Companies_CompanyId",
                table: "Cashboxes");

            migrationBuilder.DropForeignKey(
                name: "FK_CashPayments_Companies_CompanyId",
                table: "CashPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_CashReceipts_Companies_CompanyId",
                table: "CashReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_CashTransfers_Companies_CompanyId",
                table: "CashTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Companies_CompanyId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAdjustments_Companies_CompanyId",
                table: "InventoryAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Companies_CompanyId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Companies_CompanyId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseQuotations_Companies_CompanyId",
                table: "PurchaseQuotations");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Companies_CompanyId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Companies_CompanyId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesQuotations_Companies_CompanyId",
                table: "SalesQuotations");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Companies_CompanyId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Companies_CompanyId",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Companies_CompanyId",
                table: "Warehouses");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_CompanyId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CompanyId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_CompanyId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesQuotations_CompanyId",
                table: "SalesQuotations");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CompanyId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_CompanyId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseQuotations_CompanyId",
                table: "PurchaseQuotations");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_CompanyId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CompanyId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAdjustments_CompanyId",
                table: "InventoryAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CompanyId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CashTransfers_CompanyId",
                table: "CashTransfers");

            migrationBuilder.DropIndex(
                name: "IX_CashReceipts_CompanyId",
                table: "CashReceipts");

            migrationBuilder.DropIndex(
                name: "IX_CashPayments_CompanyId",
                table: "CashPayments");

            migrationBuilder.DropIndex(
                name: "IX_Cashboxes_CompanyId",
                table: "Cashboxes");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_CompanyId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SalesQuotations");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PurchaseQuotations");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "InventoryAdjustments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CashTransfers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CashReceipts");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CashPayments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Cashboxes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Cashboxes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Cashboxes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Cashboxes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BankAccounts");
        }
    }
}
