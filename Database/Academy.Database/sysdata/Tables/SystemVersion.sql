CREATE TABLE [sysdata].[SystemVersion] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [Major] SMALLINT NOT NULL,
    [Revision] SMALLINT NOT NULL,
    [ScriptName] NVARCHAR (255) NOT NULL,
    [DateApplied] DATETIME2 (0)  NOT NULL, 
    CONSTRAINT [PK_SystemVersion] PRIMARY KEY ([Id])
);