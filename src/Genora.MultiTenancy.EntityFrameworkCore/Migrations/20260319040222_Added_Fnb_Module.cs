using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Added_Fnb_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppFnbCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_AppFnbCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppFnbOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BagTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelReason = table.Column<byte>(type: "tinyint", nullable: true),
                    CancelNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AppFnbOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFnbOrders_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppFnbItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_AppFnbItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFnbItems_AppFnbCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AppFnbCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppFnbOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFnbOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFnbOrderItems_AppFnbItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "AppFnbItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppFnbOrderItems_AppFnbOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppFnbOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbCategories_TenantId_Code",
                table: "AppFnbCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbCategories_TenantId_SortOrder",
                table: "AppFnbCategories",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbItems_CategoryId",
                table: "AppFnbItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbItems_TenantId_CategoryId_SortOrder",
                table: "AppFnbItems",
                columns: new[] { "TenantId", "CategoryId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbItems_TenantId_Name",
                table: "AppFnbItems",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrderItems_ItemId",
                table: "AppFnbOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrderItems_OrderId",
                table: "AppFnbOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrders_CustomerId",
                table: "AppFnbOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrders_TenantId_BagTag",
                table: "AppFnbOrders",
                columns: new[] { "TenantId", "BagTag" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrders_TenantId_CreationTime",
                table: "AppFnbOrders",
                columns: new[] { "TenantId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrders_TenantId_OrderCode",
                table: "AppFnbOrders",
                columns: new[] { "TenantId", "OrderCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppFnbOrders_TenantId_ServiceStatus_PaymentStatus",
                table: "AppFnbOrders",
                columns: new[] { "TenantId", "ServiceStatus", "PaymentStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppFnbOrderItems");

            migrationBuilder.DropTable(
                name: "AppFnbItems");

            migrationBuilder.DropTable(
                name: "AppFnbOrders");

            migrationBuilder.DropTable(
                name: "AppFnbCategories");
        }
    }
}
