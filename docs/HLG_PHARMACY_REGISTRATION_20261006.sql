BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006054856_AddHlgPharmacyRegistration'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgUserProfiles] ADD [DmsCustomerCode] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006054856_AddHlgPharmacyRegistration'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgUserProfiles] ADD [PharmaPhone] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006054856_AddHlgPharmacyRegistration'
)
BEGIN
    CREATE INDEX [IX_AppHlgUserProfiles_TenantId_PharmaPhone] ON [HLG].[AppHlgUserProfiles] ([TenantId], [PharmaPhone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006054856_AddHlgPharmacyRegistration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006054856_AddHlgPharmacyRegistration', N'9.0.5');
END;

COMMIT;
GO

