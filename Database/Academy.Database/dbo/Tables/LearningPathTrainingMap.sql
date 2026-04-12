CREATE TABLE [dbo].[LearningPathTrainingMap]
(
	[LearningPathTrainingMapId] INT IDENTITY(1,1) NOT NULL,
	[SeniorityId] TINYINT NOT NULL, 
	[TrainingId] INT NOT NULL, 
	[LearningPathId] INT NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_LearningPathTrainingMap] PRIMARY KEY ([LearningPathTrainingMapId])
)
GO

CREATE NONCLUSTERED INDEX ix_LearningPathTrainingMap_TrainingIdLearningPathIdSeniorityId
ON [dbo].[LearningPathTrainingMap] (TrainingId, LearningPathId, SeniorityId)
GO

