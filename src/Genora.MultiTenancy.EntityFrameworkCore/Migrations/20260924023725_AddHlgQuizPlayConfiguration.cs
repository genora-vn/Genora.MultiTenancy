using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgQuizPlayConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowedWrongAnswers",
                schema: "HLG",
                table: "AppHlgGames",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionsPerPlay",
                schema: "HLG",
                table: "AppHlgGames",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AllowedWrongAnswers",
                schema: "HLG",
                table: "AppHlgGameSessions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedWrongAnswers",
                schema: "HLG",
                table: "AppHlgGames");

            migrationBuilder.DropColumn(
                name: "QuestionsPerPlay",
                schema: "HLG",
                table: "AppHlgGames");

            migrationBuilder.DropColumn(
                name: "AllowedWrongAnswers",
                schema: "HLG",
                table: "AppHlgGameSessions");
        }
    }
}
