BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920100056_AddHl25MiniAppReadIndexes'
)
BEGIN
    IF OBJECT_ID(N'[hl25].[AppHl25SpinLogs]', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[hl25].[AppHl25SpinLogs]') AND name = N'IX_AppHl25SpinLogs_TenantId_ParticipantId_SpinTime')
    BEGIN
        CREATE INDEX [IX_AppHl25SpinLogs_TenantId_ParticipantId_SpinTime] ON [hl25].[AppHl25SpinLogs] ([TenantId], [ParticipantId], [SpinTime]);
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920100056_AddHl25MiniAppReadIndexes'
)
BEGIN
    IF OBJECT_ID(N'[hl25].[AppHl25Participants]', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[hl25].[AppHl25Participants]') AND name = N'IX_AppHl25Participants_TenantId_PhoneNumber_IsDeleted')
    BEGIN
        CREATE INDEX [IX_AppHl25Participants_TenantId_PhoneNumber_IsDeleted] ON [hl25].[AppHl25Participants] ([TenantId], [PhoneNumber], [IsDeleted]);
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920100056_AddHl25MiniAppReadIndexes'
)
BEGIN
    IF OBJECT_ID(N'[hl25].[AppHl25FrameCreations]', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[hl25].[AppHl25FrameCreations]') AND name = N'IX_AppHl25FrameCreations_TenantId_ParticipantId_CreatedTime')
    BEGIN
        CREATE INDEX [IX_AppHl25FrameCreations_TenantId_ParticipantId_CreatedTime] ON [hl25].[AppHl25FrameCreations] ([TenantId], [ParticipantId], [CreatedTime]);
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920100056_AddHl25MiniAppReadIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920100056_AddHl25MiniAppReadIndexes', N'9.0.5');
END;

COMMIT;
GO

