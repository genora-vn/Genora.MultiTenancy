using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHl25ProgramInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Format",
                schema: "hl25",
                table: "AppHl25AppConfig",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiftDeliveryTime",
                schema: "hl25",
                table: "AppHl25AppConfig",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntroductionHtml",
                schema: "hl25",
                table: "AppHl25AppConfig",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                schema: "hl25",
                table: "AppHl25AppConfig");

            migrationBuilder.DropColumn(
                name: "GiftDeliveryTime",
                schema: "hl25",
                table: "AppHl25AppConfig");

            migrationBuilder.DropColumn(
                name: "IntroductionHtml",
                schema: "hl25",
                table: "AppHl25AppConfig");
        }
    }
}
