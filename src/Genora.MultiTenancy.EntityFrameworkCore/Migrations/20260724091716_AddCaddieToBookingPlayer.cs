using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddCaddieToBookingPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CaddieBookingId",
                table: "AppBookingPlayers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CaddieId",
                table: "AppBookingPlayers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaddieName",
                table: "AppBookingPlayers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaddieBookingId",
                table: "AppBookingPlayers");

            migrationBuilder.DropColumn(
                name: "CaddieId",
                table: "AppBookingPlayers");

            migrationBuilder.DropColumn(
                name: "CaddieName",
                table: "AppBookingPlayers");
        }
    }
}
