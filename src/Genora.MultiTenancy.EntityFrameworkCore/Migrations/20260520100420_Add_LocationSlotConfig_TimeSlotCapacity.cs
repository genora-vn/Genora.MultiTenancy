using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_LocationSlotConfig_TimeSlotCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookedCount",
                schema: "Salon",
                table: "AppSalonBeautyTimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                schema: "Salon",
                table: "AppSalonBeautyTimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualOverride",
                schema: "Salon",
                table: "AppSalonBeautyTimeSlots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BufferTime",
                schema: "Salon",
                table: "AppSalonBeautyLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacityPerSlot",
                schema: "Salon",
                table: "AppSalonBeautyLocations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SlotDuration",
                schema: "Salon",
                table: "AppSalonBeautyLocations",
                type: "int",
                nullable: false,
                defaultValue: 60);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookedCount",
                schema: "Salon",
                table: "AppSalonBeautyTimeSlots");

            migrationBuilder.DropColumn(
                name: "Capacity",
                schema: "Salon",
                table: "AppSalonBeautyTimeSlots");

            migrationBuilder.DropColumn(
                name: "IsManualOverride",
                schema: "Salon",
                table: "AppSalonBeautyTimeSlots");

            migrationBuilder.DropColumn(
                name: "BufferTime",
                schema: "Salon",
                table: "AppSalonBeautyLocations");

            migrationBuilder.DropColumn(
                name: "MaxCapacityPerSlot",
                schema: "Salon",
                table: "AppSalonBeautyLocations");

            migrationBuilder.DropColumn(
                name: "SlotDuration",
                schema: "Salon",
                table: "AppSalonBeautyLocations");
        }
    }
}
