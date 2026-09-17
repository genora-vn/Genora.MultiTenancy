using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHlgGames",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewardDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaseScorePerQuestion = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlgGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlgGameSessions",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentIndex = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlgGameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHlgQuestions",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TimeLimitSec = table.Column<int>(type: "int", nullable: false),
                    ScoreMultiplier = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    CorrectKey = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlgQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgQuestions_AppHlgGames_GameId",
                        column: x => x.GameId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppHlgSessionAnswers",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedKey = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ScoreGained = table.Column<int>(type: "int", nullable: false),
                    TimeSpentSec = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlgSessionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgSessionAnswers_AppHlgGameSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgGameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppHlgAnswerOptions",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<byte>(type: "tinyint", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlgAnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgAnswerOptions_AppHlgQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgAnswerOptions_QuestionId",
                schema: "HLG",
                table: "AppHlgAnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgAnswerOptions_TenantId_QuestionId",
                schema: "HLG",
                table: "AppHlgAnswerOptions",
                columns: new[] { "TenantId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgGames_TenantId_Status_DisplayOrder",
                schema: "HLG",
                table: "AppHlgGames",
                columns: new[] { "TenantId", "Status", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgGameSessions_TenantId_CustomerId_GameId",
                schema: "HLG",
                table: "AppHlgGameSessions",
                columns: new[] { "TenantId", "CustomerId", "GameId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgQuestions_GameId",
                schema: "HLG",
                table: "AppHlgQuestions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgQuestions_TenantId_GameId_Index",
                schema: "HLG",
                table: "AppHlgQuestions",
                columns: new[] { "TenantId", "GameId", "Index" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgSessionAnswers_SessionId",
                schema: "HLG",
                table: "AppHlgSessionAnswers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgSessionAnswers_TenantId_SessionId",
                schema: "HLG",
                table: "AppHlgSessionAnswers",
                columns: new[] { "TenantId", "SessionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlgAnswerOptions",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgSessionAnswers",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgQuestions",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgGameSessions",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgGames",
                schema: "HLG");
        }
    }
}
