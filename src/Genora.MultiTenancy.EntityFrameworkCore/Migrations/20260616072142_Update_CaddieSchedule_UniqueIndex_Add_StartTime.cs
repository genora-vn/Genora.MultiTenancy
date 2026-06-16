using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Update_CaddieSchedule_UniqueIndex_Add_StartTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift",
                table: "AppCaddieSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift_Start",
                table: "AppCaddieSchedules",
                columns: new[] { "TenantId", "CaddieId", "WorkDate", "ShiftCode", "StartTime" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift_Start",
                table: "AppCaddieSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift",
                table: "AppCaddieSchedules",
                columns: new[] { "TenantId", "CaddieId", "WorkDate", "ShiftCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }
    }
}
