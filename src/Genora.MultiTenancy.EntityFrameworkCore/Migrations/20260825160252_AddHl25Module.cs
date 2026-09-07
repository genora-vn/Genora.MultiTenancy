using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddHl25Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hl25");

            migrationBuilder.CreateTable(
                name: "AppHl25AppConfig",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProgramName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    BannerUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TvcUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TvcHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RulesHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GamePlayHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrganizerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_AppHl25AppConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25FrameCampaigns",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_AppHl25FrameCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25Gifts",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_AppHl25Gifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25Participants",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ZaloUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    AgeGroup = table.Column<byte>(type: "tinyint", nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    ReceiveAddress = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    JoinedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFollowingOa = table.Column<bool>(type: "bit", nullable: false),
                    HasConsent = table.Column<bool>(type: "bit", nullable: false),
                    ConsentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemainingSpinTurns = table.Column<int>(type: "int", nullable: false),
                    TotalSpinTurns = table.Column<int>(type: "int", nullable: false),
                    EarnedCycles = table.Column<int>(type: "int", nullable: false),
                    TotalGiftsWon = table.Column<int>(type: "int", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_AppHl25Participants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25WheelConfig",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SubTitle = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    SecondaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    PointerImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SlotCount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppHl25WheelConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25FrameTemplates",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_AppHl25FrameTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHl25FrameTemplates_AppHl25FrameCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalSchema: "hl25",
                        principalTable: "AppHl25FrameCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25FrameCreations",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResultImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    WishMessage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ShareLink = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SharePlatform = table.Column<byte>(type: "tinyint", nullable: false),
                    ShareTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_AppHl25FrameCreations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHl25FrameCreations_AppHl25Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalSchema: "hl25",
                        principalTable: "AppHl25Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25SpinLogs",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WheelSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GiftNameSnapshot = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SpinTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RewardStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    DeliveredTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiverAddressSnapshot = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_AppHl25SpinLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHl25SpinLogs_AppHl25Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalSchema: "hl25",
                        principalTable: "AppHl25Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25SpinTurnLogs",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    TurnsAdded = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    GrantedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrameCreationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_AppHl25SpinTurnLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHl25SpinTurnLogs_AppHl25Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalSchema: "hl25",
                        principalTable: "AppHl25Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppHl25WheelSlots",
                schema: "hl25",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WheelConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SlotImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    WinRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
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
                    table.PrimaryKey("PK_AppHl25WheelSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHl25WheelSlots_AppHl25WheelConfig_WheelConfigId",
                        column: x => x.WheelConfigId,
                        principalSchema: "hl25",
                        principalTable: "AppHl25WheelConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25AppConfig_TenantId",
                schema: "hl25",
                table: "AppHl25AppConfig",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameCampaigns_TenantId_Status",
                schema: "hl25",
                table: "AppHl25FrameCampaigns",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameCreations_ParticipantId",
                schema: "hl25",
                table: "AppHl25FrameCreations",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameCreations_TenantId_CampaignId",
                schema: "hl25",
                table: "AppHl25FrameCreations",
                columns: new[] { "TenantId", "CampaignId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameCreations_TenantId_CreatedTime",
                schema: "hl25",
                table: "AppHl25FrameCreations",
                columns: new[] { "TenantId", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameCreations_TenantId_ParticipantId",
                schema: "hl25",
                table: "AppHl25FrameCreations",
                columns: new[] { "TenantId", "ParticipantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameTemplates_CampaignId",
                schema: "hl25",
                table: "AppHl25FrameTemplates",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25FrameTemplates_TenantId_CampaignId",
                schema: "hl25",
                table: "AppHl25FrameTemplates",
                columns: new[] { "TenantId", "CampaignId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25Gifts_TenantId_Status",
                schema: "hl25",
                table: "AppHl25Gifts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25Participants_TenantId_JoinedTime",
                schema: "hl25",
                table: "AppHl25Participants",
                columns: new[] { "TenantId", "JoinedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25Participants_TenantId_PhoneNumber",
                schema: "hl25",
                table: "AppHl25Participants",
                columns: new[] { "TenantId", "PhoneNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25Participants_TenantId_ZaloUserId",
                schema: "hl25",
                table: "AppHl25Participants",
                columns: new[] { "TenantId", "ZaloUserId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [ZaloUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinLogs_ParticipantId",
                schema: "hl25",
                table: "AppHl25SpinLogs",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinLogs_TenantId_GiftId",
                schema: "hl25",
                table: "AppHl25SpinLogs",
                columns: new[] { "TenantId", "GiftId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinLogs_TenantId_ParticipantId",
                schema: "hl25",
                table: "AppHl25SpinLogs",
                columns: new[] { "TenantId", "ParticipantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinLogs_TenantId_RewardStatus",
                schema: "hl25",
                table: "AppHl25SpinLogs",
                columns: new[] { "TenantId", "RewardStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinLogs_TenantId_SpinTime",
                schema: "hl25",
                table: "AppHl25SpinLogs",
                columns: new[] { "TenantId", "SpinTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinTurnLogs_ParticipantId",
                schema: "hl25",
                table: "AppHl25SpinTurnLogs",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinTurnLogs_TenantId_GrantedTime",
                schema: "hl25",
                table: "AppHl25SpinTurnLogs",
                columns: new[] { "TenantId", "GrantedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25SpinTurnLogs_TenantId_ParticipantId",
                schema: "hl25",
                table: "AppHl25SpinTurnLogs",
                columns: new[] { "TenantId", "ParticipantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25WheelConfig_TenantId",
                schema: "hl25",
                table: "AppHl25WheelConfig",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25WheelSlots_TenantId_WheelConfigId",
                schema: "hl25",
                table: "AppHl25WheelSlots",
                columns: new[] { "TenantId", "WheelConfigId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHl25WheelSlots_WheelConfigId",
                schema: "hl25",
                table: "AppHl25WheelSlots",
                column: "WheelConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHl25AppConfig",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25FrameCreations",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25FrameTemplates",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25Gifts",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25SpinLogs",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25SpinTurnLogs",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25WheelSlots",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25FrameCampaigns",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25Participants",
                schema: "hl25");

            migrationBuilder.DropTable(
                name: "AppHl25WheelConfig",
                schema: "hl25");
        }
    }
}
