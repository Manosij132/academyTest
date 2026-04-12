CREATE TABLE [dbo].[LearningPath]
(
	[LearningPathId] INT IDENTITY(1,1) NOT NULL,
	[LearningPathName] VARCHAR(300) NOT NULL,
	[LearningPathDescription] VARCHAR(500) NOT NULL,
	[LearningPathUrl] VARCHAR(500) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_LearningPath] PRIMARY KEY ([LearningPathId])
)