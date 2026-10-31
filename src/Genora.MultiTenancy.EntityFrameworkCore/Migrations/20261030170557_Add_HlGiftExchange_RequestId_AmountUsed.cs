using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_HlGiftExchange_RequestId_AmountUsed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountUsed",
                schema: "HL",
                table: "AppHlGiftExchanges",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                schema: "HL",
                table: "AppHlGiftExchanges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftExchanges_TenantId_CustomerPhone",
                schema: "HL",
                table: "AppHlGiftExchanges",
                columns: new[] { "TenantId", "CustomerPhone" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftExchanges_TenantId_RequestId",
                schema: "HL",
                table: "AppHlGiftExchanges",
                columns: new[] { "TenantId", "RequestId" },
                unique: true,
                filter: "[RequestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppHlGiftExchanges_TenantId_CustomerPhone",
                schema: "HL",
                table: "AppHlGiftExchanges");

            migrationBuilder.DropIndex(
                name: "IX_AppHlGiftExchanges_TenantId_RequestId",
                schema: "HL",
                table: "AppHlGiftExchanges");

            migrationBuilder.DropColumn(
                name: "AmountUsed",
                schema: "HL",
                table: "AppHlGiftExchanges");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "HL",
                table: "AppHlGiftExchanges");
        }
    }
}
