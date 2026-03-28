using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Added_PaymentInfo_GolfCourse_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentQrBankAccount",
                table: "AppGolfCourses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrBankCode",
                table: "AppGolfCourses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrBankDisplay",
                table: "AppGolfCourses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrText",
                table: "AppGolfCourses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentQrBankAccount",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PaymentQrBankCode",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PaymentQrBankDisplay",
                table: "AppGolfCourses");

            migrationBuilder.DropColumn(
                name: "PaymentQrText",
                table: "AppGolfCourses");
        }
    }
}
