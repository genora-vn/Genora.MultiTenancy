using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppSpecialDates_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "AppCalendarSlotPrices",
                newName: "Price18");

            migrationBuilder.AddColumn<decimal>(
                name: "Price27",
                table: "AppCalendarSlotPrices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price36",
                table: "AppCalendarSlotPrices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price9",
                table: "AppCalendarSlotPrices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppSpecialDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GolfCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DatesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_AppSpecialDates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSpecialDates_TenantId_GolfCourseId_Name",
                table: "AppSpecialDates",
                columns: new[] { "TenantId", "GolfCourseId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [GolfCourseId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSpecialDates");

            migrationBuilder.DropColumn(
                name: "Price27",
                table: "AppCalendarSlotPrices");

            migrationBuilder.DropColumn(
                name: "Price36",
                table: "AppCalendarSlotPrices");

            migrationBuilder.DropColumn(
                name: "Price9",
                table: "AppCalendarSlotPrices");

            migrationBuilder.RenameColumn(
                name: "Price18",
                table: "AppCalendarSlotPrices",
                newName: "Price");
        }
    }
}
