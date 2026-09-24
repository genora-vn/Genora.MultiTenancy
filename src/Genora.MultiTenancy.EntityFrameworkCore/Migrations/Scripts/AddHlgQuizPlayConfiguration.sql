BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924023725_AddHlgQuizPlayConfiguration'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgGames] ADD [AllowedWrongAnswers] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924023725_AddHlgQuizPlayConfiguration'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgGames] ADD [QuestionsPerPlay] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924023725_AddHlgQuizPlayConfiguration'
)
BEGIN
    ALTER TABLE [HLG].[AppHlgGameSessions] ADD [AllowedWrongAnswers] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924023725_AddHlgQuizPlayConfiguration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924023725_AddHlgQuizPlayConfiguration', N'9.0.5');
END;

COMMIT;
GO

