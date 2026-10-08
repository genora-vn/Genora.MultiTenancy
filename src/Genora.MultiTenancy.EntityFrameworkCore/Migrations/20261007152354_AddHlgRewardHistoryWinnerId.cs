using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgRewardHistoryWinnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WinnerId",
                schema: "HLG",
                table: "AppHlgRewardHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRewardHistories_WinnerId",
                schema: "HLG",
                table: "AppHlgRewardHistories",
                column: "WinnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppHlgRewardHistories_WinnerId",
                schema: "HLG",
                table: "AppHlgRewardHistories");

            migrationBuilder.DropColumn(
                name: "WinnerId",
                schema: "HLG",
                table: "AppHlgRewardHistories");
        }
    }
}
