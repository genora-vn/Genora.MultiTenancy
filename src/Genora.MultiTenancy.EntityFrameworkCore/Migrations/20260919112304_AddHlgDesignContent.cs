using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHlgDesignContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A previously migrated database can have HLG history rows without the
            // physical HLG tables (observed on staging host). Fail before any DDL;
            // those baseline migrations must be replayed after a guarded repair.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[HLG].[AppHlgUserProfiles]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgKnowledgeCategories]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgLearningProgress]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgProducts]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgGames]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgGameSessions]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgQuestions]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgSessionAnswers]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgAnswerOptions]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgRewardHistories]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgRewards]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgShippingAddresses]', N'U') IS NULL
    OR OBJECT_ID(N'[HLG].[AppHlgRankingEvents]', N'U') IS NULL
    THROW 51023, 'HLG baseline tables are missing. Repair the HLG migration history/schema before AddHlgDesignContent.', 1;");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyCode",
                schema: "HLG",
                table: "AppHlgUserProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                schema: "HLG",
                table: "AppHlgRankingEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                schema: "HLG",
                table: "AppHlgProducts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailsJson",
                schema: "HLG",
                table: "AppHlgProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BadgeText",
                schema: "HLG",
                table: "AppHlgGames",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                schema: "HLG",
                table: "AppHlgGames",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppHlgBrands",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
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
                    table.PrimaryKey("PK_AppHlgBrands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgBrands_AppHlgKnowledgeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgKnowledgeCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppHlgContentItems",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BadgeText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TargetUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_AppHlgContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgContentItems_AppHlgGames_GameId",
                        column: x => x.GameId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgGames",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppHlgRankingPrizes",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RewardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppHlgRankingPrizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgRankingPrizes_AppHlgRankingEvents_EventId",
                        column: x => x.EventId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgRankingEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppHlgRankingPrizes_AppHlgRewards_RewardId",
                        column: x => x.RewardId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgRewards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppHlgRankingWinners",
                schema: "HLG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrizeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppHlgRankingWinners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHlgRankingWinners_AppHlgRankingEvents_EventId",
                        column: x => x.EventId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgRankingEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppHlgRankingWinners_AppHlgRankingPrizes_PrizeId",
                        column: x => x.PrizeId,
                        principalSchema: "HLG",
                        principalTable: "AppHlgRankingPrizes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingEvents_GameId",
                schema: "HLG",
                table: "AppHlgRankingEvents",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgProducts_BrandId",
                schema: "HLG",
                table: "AppHlgProducts",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgBrands_CategoryId",
                schema: "HLG",
                table: "AppHlgBrands",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgBrands_TenantId_CategoryId_DisplayOrder",
                schema: "HLG",
                table: "AppHlgBrands",
                columns: new[] { "TenantId", "CategoryId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgContentItems_GameId",
                schema: "HLG",
                table: "AppHlgContentItems",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgContentItems_TenantId_Slot_DisplayOrder",
                schema: "HLG",
                table: "AppHlgContentItems",
                columns: new[] { "TenantId", "Slot", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingPrizes_EventId",
                schema: "HLG",
                table: "AppHlgRankingPrizes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingPrizes_RewardId",
                schema: "HLG",
                table: "AppHlgRankingPrizes",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingPrizes_TenantId_EventId_DisplayOrder",
                schema: "HLG",
                table: "AppHlgRankingPrizes",
                columns: new[] { "TenantId", "EventId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingWinners_EventId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingWinners_PrizeId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                column: "PrizeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHlgRankingWinners_TenantId_EventId_CustomerId",
                schema: "HLG",
                table: "AppHlgRankingWinners",
                columns: new[] { "TenantId", "EventId", "CustomerId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHlgProducts_AppHlgBrands_BrandId",
                schema: "HLG",
                table: "AppHlgProducts",
                column: "BrandId",
                principalSchema: "HLG",
                principalTable: "AppHlgBrands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHlgRankingEvents_AppHlgGames_GameId",
                schema: "HLG",
                table: "AppHlgRankingEvents",
                column: "GameId",
                principalSchema: "HLG",
                principalTable: "AppHlgGames",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHlgProducts_AppHlgBrands_BrandId",
                schema: "HLG",
                table: "AppHlgProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_AppHlgRankingEvents_AppHlgGames_GameId",
                schema: "HLG",
                table: "AppHlgRankingEvents");

            migrationBuilder.DropTable(
                name: "AppHlgBrands",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgContentItems",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgRankingWinners",
                schema: "HLG");

            migrationBuilder.DropTable(
                name: "AppHlgRankingPrizes",
                schema: "HLG");

            migrationBuilder.DropIndex(
                name: "IX_AppHlgRankingEvents_GameId",
                schema: "HLG",
                table: "AppHlgRankingEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppHlgProducts_BrandId",
                schema: "HLG",
                table: "AppHlgProducts");

            migrationBuilder.DropColumn(
                name: "PharmacyCode",
                schema: "HLG",
                table: "AppHlgUserProfiles");

            migrationBuilder.DropColumn(
                name: "GameId",
                schema: "HLG",
                table: "AppHlgRankingEvents");

            migrationBuilder.DropColumn(
                name: "BrandId",
                schema: "HLG",
                table: "AppHlgProducts");

            migrationBuilder.DropColumn(
                name: "DetailsJson",
                schema: "HLG",
                table: "AppHlgProducts");

            migrationBuilder.DropColumn(
                name: "BadgeText",
                schema: "HLG",
                table: "AppHlgGames");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                schema: "HLG",
                table: "AppHlgGames");
        }
    }
}
