using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Drop_FK_AppProOrders_AppCustomers_CustomerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProOrders_AppCustomers_CustomerId",
                table: "AppProOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_AppProOrders_AppCustomers_CustomerId",
                table: "AppProOrders",
                column: "CustomerId",
                principalTable: "AppCustomers",
                principalColumn: "Id");
        }
    }
}
