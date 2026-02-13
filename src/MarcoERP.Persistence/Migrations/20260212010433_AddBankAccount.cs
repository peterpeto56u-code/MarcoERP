using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "SalesInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "PurchaseInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CostCenterId",
                table: "JournalEntryLines",
                type: "int",
                nullable: true,
                comment: "Reserved for future Cost Center module",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CostCenterId",
                table: "JournalEntries",
                type: "int",
                nullable: true,
                comment: "Reserved for future Cost Center module",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_JournalEntries_Balance",
                table: "JournalEntries",
                sql: "[Status] <> 1 OR [TotalDebit] = [TotalCredit]");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Code",
                table: "BankAccounts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_IBAN",
                table: "BankAccounts",
                column: "IBAN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_JournalEntries_Balance",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "PurchaseInvoices");

            migrationBuilder.AlterColumn<int>(
                name: "CostCenterId",
                table: "JournalEntryLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "Reserved for future Cost Center module");

            migrationBuilder.AlterColumn<int>(
                name: "CostCenterId",
                table: "JournalEntries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "Reserved for future Cost Center module");
        }
    }
}
