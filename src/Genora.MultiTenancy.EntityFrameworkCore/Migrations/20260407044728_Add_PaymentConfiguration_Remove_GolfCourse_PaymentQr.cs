using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_PaymentConfiguration_Remove_GolfCourse_PaymentQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentQrBankAccount",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PaymentQrBankCode",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PaymentQrBankDisplay",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PaymentQrText",
                table: "AppGolfCourses");

            migrationBuilder.CreateTable(
                name: "AppPaymentConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankBin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MerchantId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AppPaymentConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPaymentConfigurations_TenantId_DisplayOrder",
                table: "AppPaymentConfigurations",
                columns: new[] { "TenantId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPaymentConfigurations_TenantId_IsActive",
                table: "AppPaymentConfigurations",
                columns: new[] { "TenantId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPaymentConfigurations");

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrBankAccount",
                table: "AppGolfCourses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrBankCode",
                table: "AppGolfCourses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrBankDisplay",
                table: "AppGolfCourses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrText",
                table: "AppGolfCourses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
