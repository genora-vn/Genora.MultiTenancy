using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddCaddieModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasNightShift",
                table: "AppGolfCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AppCaddies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaddieCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CaddieName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GolfCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HeightCm = table.Column<int>(type: "int", nullable: true),
                    RatingAvg = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    TotalBooking = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    IsShowOnApp = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AppCaddies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SkillCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SkillName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
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
                    table.PrimaryKey("PK_AppCaddieSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLanguages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieVoiceRegions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaddieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoiceRegion = table.Column<byte>(type: "tinyint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCaddieVoiceRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieVoiceRegions_AppCaddies_CaddieId",
                        column: x => x.CaddieId,
                        principalTable: "AppCaddies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaddieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCaddieLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieLanguages_AppCaddies_CaddieId",
                        column: x => x.CaddieId,
                        principalTable: "AppCaddies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCaddieLanguages_AppLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "AppLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieBookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GolfCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaddieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    NumberOfHoles = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    PaymentStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    CheckinStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    CheckinTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppCaddieBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieBookings_AppCaddies_CaddieId",
                        column: x => x.CaddieId,
                        principalTable: "AppCaddies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaddieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallRating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovalStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCaddieRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieRatings_AppCaddieBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "AppCaddieBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCaddieRatings_AppCaddies_CaddieId",
                        column: x => x.CaddieId,
                        principalTable: "AppCaddies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaddieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftCode = table.Column<byte>(type: "tinyint", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SlotStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppCaddieSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieSchedules_AppCaddieBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "AppCaddieBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppCaddieSchedules_AppCaddies_CaddieId",
                        column: x => x.CaddieId,
                        principalTable: "AppCaddies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCaddieRatingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCaddieRatingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCaddieRatingDetails_AppCaddieRatings_RatingId",
                        column: x => x.RatingId,
                        principalTable: "AppCaddieRatings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCaddieRatingDetails_AppCaddieSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "AppCaddieSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_CaddieId",
                table: "AppCaddieBookings",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_ScheduleId",
                table: "AppCaddieBookings",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_TenantId_Caddie_Date",
                table: "AppCaddieBookings",
                columns: new[] { "TenantId", "CaddieId", "BookingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_TenantId_Code",
                table: "AppCaddieBookings",
                columns: new[] { "TenantId", "BookingCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieBookings_TenantId_CustomerId",
                table: "AppCaddieBookings",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieLanguages_CaddieId",
                table: "AppCaddieLanguages",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieLanguages_LanguageId",
                table: "AppCaddieLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieLanguages_TenantId_Caddie_Language",
                table: "AppCaddieLanguages",
                columns: new[] { "TenantId", "CaddieId", "LanguageId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatingDetails_RatingId",
                table: "AppCaddieRatingDetails",
                column: "RatingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatingDetails_SkillId",
                table: "AppCaddieRatingDetails",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatingDetails_TenantId_Rating_Skill",
                table: "AppCaddieRatingDetails",
                columns: new[] { "TenantId", "RatingId", "SkillId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatings_BookingId",
                table: "AppCaddieRatings",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatings_CaddieId",
                table: "AppCaddieRatings",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatings_TenantId_BookingId",
                table: "AppCaddieRatings",
                columns: new[] { "TenantId", "BookingId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieRatings_TenantId_Caddie_Status",
                table: "AppCaddieRatings",
                columns: new[] { "TenantId", "CaddieId", "ApprovalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddies_TenantId_Code",
                table: "AppCaddies",
                columns: new[] { "TenantId", "CaddieCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddies_TenantId_GolfCourseId",
                table: "AppCaddies",
                columns: new[] { "TenantId", "GolfCourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieSchedules_BookingId",
                table: "AppCaddieSchedules",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieSchedules_CaddieId",
                table: "AppCaddieSchedules",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift",
                table: "AppCaddieSchedules",
                columns: new[] { "TenantId", "CaddieId", "WorkDate", "ShiftCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieSkills_TenantId_Code",
                table: "AppCaddieSkills",
                columns: new[] { "TenantId", "SkillCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieVoiceRegions_CaddieId",
                table: "AppCaddieVoiceRegions",
                column: "CaddieId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCaddieVoiceRegions_TenantId_Caddie_Region",
                table: "AppCaddieVoiceRegions",
                columns: new[] { "TenantId", "CaddieId", "VoiceRegion" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppLanguages_TenantId_Code",
                table: "AppLanguages",
                columns: new[] { "TenantId", "LanguageCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCaddieBookings_AppCaddieSchedules_ScheduleId",
                table: "AppCaddieBookings",
                column: "ScheduleId",
                principalTable: "AppCaddieSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCaddieBookings_AppCaddieSchedules_ScheduleId",
                table: "AppCaddieBookings");

            migrationBuilder.DropTable(
                name: "AppCaddieLanguages");

            migrationBuilder.DropTable(
                name: "AppCaddieRatingDetails");

            migrationBuilder.DropTable(
                name: "AppCaddieVoiceRegions");

            migrationBuilder.DropTable(
                name: "AppLanguages");

            migrationBuilder.DropTable(
                name: "AppCaddieRatings");

            migrationBuilder.DropTable(
                name: "AppCaddieSkills");

            migrationBuilder.DropTable(
                name: "AppCaddieSchedules");

            migrationBuilder.DropTable(
                name: "AppCaddieBookings");

            migrationBuilder.DropTable(
                name: "AppCaddies");

            migrationBuilder.DropColumn(
                name: "HasNightShift",
                table: "AppGolfCourses");
        }
    }
}
