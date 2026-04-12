CREATE TABLE [dbo].[AiStudioClientMap]
(
	[AiStudioClientId] TINYINT NOT NULL IDENTITY(1,1),
	[AiStudioName] VARCHAR(100) NOT NULL,
	[Client] NVARCHAR(100) NOT NULL, 
	[IsActive] BIT NOT NULL DEFAULT 0,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_AiStudioClientMap] PRIMARY KEY ([AiStudioClientId])
)
