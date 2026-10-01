BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930171009_AddHlGiftReceipts'
)
BEGIN
    CREATE TABLE [HL].[AppHlGiftReceipts] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [ReceiptCode] nvarchar(50) NOT NULL,
        [CustCode] nvarchar(100) NOT NULL,
        [CustName] nvarchar(250) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [Address] nvarchar(1000) NULL,
        [CampaignCode] nvarchar(100) NOT NULL,
        [CampaignName] nvarchar(250) NULL,
        [CampaignPeriod] int NOT NULL,
        [CampaignStartDate] datetime2 NULL,
        [CampaignEndDate] datetime2 NULL,
        [VoucherCode] nvarchar(100) NOT NULL,
        [VoucherName] nvarchar(500) NOT NULL,
        [VoucherType] int NOT NULL,
        [VoucherValue] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [Status] tinyint NOT NULL,
        [ConfirmedAt] datetime2 NOT NULL,
        [MembershipTier] nvarchar(100) NULL,
        [AccumulatedSales] decimal(18,2) NULL,
        [AccumulatedPoints] int NULL,
        [DsrCode] nvarchar(100) NULL,
        [DsrName] nvarchar(250) NULL,
        [DistributorCode] nvarchar(100) NULL,
        [DistributorName] nvarchar(250) NULL,
        [Source] nvarchar(30) NOT NULL,
        [Note] nvarchar(1000) NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        CONSTRAINT [PK_AppHlGiftReceipts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930171009_AddHlGiftReceipts'
)
BEGIN
    CREATE INDEX [IX_AppHlGiftReceipts_TenantId_ConfirmedAt] ON [HL].[AppHlGiftReceipts] ([TenantId], [ConfirmedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930171009_AddHlGiftReceipts'
)
BEGIN
    CREATE INDEX [IX_AppHlGiftReceipts_TenantId_PhoneNumber_CustCode_ConfirmedAt] ON [HL].[AppHlGiftReceipts] ([TenantId], [PhoneNumber], [CustCode], [ConfirmedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930171009_AddHlGiftReceipts'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HlGiftReceipts_Entitlement] ON [HL].[AppHlGiftReceipts] ([TenantId], [CustCode], [CampaignCode], [CampaignPeriod], [VoucherCode]) WHERE [TenantId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930171009_AddHlGiftReceipts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930171009_AddHlGiftReceipts', N'9.0.5');
END;

COMMIT;
GO

