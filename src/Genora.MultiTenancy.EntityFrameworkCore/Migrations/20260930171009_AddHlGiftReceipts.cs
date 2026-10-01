using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlGiftReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHlGiftReceipts",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiptCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CampaignCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CampaignName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CampaignPeriod = table.Column<int>(type: "int", nullable: false),
                    CampaignStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CampaignEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoucherCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VoucherType = table.Column<int>(type: "int", nullable: false),
                    VoucherValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MembershipTier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccumulatedSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AccumulatedPoints = table.Column<int>(type: "int", nullable: true),
                    DsrCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DsrName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DistributorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DistributorName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlGiftReceipts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftReceipts_TenantId_ConfirmedAt",
                schema: "HL",
                table: "AppHlGiftReceipts",
                columns: new[] { "TenantId", "ConfirmedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftReceipts_TenantId_PhoneNumber_CustCode_ConfirmedAt",
                schema: "HL",
                table: "AppHlGiftReceipts",
                columns: new[] { "TenantId", "PhoneNumber", "CustCode", "ConfirmedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_HlGiftReceipts_Entitlement",
                schema: "HL",
                table: "AppHlGiftReceipts",
                columns: new[] { "TenantId", "CustCode", "CampaignCode", "CampaignPeriod", "VoucherCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlGiftReceipts",
                schema: "HL");
        }
    }
}
