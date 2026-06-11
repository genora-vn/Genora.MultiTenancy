using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaddieBookingRemoveCaddieIdAddPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCaddieBookings_AppCaddieSchedules_ScheduleId",
                table: "AppCaddieBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_AppCaddieBookings_AppCaddies_CaddieId",
                table: "AppCaddieBookings");

            migrationBuilder.DropIndex(
                name: "IX_AppCaddieBookings_CaddieId",
                table: "AppCaddieBookings");

            migrationBuilder.DropIndex(
                name: "IX_AppCaddieBookings_ScheduleId",
                table: "AppCaddieBookings");

            migrationBuilder.DropIndex(
                name: "IX_AppCaddieBookings_TenantId_Caddie_Date",
                table: "AppCaddieBookings");

            migrationBuilder.DropColumn(
                name: "CaddieId",
                table: "AppCaddieBookings");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "AppCaddieBookings");

            migrationBuilder.AddColumn<byte>(
                name: "PaymentMethod",
                table: "AppCaddieBookings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCaddieFee",
                table: "AppCaddieBookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "AppCaddieBookings");

            migrationBuilder.DropColumn(
                name: "TotalCaddieFee",
                table: "AppCaddieBookings");

            migrationBuilder.AddColumn<Guid>(
                name: "CaddieId",
                table: "AppCaddieBookings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                table: "AppCaddieBookings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_CaddieId",
                table: "AppCaddieBookings",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_ScheduleId",
                table: "AppCaddieBookings",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_TenantId_Caddie_Date",
                table: "AppCaddieBookings",
                columns: new[] { "TenantId", "CaddieId", "BookingDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppCaddieBookings_AppCaddieSchedules_ScheduleId",
                table: "AppCaddieBookings",
                column: "ScheduleId",
                principalTable: "AppCaddieSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppCaddieBookings_AppCaddies_CaddieId",
                table: "AppCaddieBookings",
                column: "CaddieId",
                principalTable: "AppCaddies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
