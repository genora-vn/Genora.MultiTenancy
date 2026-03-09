using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppHomePageConfig_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHomePageConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThemeKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHomePageConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHomePageWidgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppHomePageConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WidgetKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Limit = table.Column<int>(type: "int", nullable: true),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHomePageWidgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHomePageWidgets_AppHomePageConfigs_AppHomePageConfigId",
                        column: x => x.AppHomePageConfigId,
                        principalTable: "AppHomePageConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppHomePageWidgetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppHomePageWidgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHomePageWidgetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHomePageWidgetItems_AppHomePageWidgets_AppHomePageWidgetId",
                        column: x => x.AppHomePageWidgetId,
                        principalTable: "AppHomePageWidgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHomePageConfigs_TenantId",
                table: "AppHomePageConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHomePageWidgetItems_AppHomePageWidgetId_DisplayOrder",
                table: "AppHomePageWidgetItems",
                columns: new[] { "AppHomePageWidgetId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHomePageWidgetItems_TenantId",
                table: "AppHomePageWidgetItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHomePageWidgets_AppHomePageConfigId_DisplayOrder",
                table: "AppHomePageWidgets",
                columns: new[] { "AppHomePageConfigId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHomePageWidgets_AppHomePageConfigId_WidgetKey",
                table: "AppHomePageWidgets",
                columns: new[] { "AppHomePageConfigId", "WidgetKey" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHomePageWidgets_TenantId",
                table: "AppHomePageWidgets",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHomePageWidgetItems");

            migrationBuilder.DropTable(
                name: "AppHomePageWidgets");

            migrationBuilder.DropTable(
                name: "AppHomePageConfigs");
        }
    }
}
