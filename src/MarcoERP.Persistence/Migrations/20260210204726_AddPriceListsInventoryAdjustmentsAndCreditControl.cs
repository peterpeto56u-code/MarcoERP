using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceListsInventoryAdjustmentsAndCreditControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesRepresentativeId",
                table: "SalesInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BlockedOnOverdue",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DaysAllowed",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultSalesRepresentativeId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceListId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "date", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalCostDifference = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesRepresentatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CommissionBasedOn = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRepresentatives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustmentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryAdjustmentId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DifferenceQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DifferenceInBaseUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostDifference = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustmentLines_InventoryAdjustments_InventoryAdjustmentId",
                        column: x => x.InventoryAdjustmentId,
                        principalTable: "InventoryAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustmentLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustmentLines_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceListId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MinimumQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceTiers_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceTiers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_SalesRepresentativeId",
                table: "SalesInvoices",
                column: "SalesRepresentativeId",
                filter: "[SalesRepresentativeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DefaultSalesRepresentativeId",
                table: "Customers",
                column: "DefaultSalesRepresentativeId",
                filter: "[DefaultSalesRepresentativeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PriceListId",
                table: "Customers",
                column: "PriceListId",
                filter: "[PriceListId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_AdjustmentId",
                table: "InventoryAdjustmentLines",
                column: "InventoryAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_ProductId",
                table: "InventoryAdjustmentLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_UnitId",
                table: "InventoryAdjustmentLines",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_Date",
                table: "InventoryAdjustments",
                column: "AdjustmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_JournalEntryId",
                table: "InventoryAdjustments",
                column: "JournalEntryId",
                filter: "[JournalEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_Number",
                table: "InventoryAdjustments",
                column: "AdjustmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_Status",
                table: "InventoryAdjustments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_WarehouseId",
                table: "InventoryAdjustments",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_Code",
                table: "PriceLists",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_IsActive",
                table: "PriceLists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTiers_List_Product_MinQty",
                table: "PriceTiers",
                columns: new[] { "PriceListId", "ProductId", "MinimumQuantity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceTiers_PriceListId",
                table: "PriceTiers",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTiers_ProductId",
                table: "PriceTiers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRepresentatives_Code",
                table: "SalesRepresentatives",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesRepresentatives_IsActive",
                table: "SalesRepresentatives",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_PriceLists_PriceListId",
                table: "Customers",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_SalesRepresentatives_DefaultSalesRepresentativeId",
                table: "Customers",
                column: "DefaultSalesRepresentativeId",
                principalTable: "SalesRepresentatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_SalesRepresentatives_SalesRepresentativeId",
                table: "SalesInvoices",
                column: "SalesRepresentativeId",
                principalTable: "SalesRepresentatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_PriceLists_PriceListId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_SalesRepresentatives_DefaultSalesRepresentativeId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_SalesRepresentatives_SalesRepresentativeId",
                table: "SalesInvoices");

            migrationBuilder.DropTable(
                name: "InventoryAdjustmentLines");

            migrationBuilder.DropTable(
                name: "PriceTiers");

            migrationBuilder.DropTable(
                name: "SalesRepresentatives");

            migrationBuilder.DropTable(
                name: "InventoryAdjustments");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_SalesRepresentativeId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DefaultSalesRepresentativeId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PriceListId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SalesRepresentativeId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "BlockedOnOverdue",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DaysAllowed",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultSalesRepresentativeId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "Customers");
        }
    }
}
