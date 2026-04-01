using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_GolfCourse_Cancellation_Policy_Hours_And_PromotionTypeIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CancellationPolicyHours",
                table: "AppGolfCourses",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromotionTypeIds",
                table: "AppGolfCourses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationPolicyHours",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PromotionTypeIds",
                table: "AppGolfCourses");
        }
    }
}
