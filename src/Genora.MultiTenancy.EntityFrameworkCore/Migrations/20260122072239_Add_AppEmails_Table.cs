using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppEmails_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToEmails = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CcEmails = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    BccEmails = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ModelJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TryCount = table.Column<int>(type: "int", nullable: false),
                    LastTryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextTryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookingCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_AppEmails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmails_TenantId_BookingCode",
                table: "AppEmails",
                columns: new[] { "TenantId", "BookingCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmails_TenantId_BookingId",
                table: "AppEmails",
                columns: new[] { "TenantId", "BookingId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmails_TenantId_Status_CreationTime",
                table: "AppEmails",
                columns: new[] { "TenantId", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppEmails");
        }
    }
}
