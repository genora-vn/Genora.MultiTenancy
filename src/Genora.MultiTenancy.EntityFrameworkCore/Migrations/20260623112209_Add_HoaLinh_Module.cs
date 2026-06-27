using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_HoaLinh_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "HL");

            migrationBuilder.CreateTable(
                name: "AppHlApiLogs",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CallerSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlApiLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlGiftExchanges",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExchangeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GiftName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GiftCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GiftImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PointsRequired = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalPointsUsed = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UrBoxVoucherCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UrBoxResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppHlGiftExchanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlOrders",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BranchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceiverName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReceiverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SystemDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalOrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSyncedToHl = table.Column<bool>(type: "bit", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AppHlOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlOrderItems",
                schema: "HL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProductGroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductGroupName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProductUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlOrderItems_AppHlOrders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "HL",
                        principalTable: "AppHlOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlApiLogs_TenantId_CreationTime",
                schema: "HL",
                table: "AppHlApiLogs",
                columns: new[] { "TenantId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlApiLogs_TenantId_DataType",
                schema: "HL",
                table: "AppHlApiLogs",
                columns: new[] { "TenantId", "DataType" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlApiLogs_TenantId_IsError",
                schema: "HL",
                table: "AppHlApiLogs",
                columns: new[] { "TenantId", "IsError" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftExchanges_TenantId_CustomerCode",
                schema: "HL",
                table: "AppHlGiftExchanges",
                columns: new[] { "TenantId", "CustomerCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftExchanges_TenantId_ExchangeCode",
                schema: "HL",
                table: "AppHlGiftExchanges",
                columns: new[] { "TenantId", "ExchangeCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlGiftExchanges_TenantId_Status",
                schema: "HL",
                table: "AppHlGiftExchanges",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlOrderItems_OrderId",
                schema: "HL",
                table: "AppHlOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlOrderItems_TenantId_OrderId",
                schema: "HL",
                table: "AppHlOrderItems",
                columns: new[] { "TenantId", "OrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlOrders_TenantId_CreationTime",
                schema: "HL",
                table: "AppHlOrders",
                columns: new[] { "TenantId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlOrders_TenantId_CustomerCode",
                schema: "HL",
                table: "AppHlOrders",
                columns: new[] { "TenantId", "CustomerCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlOrders_TenantId_OrderCode",
                schema: "HL",
                table: "AppHlOrders",
                columns: new[] { "TenantId", "OrderCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlOrders_TenantId_Status",
                schema: "HL",
                table: "AppHlOrders",
                columns: new[] { "TenantId", "DeliveryStatus", "PaymentStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlApiLogs",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlGiftExchanges",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlOrderItems",
                schema: "HL");

            migrationBuilder.DropTable(
                name: "AppHlOrders",
                schema: "HL");
        }
    }
}
