CREATE TABLE [dbo].[KnowledgeMaster]
(
	[KnowledgeId] SMALLINT IDENTITY(1,1) NOT NULL,
	[KnowledgeRating] TINYINT NOT NULL,
	[KnowledgeName] NVARCHAR(50) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_KnowledgeMaster] PRIMARY KEY ([KnowledgeId])
)
