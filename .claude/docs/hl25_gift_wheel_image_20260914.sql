BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914111213_AddHl25GiftWheelImage'
)
BEGIN
    ALTER TABLE [hl25].[AppHl25Gifts] ADD [WheelImageUrl] nvarchar(1024) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914111213_AddHl25GiftWheelImage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914111213_AddHl25GiftWheelImage', N'9.0.5');
END;

COMMIT;
GO

