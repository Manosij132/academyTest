CREATE TABLE [dbo].[TrainingStatusMaster]
(
	[TrainingStatusId] TINYINT NOT NULL, 
    [TrainingStatusName] NVARCHAR(50) NULL, 
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_TrainingStatusMaster] PRIMARY KEY ([TrainingStatusId])
)
