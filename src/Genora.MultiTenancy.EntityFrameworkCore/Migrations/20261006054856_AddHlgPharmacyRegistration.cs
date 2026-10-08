using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgPharmacyRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DmsCustomerCode",
                schema: "HLG",
                table: "AppHlgUserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PharmaPhone",
                schema: "HLG",
                table: "AppHlgUserProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgUserProfiles_TenantId_PharmaPhone",
                schema: "HLG",
                table: "AppHlgUserProfiles",
                columns: new[] { "TenantId", "PharmaPhone" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppHlgUserProfiles_TenantId_PharmaPhone",
                schema: "HLG",
                table: "AppHlgUserProfiles");

            migrationBuilder.DropColumn(
                name: "DmsCustomerCode",
                schema: "HLG",
                table: "AppHlgUserProfiles");

            migrationBuilder.DropColumn(
                name: "PharmaPhone",
                schema: "HLG",
                table: "AppHlgUserProfiles");
        }
    }
}
