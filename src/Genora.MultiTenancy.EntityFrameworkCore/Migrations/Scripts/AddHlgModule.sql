BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819094651_AddHlgModule'
)
BEGIN
    IF SCHEMA_ID(N'HLG') IS NULL EXEC(N'CREATE SCHEMA [HLG];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819094651_AddHlgModule'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgUserProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [ZaloId] nvarchar(100) NULL,
        [CustomerType] tinyint NULL,
        [IsRegistered] bit NOT NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgUserProfiles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819094651_AddHlgModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AppHlgUserProfiles_TenantId_CustomerId] ON [HLG].[AppHlgUserProfiles] ([TenantId], [CustomerId]) WHERE [TenantId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819094651_AddHlgModule'
)
BEGIN
    CREATE INDEX [IX_AppHlgUserProfiles_TenantId_ZaloId] ON [HLG].[AppHlgUserProfiles] ([TenantId], [ZaloId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819094651_AddHlgModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260819094651_AddHlgModule', N'9.0.5');
END;

COMMIT;
GO

