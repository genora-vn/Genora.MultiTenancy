using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddCaddieFeeAndBookingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CaddieFee",
                table: "AppGolfCourses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "GolfCourseId",
                table: "AppCaddies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "AppCaddieBookingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaddieBookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaddieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCaddieBookingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieBookingDetails_AppCaddieBookings_CaddieBookingId",
                        column: x => x.CaddieBookingId,
                        principalTable: "AppCaddieBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCaddieBookingDetails_AppCaddieSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "AppCaddieSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCaddieBookingDetails_AppCaddies_CaddieId",
                        column: x => x.CaddieId,
                        principalTable: "AppCaddies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookingDetails_CaddieBookingId",
                table: "AppCaddieBookingDetails",
                column: "CaddieBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookingDetails_CaddieId",
                table: "AppCaddieBookingDetails",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookingDetails_ScheduleId",
                table: "AppCaddieBookingDetails",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookingDetails_TenantId_BookingId",
                table: "AppCaddieBookingDetails",
                columns: new[] { "TenantId", "CaddieBookingId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookingDetails_TenantId_CaddieId",
                table: "AppCaddieBookingDetails",
                columns: new[] { "TenantId", "CaddieId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCaddieBookingDetails");

            migrationBuilder.DropColumn(
                name: "CaddieFee",
                table: "AppGolfCourses");

            migrationBuilder.AlterColumn<Guid>(
                name: "GolfCourseId",
                table: "AppCaddies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
