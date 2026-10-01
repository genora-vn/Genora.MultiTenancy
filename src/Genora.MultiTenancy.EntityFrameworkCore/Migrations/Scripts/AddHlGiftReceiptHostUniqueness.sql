BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001025429_AddHlGiftReceiptHostUniqueness'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HlGiftReceipts_HostEntitlement] ON [HL].[AppHlGiftReceipts] ([CustCode], [CampaignCode], [CampaignPeriod], [VoucherCode]) WHERE [TenantId] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001025429_AddHlGiftReceiptHostUniqueness'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001025429_AddHlGiftReceiptHostUniqueness', N'9.0.5');
END;

COMMIT;
GO

