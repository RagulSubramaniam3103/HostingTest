BEGIN TRANSACTION;
GO

ALTER TABLE [EWO_ChatMessage] ADD [DeletedBy] nvarchar(max) NULL;
GO

ALTER TABLE [EWO_ChatMessage] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [EWO_ChatGroup] ADD [ProfileImage] nvarchar(max) NULL;
GO

CREATE TABLE [EWO_UserPersonalFiles] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [FileName] nvarchar(max) NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [FileType] nvarchar(max) NOT NULL,
    [FileSize] bigint NOT NULL,
    [UploadDate] datetime2 NOT NULL,
    CONSTRAINT [PK_EWO_UserPersonalFiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EWO_UserPersonalFiles_EWO_MasterUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [EWO_MasterUser] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_EWO_UserPersonalFiles_UserId] ON [EWO_UserPersonalFiles] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260511163628_AddUserPersonalFiles', N'8.0.4');
GO

COMMIT;
GO

