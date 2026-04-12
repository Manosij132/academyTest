CREATE TABLE [dbo].[TrainingMaster]
(
	[TrainingId] INT NOT NULL, 
    [TrainingName] NVARCHAR(150) NOT NULL, 
    [TrainingDescription] NVARCHAR(500) NULL, 
    [TrainingUrl] NVARCHAR(2000) NOT NULL, 
	[TrainingCompletionHours] SMALLINT NOT NULL,
	[IsAssignment] BIT NOT NULL DEFAULT 0,
	[IsPriortize] bit NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_TrainingMaster] PRIMARY KEY ([TrainingId])
)
