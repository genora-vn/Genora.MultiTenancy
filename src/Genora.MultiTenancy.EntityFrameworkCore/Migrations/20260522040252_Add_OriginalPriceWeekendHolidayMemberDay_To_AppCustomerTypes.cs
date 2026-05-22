using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_OriginalPriceWeekendHolidayMemberDay_To_AppCustomerTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPriceHoliday",
                table: "AppCustomerTypes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPriceMemberDay",
                table: "AppCustomerTypes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPriceWeekend",
                table: "AppCustomerTypes",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPriceHoliday",
                table: "AppCustomerTypes");

            migrationBuilder.DropColumn(
                name: "OriginalPriceMemberDay",
                table: "AppCustomerTypes");

            migrationBuilder.DropColumn(
                name: "OriginalPriceWeekend",
                table: "AppCustomerTypes");
        }
    }
}
