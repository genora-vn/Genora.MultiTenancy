using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_SlotAvailable_To_CalendarSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotAvailable",
                table: "AppCalendarSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Khởi tạo SlotAvailable = MaxSlots cho tất cả row hiện có
            migrationBuilder.Sql("UPDATE AppCalendarSlots SET SlotAvailable = MaxSlots WHERE SlotAvailable = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotAvailable",
                table: "AppCalendarSlots");
        }
    }
}
