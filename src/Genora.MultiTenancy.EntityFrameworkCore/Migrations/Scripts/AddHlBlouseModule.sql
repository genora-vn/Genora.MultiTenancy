-- ============================================================================
-- AddHlBlouseModule — Idempotent script cho tính năng "Đăng ký nhận áo Blouse"
-- Schema: HL. Bảng: AppHlBlouseCampaigns, AppHlBlouseSizes,
--         AppHlBlouseRegistrations, AppHlBlouseRegistrationItems
--
-- An toàn chạy nhiều lần. Bỏ qua nếu bảng/cột/index đã tồn tại.
-- Chạy trên đúng DB tenant Hoa Linh Sales (VD: HoaLinhMienNam).
-- ============================================================================

SET NOCOUNT ON;
GO

-- Bảo đảm schema HL tồn tại
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'HL')
    EXEC(N'CREATE SCHEMA [HL]');
GO

-- ============================ AppHlBlouseCampaigns ==========================
IF OBJECT_ID(N'[HL].[AppHlBlouseCampaigns]', N'U') IS NULL
BEGIN
    CREATE TABLE [HL].[AppHlBlouseCampaigns] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [ProgramName] nvarchar(250) NOT NULL,
        [IntroductionHtml] nvarchar(max) NULL,
        [FreeShirtLimit] int NOT NULL,
        [PointsPerShirt] int NOT NULL,
        [MaxExchangeShirt] int NOT NULL,
        [SizeChartImageUrl] nvarchar(500) NULL,
        [StartTime] datetime2 NULL,
        [EndTime] datetime2 NULL,
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
        CONSTRAINT [PK_AppHlBlouseCampaigns] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseCampaigns]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseCampaigns]') AND name = N'IX_AppHlBlouseCampaigns_TenantId_IsActive')
    CREATE INDEX [IX_AppHlBlouseCampaigns_TenantId_IsActive] ON [HL].[AppHlBlouseCampaigns] ([TenantId], [IsActive]);
GO

-- ============================ AppHlBlouseSizes ==============================
IF OBJECT_ID(N'[HL].[AppHlBlouseSizes]', N'U') IS NULL
BEGIN
    CREATE TABLE [HL].[AppHlBlouseSizes] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Style] tinyint NOT NULL,
        [SizeCode] nvarchar(20) NOT NULL,
        [WeightRange] nvarchar(100) NULL,
        [StockQuantity] int NOT NULL,
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
        CONSTRAINT [PK_AppHlBlouseSizes] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseSizes]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseSizes]') AND name = N'IX_AppHlBlouseSizes_TenantId_Style_SizeCode')
    CREATE UNIQUE INDEX [IX_AppHlBlouseSizes_TenantId_Style_SizeCode] ON [HL].[AppHlBlouseSizes] ([TenantId], [Style], [SizeCode]) WHERE [TenantId] IS NOT NULL;
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseSizes]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseSizes]') AND name = N'IX_AppHlBlouseSizes_TenantId_IsActive_Style')
    CREATE INDEX [IX_AppHlBlouseSizes_TenantId_IsActive_Style] ON [HL].[AppHlBlouseSizes] ([TenantId], [IsActive], [Style]);
GO

-- ========================= AppHlBlouseRegistrations =========================
IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]', N'U') IS NULL
BEGIN
    CREATE TABLE [HL].[AppHlBlouseRegistrations] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [RegistrationCode] nvarchar(50) NOT NULL,
        [CustomerCode] nvarchar(50) NULL,
        [CustomerName] nvarchar(250) NULL,
        [CustomerPhone] nvarchar(20) NULL,
        [ZaloUserId] nvarchar(100) NULL,
        [ReceiverName] nvarchar(150) NULL,
        [DeliveryAddress] nvarchar(500) NULL,
        [BusinessType] tinyint NULL,
        [BusinessTypeName] nvarchar(250) NULL,
        [StoreName] nvarchar(250) NULL,
        [PrintedName] nvarchar(250) NULL,
        [Note] nvarchar(max) NULL,
        [FreeQuantity] int NOT NULL,
        [ExchangeQuantity] int NOT NULL,
        [TotalQuantity] int NOT NULL,
        [TotalPointsUsed] int NOT NULL,
        [Status] tinyint NOT NULL,
        [InternalNote] nvarchar(max) NULL,
        [ProcessedBy] uniqueidentifier NULL,
        [ProcessedAt] datetime2 NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlBlouseRegistrations] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]') AND name = N'IX_AppHlBlouseRegistrations_TenantId_Code')
    CREATE UNIQUE INDEX [IX_AppHlBlouseRegistrations_TenantId_Code] ON [HL].[AppHlBlouseRegistrations] ([TenantId], [RegistrationCode]) WHERE [TenantId] IS NOT NULL;
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]') AND name = N'IX_AppHlBlouseRegistrations_TenantId_CreationTime')
    CREATE INDEX [IX_AppHlBlouseRegistrations_TenantId_CreationTime] ON [HL].[AppHlBlouseRegistrations] ([TenantId], [CreationTime]);
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]') AND name = N'IX_AppHlBlouseRegistrations_TenantId_CustomerCode')
    CREATE INDEX [IX_AppHlBlouseRegistrations_TenantId_CustomerCode] ON [HL].[AppHlBlouseRegistrations] ([TenantId], [CustomerCode]);
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseRegistrations]') AND name = N'IX_AppHlBlouseRegistrations_TenantId_Status')
    CREATE INDEX [IX_AppHlBlouseRegistrations_TenantId_Status] ON [HL].[AppHlBlouseRegistrations] ([TenantId], [Status]);
GO

-- ======================= AppHlBlouseRegistrationItems =======================
IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrationItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [HL].[AppHlBlouseRegistrationItems] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [RegistrationId] uniqueidentifier NOT NULL,
        [ItemType] tinyint NOT NULL,
        [Style] tinyint NOT NULL,
        [SizeId] uniqueidentifier NULL,
        [SizeCode] nvarchar(20) NOT NULL,
        [WeightRange] nvarchar(100) NULL,
        [Quantity] int NOT NULL,
        [PointsPerItem] int NOT NULL,
        [TotalPoints] int NOT NULL,
        CONSTRAINT [PK_AppHlBlouseRegistrationItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlBlouseRegistrationItems_AppHlBlouseRegistrations_RegistrationId]
            FOREIGN KEY ([RegistrationId]) REFERENCES [HL].[AppHlBlouseRegistrations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrationItems]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseRegistrationItems]') AND name = N'IX_AppHlBlouseRegistrationItems_RegistrationId')
    CREATE INDEX [IX_AppHlBlouseRegistrationItems_RegistrationId] ON [HL].[AppHlBlouseRegistrationItems] ([RegistrationId]);
GO

IF OBJECT_ID(N'[HL].[AppHlBlouseRegistrationItems]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[HL].[AppHlBlouseRegistrationItems]') AND name = N'IX_AppHlBlouseRegistrationItems_TenantId_RegistrationId')
    CREATE INDEX [IX_AppHlBlouseRegistrationItems_TenantId_RegistrationId] ON [HL].[AppHlBlouseRegistrationItems] ([TenantId], [RegistrationId]);
GO

-- ============================================================================
-- Ghi nhận migration vào history (để EF không cố áp lại). Đổi ProductVersion
-- cho khớp bản EF Core đang dùng nếu cần.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260929091139_AddHlBlouseModule')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929091139_AddHlBlouseModule', N'9.0.0');
GO
