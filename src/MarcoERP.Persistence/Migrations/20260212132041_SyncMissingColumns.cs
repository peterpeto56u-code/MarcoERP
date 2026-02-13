using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CounterpartyType",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "SalesInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyCustomerId",
                table: "PurchaseInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyType",
                table: "PurchaseInvoices",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DefaultSupplierId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_SupplierId",
                table: "SalesInvoices",
                column: "SupplierId",
                filter: "[SupplierId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_CounterpartyCustomerId",
                table: "PurchaseInvoices",
                column: "CounterpartyCustomerId",
                filter: "[CounterpartyCustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultSupplierId",
                table: "Products",
                column: "DefaultSupplierId",
                filter: "[DefaultSupplierId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_DefaultSupplierId",
                table: "Products",
                column: "DefaultSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Customers_CounterpartyCustomerId",
                table: "PurchaseInvoices",
                column: "CounterpartyCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Suppliers_SupplierId",
                table: "SalesInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_DefaultSupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Customers_CounterpartyCustomerId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Suppliers_SupplierId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_SupplierId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_CounterpartyCustomerId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultSupplierId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CounterpartyType",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CounterpartyCustomerId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "CounterpartyType",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "DefaultSupplierId",
                table: "Products");
        }
    }
}
