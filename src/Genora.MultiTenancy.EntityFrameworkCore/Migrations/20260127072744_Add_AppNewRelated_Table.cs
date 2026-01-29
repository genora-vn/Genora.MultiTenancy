using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppNewRelated_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNewsRelateds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedNewsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewsId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_AppNewsRelateds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNewsRelateds_AppNews_NewsId",
                        column: x => x.NewsId,
                        principalTable: "AppNews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppNewsRelateds_AppNews_NewsId1",
                        column: x => x.NewsId1,
                        principalTable: "AppNews",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppNewsRelateds_AppNews_RelatedNewsId",
                        column: x => x.RelatedNewsId,
                        principalTable: "AppNews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNews_TenantId_CreationTime",
                table: "AppNews",
                columns: new[] { "TenantId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_NewsId",
                table: "AppNewsRelateds",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_NewsId1",
                table: "AppNewsRelateds",
                column: "NewsId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_RelatedNewsId",
                table: "AppNewsRelateds",
                column: "RelatedNewsId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_TenantId_NewsId_RelatedNewsId",
                table: "AppNewsRelateds",
                columns: new[] { "TenantId", "NewsId", "RelatedNewsId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNewsRelateds");

            migrationBuilder.DropIndex(
                name: "IX_AppNews_TenantId_CreationTime",
                table: "AppNews");
        }
    }
}
