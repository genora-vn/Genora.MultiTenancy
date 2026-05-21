using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_TimeSlotId_To_SalonBeautyBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                columns: new[] { "TenantId", "TimeSlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalonBeautyBookings_TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "TimeSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSalonBeautyBookings_AppSalonBeautyTimeSlots_TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings",
                column: "TimeSlotId",
                principalSchema: "Salon",
                principalTable: "AppSalonBeautyTimeSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSalonBeautyBookings_AppSalonBeautyTimeSlots_TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyBookings_TenantId_TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");

            migrationBuilder.DropIndex(
                name: "IX_AppSalonBeautyBookings_TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");

            migrationBuilder.DropColumn(
                name: "TimeSlotId",
                schema: "Salon",
                table: "AppSalonBeautyBookings");
        }
    }
}
