using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_SalonBeautyDeposit_LoyaltyBonusTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BalanceAfter",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BalanceBefore",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ReferenceType",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)99);

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyDepositTransactions",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BasePoint = table.Column<int>(type: "int", nullable: false),
                    BonusPoint = table.Column<int>(type: "int", nullable: false),
                    TotalPoint = table.Column<int>(type: "int", nullable: false),
                    BonusTierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentMethod = table.Column<byte>(type: "tinyint", nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppSalonBeautyDepositTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyDepositTransactions_AppSalonBeautyCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyLoyaltyBonusTiers",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BonusPoint = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppSalonBeautyLoyaltyBonusTiers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomerLoyaltyTransactions_TenantId_Ref",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyDepositTransactions_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyDepositTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyDepositTransactions_TenantId_Code",
                schema: "Salon",
                table: "AppSalonBeautyDepositTransactions",
                columns: new[] { "TenantId", "TransactionCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyDepositTransactions_TenantId_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyDepositTransactions",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyDepositTransactions_TenantId_Status",
                schema: "Salon",
                table: "AppSalonBeautyDepositTransactions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyLoyaltyBonusTiers_TenantId_MinAmount",
                schema: "Salon",
                table: "AppSalonBeautyLoyaltyBonusTiers",
                columns: new[] { "TenantId", "MinAmount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSalonBeautyDepositTransactions",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyLoyaltyBonusTiers",
                schema: "Salon");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyCustomerLoyaltyTransactions_TenantId_Ref",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceBefore",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions");
        }
    }
}
