BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgUserProfiles] ADD [PharmacyCode] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgRankingEvents] ADD [GameId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgProducts] ADD [BrandId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgProducts] ADD [DetailsJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgGames] ADD [BadgeText] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgGames] ADD [BannerUrl] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgBrands] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(250) NOT NULL,
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
        CONSTRAINT [PK_AppHlgBrands] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgBrands_AppHlgKnowledgeCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [HLG].[AppHlgKnowledgeCategories] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgContentItems] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Slot] int NOT NULL,
        [Title] nvarchar(250) NOT NULL,
        [Summary] nvarchar(1000) NULL,
        [BadgeText] nvarchar(100) NULL,
        [ImageUrl] nvarchar(1000) NULL,
        [TargetUrl] nvarchar(1000) NULL,
        [GameId] uniqueidentifier NULL,
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
        CONSTRAINT [PK_AppHlgContentItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgContentItems_AppHlgGames_GameId] FOREIGN KEY ([GameId]) REFERENCES [HLG].[AppHlgGames] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgRankingPrizes] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [EventId] uniqueidentifier NOT NULL,
        [RewardId] uniqueidentifier NOT NULL,
        [Title] nvarchar(250) NOT NULL,
        [Quantity] int NOT NULL,
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
        CONSTRAINT [PK_AppHlgRankingPrizes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgRankingPrizes_AppHlgRankingEvents_EventId] FOREIGN KEY ([EventId]) REFERENCES [HLG].[AppHlgRankingEvents] ([Id]),
        CONSTRAINT [FK_AppHlgRankingPrizes_AppHlgRewards_RewardId] FOREIGN KEY ([RewardId]) REFERENCES [HLG].[AppHlgRewards] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgRankingWinners] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [EventId] uniqueidentifier NOT NULL,
        [PrizeId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [Rank] int NOT NULL,
        [Score] int NOT NULL,
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
        CONSTRAINT [PK_AppHlgRankingWinners] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgRankingWinners_AppHlgRankingEvents_EventId] FOREIGN KEY ([EventId]) REFERENCES [HLG].[AppHlgRankingEvents] ([Id]),
        CONSTRAINT [FK_AppHlgRankingWinners_AppHlgRankingPrizes_PrizeId] FOREIGN KEY ([PrizeId]) REFERENCES [HLG].[AppHlgRankingPrizes] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingEvents_GameId] ON [HLG].[AppHlgRankingEvents] ([GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgProducts_BrandId] ON [HLG].[AppHlgProducts] ([BrandId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgBrands_CategoryId] ON [HLG].[AppHlgBrands] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgBrands_TenantId_CategoryId_DisplayOrder] ON [HLG].[AppHlgBrands] ([TenantId], [CategoryId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgContentItems_GameId] ON [HLG].[AppHlgContentItems] ([GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgContentItems_TenantId_Slot_DisplayOrder] ON [HLG].[AppHlgContentItems] ([TenantId], [Slot], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingPrizes_EventId] ON [HLG].[AppHlgRankingPrizes] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingPrizes_RewardId] ON [HLG].[AppHlgRankingPrizes] ([RewardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingPrizes_TenantId_EventId_DisplayOrder] ON [HLG].[AppHlgRankingPrizes] ([TenantId], [EventId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingWinners_EventId] ON [HLG].[AppHlgRankingWinners] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    CREATE INDEX [IX_AppHlgRankingWinners_PrizeId] ON [HLG].[AppHlgRankingWinners] ([PrizeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AppHlgRankingWinners_TenantId_EventId_CustomerId] ON [HLG].[AppHlgRankingWinners] ([TenantId], [EventId], [CustomerId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgProducts] ADD CONSTRAINT [FK_AppHlgProducts_AppHlgBrands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [HLG].[AppHlgBrands] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgRankingEvents] ADD CONSTRAINT [FK_AppHlgRankingEvents_AppHlgGames_GameId] FOREIGN KEY ([GameId]) REFERENCES [HLG].[AppHlgGames] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919112304_AddHlgDesignContent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919112304_AddHlgDesignContent', N'9.0.5');
END;

COMMIT;
GO

