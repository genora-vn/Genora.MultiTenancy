using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations;

public partial class AddHl25MiniAppReadIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        CreateIfMissing(migrationBuilder, "AppHl25SpinLogs", "IX_AppHl25SpinLogs_TenantId_ParticipantId_SpinTime",
            "[TenantId], [ParticipantId], [SpinTime]");
        CreateIfMissing(migrationBuilder, "AppHl25Participants", "IX_AppHl25Participants_TenantId_PhoneNumber_IsDeleted",
            "[TenantId], [PhoneNumber], [IsDeleted]");
        CreateIfMissing(migrationBuilder, "AppHl25FrameCreations", "IX_AppHl25FrameCreations_TenantId_ParticipantId_CreatedTime",
            "[TenantId], [ParticipantId], [CreatedTime]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        DropIfPresent(migrationBuilder, "AppHl25SpinLogs", "IX_AppHl25SpinLogs_TenantId_ParticipantId_SpinTime");
        DropIfPresent(migrationBuilder, "AppHl25Participants", "IX_AppHl25Participants_TenantId_PhoneNumber_IsDeleted");
        DropIfPresent(migrationBuilder, "AppHl25FrameCreations", "IX_AppHl25FrameCreations_TenantId_ParticipantId_CreatedTime");
    }

    // Identifiers below are migration constants, never user input. No business data is rewritten.
    private static void CreateIfMissing(MigrationBuilder migrationBuilder, string table, string index, string columns)
        => migrationBuilder.Sql($"""
            IF OBJECT_ID(N'[hl25].[{table}]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[hl25].[{table}]') AND name = N'{index}')
            BEGIN
                CREATE INDEX [{index}] ON [hl25].[{table}] ({columns});
            END;
            """);

    private static void DropIfPresent(MigrationBuilder migrationBuilder, string table, string index)
        => migrationBuilder.Sql($"""
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[hl25].[{table}]') AND name = N'{index}')
                DROP INDEX [{index}] ON [hl25].[{table}];
            """);
}
