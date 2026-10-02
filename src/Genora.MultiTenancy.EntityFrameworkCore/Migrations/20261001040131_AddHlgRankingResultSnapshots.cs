using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgRankingResultSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHlgRankingResultSnapshots",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventRank = table.Column<int>(type: "int", nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlayerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ZaloUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GameName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    GameScore = table.Column<int>(type: "int", nullable: false),
                    BestScore = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswerCount = table.Column<int>(type: "int", nullable: false),
                    TotalQuestionCount = table.Column<int>(type: "int", nullable: false),
                    EventScore = table.Column<int>(type: "int", nullable: false),
                    FirstPlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHlgRankingResultSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgRankingResultSnapshots_AppHlgRankingEvents_EventId",
                        column: x => x.EventId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgRankingEvents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingResultSnapshots_EventId",
                schema: "HLG",
                table: "AppHlgRankingResultSnapshots",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingResultSnapshots_TenantId_EventId_CustomerId_GameId",
                schema: "HLG",
                table: "AppHlgRankingResultSnapshots",
                columns: new[] { "TenantId", "EventId", "CustomerId", "GameId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingResultSnapshots_TenantId_EventId_EventRank",
                schema: "HLG",
                table: "AppHlgRankingResultSnapshots",
                columns: new[] { "TenantId", "EventId", "EventRank" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlgRankingResultSnapshots",
                schema: "HLG");
        }
    }
}
