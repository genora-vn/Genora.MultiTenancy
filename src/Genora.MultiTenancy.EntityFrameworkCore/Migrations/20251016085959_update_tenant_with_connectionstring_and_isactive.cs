using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class update_tenant_with_connectionstring_and_isactive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                            name: "ConnectionString",
                            table: "AbpTenants",
                            type: "nvarchar(max)",
                            nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AbpTenants",
                type: "bit",
                nullable: false,
                defaultValue: false); // hoặc false tùy theo logic mặc định
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                            name: "ConnectionString",
                            table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AbpTenants");
        }
    }
}
