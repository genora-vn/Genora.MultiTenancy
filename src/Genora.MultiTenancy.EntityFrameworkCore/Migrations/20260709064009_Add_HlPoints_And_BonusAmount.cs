using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_HlPoints_And_BonusAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BonusAmount",
                table: "AppCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AppHlPointBatches",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CampaignCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CampaignName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CampaignPeriod = table.Column<int>(type: "int", nullable: true),
                    DisplayType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MembershipTier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConvertedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ExchangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlPointBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlPointTransactions",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Unit = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancePointAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAmountAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlPointTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlPointBatches_TenantId_BatchCode",
                schema: "HL",
                table: "AppHlPointBatches",
                columns: new[] { "TenantId", "BatchCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlPointBatches_TenantId_Customer_Campaign",
                schema: "HL",
                table: "AppHlPointBatches",
                columns: new[] { "TenantId", "CustomerCode", "CampaignCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [CustomerCode] IS NOT NULL AND [CampaignCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlPointBatches_TenantId_CustomerCode",
                schema: "HL",
                table: "AppHlPointBatches",
                columns: new[] { "TenantId", "CustomerCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlPointBatches_TenantId_Status_ExpireDate",
                schema: "HL",
                table: "AppHlPointBatches",
                columns: new[] { "TenantId", "Status", "ExpireDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlPointTransactions_TenantId_Customer_Time",
                schema: "HL",
                table: "AppHlPointTransactions",
                columns: new[] { "TenantId", "CustomerCode", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlPointTransactions_TenantId_Type",
                schema: "HL",
                table: "AppHlPointTransactions",
                columns: new[] { "TenantId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlPointBatches",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlPointTransactions",
                schema: "HL");

            migrationBuilder.DropColumn(
                name: "BonusAmount",
                table: "AppCustomers");
        }
    }
}
