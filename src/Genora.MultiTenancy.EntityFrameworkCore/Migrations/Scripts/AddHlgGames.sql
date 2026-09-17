BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgGames] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Name] nvarchar(250) NOT NULL,
        [Type] tinyint NOT NULL,
        [ImageUrl] nvarchar(1000) NULL,
        [Description] nvarchar(max) NULL,
        [Rules] nvarchar(max) NULL,
        [RewardDescription] nvarchar(max) NULL,
        [Status] tinyint NOT NULL,
        [StartAt] datetime2 NULL,
        [EndAt] datetime2 NULL,
        [BaseScorePerQuestion] int NOT NULL,
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
        CONSTRAINT [PK_AppHlgGames] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgGameSessions] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [GameId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [CurrentIndex] int NOT NULL,
        [Score] int NOT NULL,
        [CorrectCount] int NOT NULL,
        [TotalQuestions] int NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [IsFinished] bit NOT NULL,
        [FinishedAt] datetime2 NULL,
        [ShippingAddressId] uniqueidentifier NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgGameSessions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgQuestions] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [GameId] uniqueidentifier NOT NULL,
        [Index] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(1000) NULL,
        [TimeLimitSec] int NOT NULL,
        [ScoreMultiplier] decimal(9,2) NOT NULL,
        [CorrectKey] tinyint NOT NULL,
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
        CONSTRAINT [PK_AppHlgQuestions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgQuestions_AppHlgGames_GameId] FOREIGN KEY ([GameId]) REFERENCES [HLG].[AppHlgGames] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgSessionAnswers] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [SessionId] uniqueidentifier NOT NULL,
        [QuestionId] uniqueidentifier NOT NULL,
        [SelectedKey] tinyint NOT NULL,
        [IsCorrect] bit NOT NULL,
        [ScoreGained] int NOT NULL,
        [TimeSpentSec] int NOT NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgSessionAnswers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgSessionAnswers_AppHlgGameSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [HLG].[AppHlgGameSessions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE TABLE [HLG].[AppHlgAnswerOptions] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [QuestionId] uniqueidentifier NOT NULL,
        [Key] tinyint NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [ExtraProperties] nvarchar(max) NOT NULL,
        [ConcurrencyStamp] nvarchar(40) NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [CreatorId] uniqueidentifier NULL,
        [LastModificationTime] datetime2 NULL,
        [LastModifierId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeleterId] uniqueidentifier NULL,
        [DeletionTime] datetime2 NULL,
        CONSTRAINT [PK_AppHlgAnswerOptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppHlgAnswerOptions_AppHlgQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [HLG].[AppHlgQuestions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgAnswerOptions_QuestionId] ON [HLG].[AppHlgAnswerOptions] ([QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgAnswerOptions_TenantId_QuestionId] ON [HLG].[AppHlgAnswerOptions] ([TenantId], [QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgGames_TenantId_Status_DisplayOrder] ON [HLG].[AppHlgGames] ([TenantId], [Status], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgGameSessions_TenantId_CustomerId_GameId] ON [HLG].[AppHlgGameSessions] ([TenantId], [CustomerId], [GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgQuestions_GameId] ON [HLG].[AppHlgQuestions] ([GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgQuestions_TenantId_GameId_Index] ON [HLG].[AppHlgQuestions] ([TenantId], [GameId], [Index]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgSessionAnswers_SessionId] ON [HLG].[AppHlgSessionAnswers] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    CREATE INDEX [IX_AppHlgSessionAnswers_TenantId_SessionId] ON [HLG].[AppHlgSessionAnswers] ([TenantId], [SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820072832_AddHlgGames'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820072832_AddHlgGames', N'9.0.5');
END;

COMMIT;
GO

