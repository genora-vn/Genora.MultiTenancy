using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_SalonBeauty_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Salon");

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyCustomers",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ZaloUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsFollowOa = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<byte>(type: "tinyint", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppSalonBeautyCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyServiceCategories",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppSalonBeautyServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyStylists",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    Role = table.Column<byte>(type: "tinyint", nullable: true),
                    Level = table.Column<byte>(type: "tinyint", nullable: true),
                    ExperienceYear = table.Column<int>(type: "int", nullable: false),
                    RatingAvg = table.Column<decimal>(type: "decimal(2,1)", precision: 2, scale: 1, nullable: false),
                    TotalBooking = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    IsShowOnApp = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppSalonBeautyStylists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyCustomerLoyaltyBalances",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentPoint = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_AppSalonBeautyCustomerLoyaltyBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyCustomerLoyaltyBalances_AppSalonBeautyCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyCustomerLoyaltyTransactions",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Point = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_AppSalonBeautyCustomerLoyaltyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyCustomerLoyaltyTransactions_AppSalonBeautyCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyServices",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    ApplicableRole = table.Column<byte>(type: "tinyint", nullable: true),
                    ApplicableLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    IsShowOnApp = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppSalonBeautyServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyServices_AppSalonBeautyServiceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyBookings",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StylistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    PaymentStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    PaymentMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    CheckinStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    CheckinTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelReason = table.Column<byte>(type: "tinyint", nullable: true),
                    CancelNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppSalonBeautyBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyBookings_AppSalonBeautyCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyBookings_AppSalonBeautyServices_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyBookings_AppSalonBeautyStylists_StylistId",
                        column: x => x.StylistId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyStylists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSalonBeautyBookingServices",
                schema: "Salon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSalonBeautyBookingServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyBookingServices_AppSalonBeautyBookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSalonBeautyBookingServices_AppSalonBeautyServices_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "Salon",
                        principalTable: "AppSalonBeautyServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_ServiceId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_StylistId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "StylistId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_BookingDate",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                columns: new[] { "TenantId", "BookingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_Code",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                columns: new[] { "TenantId", "BookingCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookingServices_BookingId",
                schema: "Salon",
                table: "AppSalonBeautyBookingServices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookingServices_ServiceId",
                schema: "Salon",
                table: "AppSalonBeautyBookingServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomerLoyaltyBalances_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyBalances",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomerLoyaltyBalances_TenantId_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyBalances",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomerLoyaltyTransactions_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomerLoyaltyTransactions_TenantId_CustomerId",
                schema: "Salon",
                table: "AppSalonBeautyCustomerLoyaltyTransactions",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomers_TenantId_Code",
                schema: "Salon",
                table: "AppSalonBeautyCustomers",
                columns: new[] { "TenantId", "CustomerCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomers_TenantId_Email",
                schema: "Salon",
                table: "AppSalonBeautyCustomers",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyCustomers_TenantId_Phone",
                schema: "Salon",
                table: "AppSalonBeautyCustomers",
                columns: new[] { "TenantId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyServiceCategories_TenantId_Name",
                schema: "Salon",
                table: "AppSalonBeautyServiceCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyServices_CategoryId",
                schema: "Salon",
                table: "AppSalonBeautyServices",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyServices_TenantId_CategoryId",
                schema: "Salon",
                table: "AppSalonBeautyServices",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyStylists_TenantId_DisplayName",
                schema: "Salon",
                table: "AppSalonBeautyStylists",
                columns: new[] { "TenantId", "DisplayName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSalonBeautyBookingServices",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyCustomerLoyaltyBalances",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyCustomerLoyaltyTransactions",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyBookings",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyCustomers",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyServices",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyStylists",
                schema: "Salon");

            migrationBuilder.DropTable(
                name: "AppSalonBeautyServiceCategories",
                schema: "Salon");
        }
    }
}
