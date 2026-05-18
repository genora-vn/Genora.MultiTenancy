using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppPromotionPolicies_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPromotionPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GolfCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CancellationPolicyHours = table.Column<int>(type: "int", nullable: true),
                    CancellationPolicyContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_AppPromotionPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPromotionPolicies_AppGolfCourses_GolfCourseId",
                        column: x => x.GolfCourseId,
                        principalTable: "AppGolfCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppPromotionPolicies_PromotionTypes_PromotionTypeId",
                        column: x => x.PromotionTypeId,
                        principalTable: "PromotionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPromotionPolicies_GolfCourseId",
                table: "AppPromotionPolicies",
                column: "GolfCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPromotionPolicies_PromotionTypeId",
                table: "AppPromotionPolicies",
                column: "PromotionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPromotionPolicies_Tenant_GolfCourse_Promotion",
                table: "AppPromotionPolicies",
                columns: new[] { "TenantId", "GolfCourseId", "PromotionTypeId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPromotionPolicies");
        }
    }
}
