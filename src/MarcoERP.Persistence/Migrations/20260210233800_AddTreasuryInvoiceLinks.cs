using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTreasuryInvoiceLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesInvoiceId",
                table: "CashReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseInvoiceId",
                table: "CashPayments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_SalesInvoiceId",
                table: "CashReceipts",
                column: "SalesInvoiceId",
                filter: "[SalesInvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_PurchaseInvoiceId",
                table: "CashPayments",
                column: "PurchaseInvoiceId",
                filter: "[PurchaseInvoiceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CashPayments_PurchaseInvoices_PurchaseInvoiceId",
                table: "CashPayments",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashReceipts_SalesInvoices_SalesInvoiceId",
                table: "CashReceipts",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashPayments_PurchaseInvoices_PurchaseInvoiceId",
                table: "CashPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_CashReceipts_SalesInvoices_SalesInvoiceId",
                table: "CashReceipts");

            migrationBuilder.DropIndex(
                name: "IX_CashReceipts_SalesInvoiceId",
                table: "CashReceipts");

            migrationBuilder.DropIndex(
                name: "IX_CashPayments_PurchaseInvoiceId",
                table: "CashPayments");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId",
                table: "CashReceipts");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "CashPayments");
        }
    }
}
