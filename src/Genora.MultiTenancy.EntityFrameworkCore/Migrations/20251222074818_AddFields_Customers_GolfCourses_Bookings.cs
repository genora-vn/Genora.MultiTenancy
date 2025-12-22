using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddFields_Customers_GolfCourses_Bookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrameTimes",
                table: "AppGolfCourses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberHoles",
                table: "AppGolfCourses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Utilities",
                table: "AppGolfCourses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppCustomers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BonusPoint",
                table: "AppCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsFollower",
                table: "AppCustomers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSensitive",
                table: "AppCustomers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipTierId",
                table: "AppCustomers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VgaCode",
                table: "AppCustomers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExportInvoice",
                table: "AppBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "NumberHole",
                table: "AppBookings",
                type: "smallint",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ultility",
                table: "AppBookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerPlayer",
                table: "AppBookingPlayers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VgaCode",
                table: "AppBookingPlayers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_MembershipTierId",
                table: "AppCustomers",
                column: "MembershipTierId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCustomers_AppMembershipTiers_MembershipTierId",
                table: "AppCustomers",
                column: "MembershipTierId",
                principalTable: "AppMembershipTiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCustomers_AppMembershipTiers_MembershipTierId",
                table: "AppCustomers");

            migrationBuilder.DropIndex(
                name: "IX_AppCustomers_MembershipTierId",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "FrameTimes",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "NumberHoles",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "Utilities",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "BonusPoint",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "IsFollower",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "IsSensitive",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "MembershipTierId",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "VgaCode",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "IsExportInvoice",
                table: "AppBookings");

            migrationBuilder.DropColumn(
                name: "NumberHole",
                table: "AppBookings");

            migrationBuilder.DropColumn(
                name: "Ultility",
                table: "AppBookings");

            migrationBuilder.DropColumn(
                name: "PricePerPlayer",
                table: "AppBookingPlayers");

            migrationBuilder.DropColumn(
                name: "VgaCode",
                table: "AppBookingPlayers");
        }
    }
}
