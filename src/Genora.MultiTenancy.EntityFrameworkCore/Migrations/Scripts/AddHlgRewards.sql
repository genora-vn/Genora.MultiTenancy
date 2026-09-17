BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgRewardHistories] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [RewardId] uniqueidentifier NOT NULL,
        [RewardName] nvarchar(250) NOT NULL,
        [PointDelta] int NOT NULL,
        [RewardType] tinyint NOT NULL,
        [Status] tinyint NOT NULL,
        [ShippingAddressId] uniqueidentifier NULL,
        [VoucherCode] nvarchar(100) NULL,
        [SessionId] uniqueidentifier NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgRewardHistories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgRewards] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Name] nvarchar(250) NOT NULL,
        [ImageUrl] nvarchar(1000) NULL,
        [PointCost] int NOT NULL,
        [Type] tinyint NOT NULL,
        [StockQuantity] int NULL,
        [VoucherCode] nvarchar(100) NULL,
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
        CONSTRAINT [PK_AppHlgRewards] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgShippingAddresses] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [ReceiverName] nvarchar(150) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [Note] nvarchar(500) NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgShippingAddresses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    CREATE INDEX [IX_AppHlgRewardHistories_TenantId_CustomerId] ON [HLG].[AppHlgRewardHistories] ([TenantId], [CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    CREATE INDEX [IX_AppHlgRewards_TenantId_IsActive_DisplayOrder] ON [HLG].[AppHlgRewards] ([TenantId], [IsActive], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    CREATE INDEX [IX_AppHlgShippingAddresses_TenantId_CustomerId] ON [HLG].[AppHlgShippingAddresses] ([TenantId], [CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820084228_AddHlgRewards'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820084228_AddHlgRewards', N'9.0.5');
END;

COMMIT;
GO

