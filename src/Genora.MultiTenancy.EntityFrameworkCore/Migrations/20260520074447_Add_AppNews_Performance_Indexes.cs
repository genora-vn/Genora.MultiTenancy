using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppNews_Performance_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppNewsRelateds_AppNews_NewsId1",
                table: "AppNewsRelateds");

            migrationBuilder.DropIndex(
                name: "IX_AppNewsRelateds_NewsId1",
                table: "AppNewsRelateds");

            migrationBuilder.DropColumn(
                name: "NewsId1",
                table: "AppNewsRelateds");

            migrationBuilder.RenameIndex(
                name: "IX_AppNewsRelateds_TenantId_NewsId_RelatedNewsId",
                table: "AppNewsRelateds",
                newName: "IX_AppNewsRelateds_Tenant_News_Related");

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_Tenant_NewsId",
                table: "AppNewsRelateds",
                columns: new[] { "TenantId", "NewsId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_Tenant_RelatedNewsId",
                table: "AppNewsRelateds",
                columns: new[] { "TenantId", "RelatedNewsId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNews_Status_Display_Published_Creation",
                table: "AppNews",
                columns: new[] { "Status", "DisplayOrder", "PublishedAt", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNews_Tenant_GolfCourse",
                table: "AppNews",
                columns: new[] { "TenantId", "GolfCourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNews_Tenant_Status_Display_Published_Creation",
                table: "AppNews",
                columns: new[] { "TenantId", "Status", "DisplayOrder", "PublishedAt", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNews_Tenant_Title",
                table: "AppNews",
                columns: new[] { "TenantId", "Title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppNewsRelateds_Tenant_NewsId",
                table: "AppNewsRelateds");

            migrationBuilder.DropIndex(
                name: "IX_AppNewsRelateds_Tenant_RelatedNewsId",
                table: "AppNewsRelateds");

            migrationBuilder.DropIndex(
                name: "IX_AppNews_Status_Display_Published_Creation",
                table: "AppNews");

            migrationBuilder.DropIndex(
                name: "IX_AppNews_Tenant_GolfCourse",
                table: "AppNews");

            migrationBuilder.DropIndex(
                name: "IX_AppNews_Tenant_Status_Display_Published_Creation",
                table: "AppNews");

            migrationBuilder.DropIndex(
                name: "IX_AppNews_Tenant_Title",
                table: "AppNews");

            migrationBuilder.RenameIndex(
                name: "IX_AppNewsRelateds_Tenant_News_Related",
                table: "AppNewsRelateds",
                newName: "IX_AppNewsRelateds_TenantId_NewsId_RelatedNewsId");

            migrationBuilder.AddColumn<Guid>(
                name: "NewsId1",
                table: "AppNewsRelateds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRelateds_NewsId1",
                table: "AppNewsRelateds",
                column: "NewsId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppNewsRelateds_AppNews_NewsId1",
                table: "AppNewsRelateds",
                column: "NewsId1",
                principalTable: "AppNews",
                principalColumn: "Id");
        }
    }
}
