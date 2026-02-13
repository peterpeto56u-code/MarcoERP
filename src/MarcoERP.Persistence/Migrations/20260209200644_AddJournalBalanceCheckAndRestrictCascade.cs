using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalBalanceCheckAndRestrictCascade : Migration
    {
        private const string FkPurchaseInvoiceLines = "FK_PurchaseInvoiceLines_PurchaseInvoices_PurchaseInvoiceId";
        private const string FkPurchaseReturnLines = "FK_PurchaseReturnLines_PurchaseReturns_PurchaseReturnId";
        private const string FkSalesInvoiceLines = "FK_SalesInvoiceLines_SalesInvoices_SalesInvoiceId";
        private const string FkSalesReturnLines = "FK_SalesReturnLines_SalesReturns_SalesReturnId";
        private const string PurchaseInvoiceLinesTable = "PurchaseInvoiceLines";
        private const string PurchaseReturnLinesTable = "PurchaseReturnLines";
        private const string SalesInvoiceLinesTable = "SalesInvoiceLines";
        private const string SalesReturnLinesTable = "SalesReturnLines";
        private const string AccountIdColumn = "AccountId";
        private const string SuppliersTable = "Suppliers";
        private const string DeletedAtColumn = "DeletedAt";
        private const string DeletedByColumn = "DeletedBy";
        private const string IsDeletedColumn = "IsDeleted";
        private const string SalesReturnsTable = "SalesReturns";
        private const string SalesInvoicesTable = "SalesInvoices";
        private const string PurchaseReturnsTable = "PurchaseReturns";
        private const string PurchaseInvoicesTable = "PurchaseInvoices";
        private const string CustomersTable = "Customers";
        private const string CashTransfersTable = "CashTransfers";
        private const string CashReceiptsTable = "CashReceipts";
        private const string CashPaymentsTable = "CashPayments";
        private const string PosSessionsTable = "PosSessions";
        private const string PosPaymentsTable = "PosPayments";
        private const string InventoryMovementsTable = "InventoryMovements";
        private const string DateTime2Type = "datetime2";
        private const string Nvarchar100Type = "nvarchar(100)";
        private const string Decimal184Type = "decimal(18,4)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: FkPurchaseInvoiceLines,
                table: PurchaseInvoiceLinesTable);

            migrationBuilder.DropForeignKey(
                name: FkPurchaseReturnLines,
                table: PurchaseReturnLinesTable);

            migrationBuilder.DropForeignKey(
                name: FkSalesInvoiceLines,
                table: SalesInvoiceLinesTable);

            migrationBuilder.DropForeignKey(
                name: FkSalesReturnLines,
                table: SalesReturnLinesTable);

            migrationBuilder.AddColumn<int>(
                name: AccountIdColumn,
                table: SuppliersTable,
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: SalesReturnsTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: SalesReturnsTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: SalesReturnsTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: SalesInvoicesTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: SalesInvoicesTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: SalesInvoicesTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: PurchaseReturnsTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: PurchaseReturnsTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: PurchaseReturnsTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: PurchaseInvoicesTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: PurchaseInvoicesTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: PurchaseInvoicesTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: AccountIdColumn,
                table: CustomersTable,
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: CashTransfersTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: CashTransfersTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: CashTransfersTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: CashReceiptsTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: CashReceiptsTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: CashReceiptsTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: DeletedAtColumn,
                table: CashPaymentsTable,
                type: DateTime2Type,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: DeletedByColumn,
                table: CashPaymentsTable,
                type: Nvarchar100Type,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: IsDeletedColumn,
                table: CashPaymentsTable,
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BackupHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BackupDate = table.Column<DateTime>(type: DateTime2Type, nullable: false),
                    PerformedBy = table.Column<string>(type: Nvarchar100Type, maxLength: 100, nullable: false),
                    BackupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: DateTime2Type, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: PosSessionsTable,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CashboxId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    TotalSales = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    TotalCashReceived = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    TotalCardReceived = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    TotalOnAccount = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    TransactionCount = table.Column<int>(type: "int", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    Variance = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: DateTime2Type, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: DateTime2Type, nullable: true),
                    ClosingNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: DateTime2Type, nullable: false),
                    CreatedBy = table.Column<string>(type: Nvarchar100Type, maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: DateTime2Type, nullable: true),
                    ModifiedBy = table.Column<string>(type: Nvarchar100Type, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosSessions_Cashboxes_CashboxId",
                        column: x => x.CashboxId,
                        principalTable: "Cashboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosSessions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: PosPaymentsTable,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesInvoiceId = table.Column<int>(type: "int", nullable: false),
                    PosSessionId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: Decimal184Type, precision: 18, scale: 4, nullable: false),
                    ReferenceNumber = table.Column<string>(type: Nvarchar100Type, maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: DateTime2Type, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosPayments_PosSessions_PosSessionId",
                        column: x => x.PosSessionId,
                        principalTable: PosSessionsTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosPayments_SalesInvoices_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: SalesInvoicesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_AccountId",
                table: SuppliersTable,
                column: AccountIdColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_Source",
                table: "JournalEntries",
                columns: new[] { "SourceType", "SourceId" },
                filter: "[SourceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_MovementDate",
                table: InventoryMovementsTable,
                column: "MovementDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ProductId",
                table: InventoryMovementsTable,
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccountId",
                table: CustomersTable,
                column: AccountIdColumn);

            migrationBuilder.CreateIndex(
                name: "IX_BackupHistory_BackupDate",
                table: "BackupHistory",
                column: "BackupDate");

            migrationBuilder.CreateIndex(
                name: "IX_PosPayments_PaymentMethod",
                table: PosPaymentsTable,
                column: "PaymentMethod");

            migrationBuilder.CreateIndex(
                name: "IX_PosPayments_PosSessionId",
                table: PosPaymentsTable,
                column: "PosSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PosPayments_SalesInvoiceId",
                table: PosPaymentsTable,
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_CashboxId",
                table: PosSessionsTable,
                column: "CashboxId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_OpenedAt",
                table: PosSessionsTable,
                column: "OpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_SessionNumber",
                table: PosSessionsTable,
                column: "SessionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_Status",
                table: PosSessionsTable,
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_UserId",
                table: PosSessionsTable,
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_WarehouseId",
                table: PosSessionsTable,
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Accounts_AccountId",
                table: CustomersTable,
                column: AccountIdColumn,
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: FkPurchaseInvoiceLines,
                table: PurchaseInvoiceLinesTable,
                column: "PurchaseInvoiceId",
                principalTable: PurchaseInvoicesTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: FkPurchaseReturnLines,
                table: PurchaseReturnLinesTable,
                column: "PurchaseReturnId",
                principalTable: PurchaseReturnsTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: FkSalesInvoiceLines,
                table: SalesInvoiceLinesTable,
                column: "SalesInvoiceId",
                principalTable: SalesInvoicesTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: FkSalesReturnLines,
                table: SalesReturnLinesTable,
                column: "SalesReturnId",
                principalTable: SalesReturnsTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Accounts_AccountId",
                table: SuppliersTable,
                column: AccountIdColumn,
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ── Trigger: posted journal entries must balance ──
            migrationBuilder.Sql(@"
                CREATE OR ALTER TRIGGER TR_JournalEntries_EnforceBalance
                ON [JournalEntries]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        WHERE i.[Status] = 1  -- Posted
                        AND EXISTS (
                            SELECT 1
                            FROM [JournalEntryLines] jel
                            WHERE jel.[JournalEntryId] = i.[Id]
                            GROUP BY jel.[JournalEntryId]
                            HAVING ABS(SUM(jel.[DebitAmount]) - SUM(jel.[CreditAmount])) > 0.001
                        )
                    )
                    BEGIN
                        RAISERROR (N'لا يمكن ترحيل قيد غير متوازن. مجموع المدين يجب أن يساوي مجموع الدائن.', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── Drop balance-enforcement trigger ──
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_JournalEntries_EnforceBalance;");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Accounts_AccountId",
                table: CustomersTable);

            migrationBuilder.DropForeignKey(
                name: FkPurchaseInvoiceLines,
                table: PurchaseInvoiceLinesTable);

            migrationBuilder.DropForeignKey(
                name: FkPurchaseReturnLines,
                table: PurchaseReturnLinesTable);

            migrationBuilder.DropForeignKey(
                name: FkSalesInvoiceLines,
                table: SalesInvoiceLinesTable);

            migrationBuilder.DropForeignKey(
                name: FkSalesReturnLines,
                table: SalesReturnLinesTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Accounts_AccountId",
                table: SuppliersTable);

            migrationBuilder.DropTable(
                name: "BackupHistory");

            migrationBuilder.DropTable(
                name: PosPaymentsTable);

            migrationBuilder.DropTable(
                name: PosSessionsTable);

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_AccountId",
                table: SuppliersTable);

            migrationBuilder.DropIndex(
                name: "IX_Products_Status",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_Source",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_MovementDate",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_ProductId",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AccountId",
                table: CustomersTable);

            migrationBuilder.DropColumn(
                name: AccountIdColumn,
                table: SuppliersTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: SalesReturnsTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: SalesReturnsTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: SalesReturnsTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: SalesInvoicesTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: SalesInvoicesTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: SalesInvoicesTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: PurchaseReturnsTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: PurchaseReturnsTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: PurchaseReturnsTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: PurchaseInvoicesTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: PurchaseInvoicesTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: PurchaseInvoicesTable);

            migrationBuilder.DropColumn(
                name: AccountIdColumn,
                table: CustomersTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: CashTransfersTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: CashTransfersTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: CashTransfersTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: CashReceiptsTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: CashReceiptsTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: CashReceiptsTable);

            migrationBuilder.DropColumn(
                name: DeletedAtColumn,
                table: CashPaymentsTable);

            migrationBuilder.DropColumn(
                name: DeletedByColumn,
                table: CashPaymentsTable);

            migrationBuilder.DropColumn(
                name: IsDeletedColumn,
                table: CashPaymentsTable);

            migrationBuilder.AddForeignKey(
                name: FkPurchaseInvoiceLines,
                table: PurchaseInvoiceLinesTable,
                column: "PurchaseInvoiceId",
                principalTable: PurchaseInvoicesTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: FkPurchaseReturnLines,
                table: PurchaseReturnLinesTable,
                column: "PurchaseReturnId",
                principalTable: PurchaseReturnsTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: FkSalesInvoiceLines,
                table: SalesInvoiceLinesTable,
                column: "SalesInvoiceId",
                principalTable: SalesInvoicesTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: FkSalesReturnLines,
                table: SalesReturnLinesTable,
                column: "SalesReturnId",
                principalTable: SalesReturnsTable,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
