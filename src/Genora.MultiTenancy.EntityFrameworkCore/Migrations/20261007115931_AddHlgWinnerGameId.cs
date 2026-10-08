using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgWinnerGameId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppHlgRankingWinners_TenantId_EventId_CustomerId",
                schema: "HLG",
                table: "AppHlgRankingWinners");

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingWinners_GameId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingWinners_TenantId_EventId_GameId_CustomerId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                columns: new[] { "TenantId", "EventId", "GameId", "CustomerId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHlgRankingWinners_AppHlgGames_GameId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                column: "GameId",
                principalSchema: "HLG",
                principalTable: "AppHlgGames",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHlgRankingWinners_AppHlgGames_GameId",
                schema: "HLG",
                table: "AppHlgRankingWinners");

            migrationBuilder.DropIndex(
                name: "IX_AppHlgRankingWinners_GameId",
                schema: "HLG",
                table: "AppHlgRankingWinners");

            migrationBuilder.DropIndex(
                name: "IX_AppHlgRankingWinners_TenantId_EventId_GameId_CustomerId",
                schema: "HLG",
                table: "AppHlgRankingWinners");

            migrationBuilder.DropColumn(
                name: "GameId",
                schema: "HLG",
                table: "AppHlgRankingWinners");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingWinners_TenantId_EventId_CustomerId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                columns: new[] { "TenantId", "EventId", "CustomerId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
