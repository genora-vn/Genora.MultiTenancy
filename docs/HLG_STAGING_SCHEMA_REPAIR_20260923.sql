-- Repair ONLY the staging host database where all HLG baseline objects are absent
-- but the five original HLG migrations are already recorded as applied.
-- Review the dry-run output first. Change @Apply to 1 to execute.
-- After execution, run dotnet ef database update from EntityFrameworkCore.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Apply bit = 0;
DECLARE @ExpectedDatabase sysname = N'GenoraMultiTenancy';

IF DB_NAME() <> @ExpectedDatabase
    THROW 51010, 'Wrong database for HLG host schema repair.', 1;

IF SCHEMA_ID(N'HLG') IS NULL
    THROW 51011, 'HLG schema is absent; investigate migration history before repair.', 1;

IF EXISTS (SELECT 1 FROM sys.objects WHERE schema_id = SCHEMA_ID(N'HLG'))
    THROW 51012, 'HLG schema contains objects; automatic history replay is unsafe.', 1;

IF OBJECT_ID(N'dbo.HlgMigrationHistoryRepair_20260923', N'U') IS NOT NULL
    THROW 51013, 'HLG migration history backup already exists; do not run twice.', 1;

DECLARE @Expected TABLE (MigrationId nvarchar(150) NOT NULL PRIMARY KEY);
INSERT INTO @Expected (MigrationId) VALUES
    (N'20260819094651_AddHlgModule'),
    (N'20260819112301_AddHlgKnowledge'),
    (N'20260820072832_AddHlgGames'),
    (N'20260820084228_AddHlgRewards'),
    (N'20260820093536_AddHlgRanking');

IF EXISTS (SELECT MigrationId FROM @Expected
           EXCEPT SELECT MigrationId FROM dbo.__EFMigrationsHistory)
    OR EXISTS (SELECT MigrationId FROM dbo.__EFMigrationsHistory
               WHERE MigrationId LIKE N'%Hlg%'
               EXCEPT SELECT MigrationId FROM @Expected)
    THROW 51014, 'HLG migration history differs from the expected five rows.', 1;

SELECT DB_NAME() AS DatabaseName,
       (SELECT COUNT(*) FROM sys.objects WHERE schema_id = SCHEMA_ID(N'HLG')) AS HlgObjectCount,
       h.MigrationId, h.ProductVersion
FROM dbo.__EFMigrationsHistory AS h
INNER JOIN @Expected AS e ON e.MigrationId = h.MigrationId
ORDER BY h.MigrationId;

IF @Apply = 0
BEGIN
    PRINT 'DRY RUN ONLY. Set @Apply = 1 after reviewing this result.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.HlgMigrationHistoryRepair_20260923
    (
        MigrationId nvarchar(150) NOT NULL PRIMARY KEY,
        ProductVersion nvarchar(32) NOT NULL,
        BackedUpAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    DELETE h
        OUTPUT deleted.MigrationId, deleted.ProductVersion
        INTO dbo.HlgMigrationHistoryRepair_20260923 (MigrationId, ProductVersion)
    FROM dbo.__EFMigrationsHistory AS h
    INNER JOIN @Expected AS e ON e.MigrationId = h.MigrationId;

    IF @@ROWCOUNT <> 5
        THROW 51015, 'Expected exactly five deleted HLG history rows; rolling back.', 1;

    COMMIT TRANSACTION;
    PRINT 'History safely backed up and cleared. Run dotnet ef database update now.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
