using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_Index_To_AppZaloLog_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppZaloLog_CreationTime",
                table: "AppZaloLog");

            migrationBuilder.CreateIndex(
                name: "IX_AppZaloLog_TenantId_CreationTime",
                table: "AppZaloLog",
                columns: new[] { "TenantId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppZaloLog_TenantId_CreationTime",
                table: "AppZaloLog");

            migrationBuilder.CreateIndex(
                name: "IX_AppZaloLog_CreationTime",
                table: "AppZaloLog",
                column: "CreationTime");
        }
    }
}
