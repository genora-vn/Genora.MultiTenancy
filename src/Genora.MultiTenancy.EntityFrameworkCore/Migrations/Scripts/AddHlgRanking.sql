BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820093536_AddHlgRanking'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgRankingEvents] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Title] nvarchar(250) NOT NULL,
        [Description] nvarchar(max) NULL,
        [StartAt] datetime2 NOT NULL,
        [EndAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgRankingEvents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820093536_AddHlgRanking'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingEvents_TenantId_IsActive_StartAt] ON [HLG].[AppHlgRankingEvents] ([TenantId], [IsActive], [StartAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820093536_AddHlgRanking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820093536_AddHlgRanking', N'9.0.5');
END;

COMMIT;
GO

