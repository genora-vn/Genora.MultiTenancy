using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlBlouseModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHlBlouseCampaigns",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProgramName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IntroductionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreeShirtLimit = table.Column<int>(type: "int", nullable: false),
                    PointsPerShirt = table.Column<int>(type: "int", nullable: false),
                    MaxExchangeShirt = table.Column<int>(type: "int", nullable: false),
                    SizeChartImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AppHlBlouseCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlBlouseRegistrations",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegistrationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ZaloUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiverName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusinessType = table.Column<byte>(type: "tinyint", nullable: true),
                    BusinessTypeName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StoreName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PrintedName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreeQuantity = table.Column<int>(type: "int", nullable: false),
                    ExchangeQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalPointsUsed = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    InternalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AppHlBlouseRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlBlouseSizes",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Style = table.Column<byte>(type: "tinyint", nullable: false),
                    SizeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WeightRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AppHlBlouseSizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlBlouseRegistrationItems",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<byte>(type: "tinyint", nullable: false),
                    Style = table.Column<byte>(type: "tinyint", nullable: false),
                    SizeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SizeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WeightRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PointsPerItem = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlBlouseRegistrationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlBlouseRegistrationItems_AppHlBlouseRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "HL",
                        principalTable: "AppHlBlouseRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseCampaigns_TenantId_IsActive",
                schema: "HL",
                table: "AppHlBlouseCampaigns",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseRegistrationItems_RegistrationId",
                schema: "HL",
                table: "AppHlBlouseRegistrationItems",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseRegistrationItems_TenantId_RegistrationId",
                schema: "HL",
                table: "AppHlBlouseRegistrationItems",
                columns: new[] { "TenantId", "RegistrationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseRegistrations_TenantId_Code",
                schema: "HL",
                table: "AppHlBlouseRegistrations",
                columns: new[] { "TenantId", "RegistrationCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseRegistrations_TenantId_CreationTime",
                schema: "HL",
                table: "AppHlBlouseRegistrations",
                columns: new[] { "TenantId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseRegistrations_TenantId_CustomerCode",
                schema: "HL",
                table: "AppHlBlouseRegistrations",
                columns: new[] { "TenantId", "CustomerCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseRegistrations_TenantId_Status",
                schema: "HL",
                table: "AppHlBlouseRegistrations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseSizes_TenantId_IsActive_Style",
                schema: "HL",
                table: "AppHlBlouseSizes",
                columns: new[] { "TenantId", "IsActive", "Style" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlBlouseSizes_TenantId_Style_SizeCode",
                schema: "HL",
                table: "AppHlBlouseSizes",
                columns: new[] { "TenantId", "Style", "SizeCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlBlouseCampaigns",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlBlouseRegistrationItems",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlBlouseSizes",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlBlouseRegistrations",
                schema: "HL");
        }
    }
}
