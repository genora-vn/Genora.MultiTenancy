using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AppCustomers_FilteredUnique_CustomerCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCustomers_TenantId_CustomerCode",
                table: "AppCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_TenantId_CustomerCode",
                table: "AppCustomers",
                columns: new[] { "TenantId", "CustomerCode" },
                unique: true,
                filter: "[IsActive] = 1 AND [CustomerCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCustomers_TenantId_CustomerCode",
                table: "AppCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_TenantId_CustomerCode",
                table: "AppCustomers",
                columns: new[] { "TenantId", "CustomerCode" },
                unique: true,
                filter: "[CustomerCode] IS NOT NULL");
        }
    }
}
