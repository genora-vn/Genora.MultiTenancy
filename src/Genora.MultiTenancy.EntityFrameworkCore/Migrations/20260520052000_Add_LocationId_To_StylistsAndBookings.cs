using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_LocationId_To_StylistsAndBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyStylists_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyStylists_TenantId_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists",
                columns: new[] { "TenantId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                columns: new[] { "TenantId", "LocationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppSalonBeautyBookings_AppSalonBeautyLocations_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "LocationId",
                principalSchema: "Salon",
                principalTable: "AppSalonBeautyLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSalonBeautyStylists_AppSalonBeautyLocations_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists",
                column: "LocationId",
                principalSchema: "Salon",
                principalTable: "AppSalonBeautyLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSalonBeautyBookings_AppSalonBeautyLocations_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSalonBeautyStylists_AppSalonBeautyLocations_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyStylists_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyStylists_TenantId_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyBookings_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "Salon",
                table: "AppSalonBeautyStylists");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");
        }
    }
}
