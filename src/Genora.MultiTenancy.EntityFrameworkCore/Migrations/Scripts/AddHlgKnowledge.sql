BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgKnowledgeCategories] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Name] nvarchar(250) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ImageUrl] nvarchar(1000) NULL,
        [DisplayOrder] int NOT NULL,
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
        CONSTRAINT [PK_AppHlgKnowledgeCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgLearningProgress] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [ProgressPercent] int NOT NULL,
        [IsCompleted] bit NOT NULL,
        [CompletedAt] datetime2 NULL,
        [LastViewedAt] datetime2 NOT NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgLearningProgress] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgProducts] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(250) NOT NULL,
        [ThumbnailUrl] nvarchar(1000) NULL,
        [Summary] nvarchar(1000) NULL,
        [Content] nvarchar(max) NULL,
        [ImagesJson] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
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
        CONSTRAINT [PK_AppHlgProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgProducts_AppHlgKnowledgeCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [HLG].[AppHlgKnowledgeCategories] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    CREATE INDEX [IX_AppHlgKnowledgeCategories_TenantId_DisplayOrder] ON [HLG].[AppHlgKnowledgeCategories] ([TenantId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AppHlgLearningProgress_TenantId_CustomerId_ProductId] ON [HLG].[AppHlgLearningProgress] ([TenantId], [CustomerId], [ProductId]) WHERE [TenantId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    CREATE INDEX [IX_AppHlgProducts_CategoryId] ON [HLG].[AppHlgProducts] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    CREATE INDEX [IX_AppHlgProducts_TenantId_CategoryId] ON [HLG].[AppHlgProducts] ([TenantId], [CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819112301_AddHlgKnowledge'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260819112301_AddHlgKnowledge', N'9.0.5');
END;

COMMIT;
GO

