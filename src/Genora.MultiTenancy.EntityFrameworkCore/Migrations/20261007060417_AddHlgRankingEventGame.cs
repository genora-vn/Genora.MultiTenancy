using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgRankingEventGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHlgRankingEventGames",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppHlgRankingEventGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgRankingEventGames_AppHlgGames_GameId",
                        column: x => x.GameId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgGames",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppHlgRankingEventGames_AppHlgRankingEvents_EventId",
                        column: x => x.EventId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgRankingEvents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingEventGames_EventId",
                schema: "HLG",
                table: "AppHlgRankingEventGames",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingEventGames_GameId",
                schema: "HLG",
                table: "AppHlgRankingEventGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingEventGames_TenantId_EventId_GameId",
                schema: "HLG",
                table: "AppHlgRankingEventGames",
                columns: new[] { "TenantId", "EventId", "GameId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHlgRankingEventGames",
                schema: "HLG");
        }
    }
}
