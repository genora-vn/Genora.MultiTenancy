using Genora.MultiTenancy.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    [DbContext(typeof(MultiTenancyDbContext))]
    [Migration("20260414090000_Add_Member_Fields_To_GolfCourse")]
    public partial class Add_Member_Fields_To_GolfCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMemberSupported",
                table: "AppGolfCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxMemberGuest",
                table: "AppGolfCourses",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMemberSupported",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "MaxMemberGuest",
                table: "AppGolfCourses");
        }
    }
}
