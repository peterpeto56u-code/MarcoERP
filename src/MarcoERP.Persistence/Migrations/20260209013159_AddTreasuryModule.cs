using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTreasuryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cashboxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Cashboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "date", nullable: false),
                    CashboxId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashPayments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashPayments_Cashboxes_CashboxId",
                        column: x => x.CashboxId,
                        principalTable: "Cashboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashPayments_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashPayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "date", nullable: false),
                    CashboxId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashReceipts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashReceipts_Cashboxes_CashboxId",
                        column: x => x.CashboxId,
                        principalTable: "Cashboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashReceipts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashReceipts_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "date", nullable: false),
                    SourceCashboxId = table.Column<int>(type: "int", nullable: false),
                    TargetCashboxId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashTransfers_Cashboxes_SourceCashboxId",
                        column: x => x.SourceCashboxId,
                        principalTable: "Cashboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashTransfers_Cashboxes_TargetCashboxId",
                        column: x => x.TargetCashboxId,
                        principalTable: "Cashboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashTransfers_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cashboxes_Code",
                table: "Cashboxes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_AccountId",
                table: "CashPayments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_CashboxId",
                table: "CashPayments",
                column: "CashboxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_JournalEntryId",
                table: "CashPayments",
                column: "JournalEntryId",
                filter: "[JournalEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_PaymentDate",
                table: "CashPayments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_PaymentNumber",
                table: "CashPayments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_Status",
                table: "CashPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CashPayments_SupplierId",
                table: "CashPayments",
                column: "SupplierId",
                filter: "[SupplierId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_AccountId",
                table: "CashReceipts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_CashboxId",
                table: "CashReceipts",
                column: "CashboxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_CustomerId",
                table: "CashReceipts",
                column: "CustomerId",
                filter: "[CustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_JournalEntryId",
                table: "CashReceipts",
                column: "JournalEntryId",
                filter: "[JournalEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_ReceiptDate",
                table: "CashReceipts",
                column: "ReceiptDate");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_ReceiptNumber",
                table: "CashReceipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashReceipts_Status",
                table: "CashReceipts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_JournalEntryId",
                table: "CashTransfers",
                column: "JournalEntryId",
                filter: "[JournalEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_SourceCashboxId",
                table: "CashTransfers",
                column: "SourceCashboxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_Status",
                table: "CashTransfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_TargetCashboxId",
                table: "CashTransfers",
                column: "TargetCashboxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_TransferDate",
                table: "CashTransfers",
                column: "TransferDate");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransfers_TransferNumber",
                table: "CashTransfers",
                column: "TransferNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashPayments");

            migrationBuilder.DropTable(
                name: "CashReceipts");

            migrationBuilder.DropTable(
                name: "CashTransfers");

            migrationBuilder.DropTable(
                name: "Cashboxes");
        }
    }
}
