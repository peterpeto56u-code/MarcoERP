using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcoERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCascadeDeleteViolations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankReconciliationItems_BankReconciliations_BankReconciliationId",
                table: "BankReconciliationItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceTiers_PriceLists_PriceListId",
                table: "PriceTiers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnits_Products_ProductId",
                table: "ProductUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileFeatures_SystemProfiles_ProfileId",
                table: "ProfileFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.AddForeignKey(
                name: "FK_BankReconciliationItems_BankReconciliations_BankReconciliationId",
                table: "BankReconciliationItems",
                column: "BankReconciliationId",
                principalTable: "BankReconciliations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceTiers_PriceLists_PriceListId",
                table: "PriceTiers",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnits_Products_ProductId",
                table: "ProductUnits",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileFeatures_SystemProfiles_ProfileId",
                table: "ProfileFeatures",
                column: "ProfileId",
                principalTable: "SystemProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankReconciliationItems_BankReconciliations_BankReconciliationId",
                table: "BankReconciliationItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceTiers_PriceLists_PriceListId",
                table: "PriceTiers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnits_Products_ProductId",
                table: "ProductUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileFeatures_SystemProfiles_ProfileId",
                table: "ProfileFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.AddForeignKey(
                name: "FK_BankReconciliationItems_BankReconciliations_BankReconciliationId",
                table: "BankReconciliationItems",
                column: "BankReconciliationId",
                principalTable: "BankReconciliations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceTiers_PriceLists_PriceListId",
                table: "PriceTiers",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnits_Products_ProductId",
                table: "ProductUnits",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileFeatures_SystemProfiles_ProfileId",
                table: "ProfileFeatures",
                column: "ProfileId",
                principalTable: "SystemProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
