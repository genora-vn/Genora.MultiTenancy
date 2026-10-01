using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlGiftReceiptHostUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_HlGiftReceipts_HostEntitlement",
                schema: "HL",
                table: "AppHlGiftReceipts",
                columns: new[] { "CustCode", "CampaignCode", "CampaignPeriod", "VoucherCode" },
                unique: true,
                filter: "[TenantId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_HlGiftReceipts_HostEntitlement",
                schema: "HL",
                table: "AppHlGiftReceipts");
        }
    }
}
