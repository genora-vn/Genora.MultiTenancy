using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <summary>
    /// Repairs databases that applied the original HL25 migration with BirthDate,
    /// before that migration was edited in-place to use AgeGroup.
    /// </summary>
    public partial class EnsureHl25ParticipantAgeGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- DbMigrator visits all tenant databases; only repair databases containing HL25.
IF OBJECT_ID(N'[hl25].[AppHl25Participants]', N'U') IS NOT NULL
BEGIN

IF COL_LENGTH(N'hl25.AppHl25Participants', N'AgeGroup') IS NULL
BEGIN
    ALTER TABLE [hl25].[AppHl25Participants]
        ADD [AgeGroup] tinyint NOT NULL
            CONSTRAINT [DF_AppHl25Participants_AgeGroup] DEFAULT (0) WITH VALUES;
END;

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[hl25].[AppHl25Participants]', N'U')
      AND name = N'AgeGroup'
      AND (system_type_id <> TYPE_ID(N'tinyint') OR is_nullable = 1)
)
BEGIN
    ;THROW 51026, 'HL25 AgeGroup must be tinyint NOT NULL. Review the existing column before applying this repair.', 1;
END;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // AgeGroup is already part of the preceding model and may predate this
            // repair. Keep it on rollback to avoid deleting existing participant data.
        }
    }
}
