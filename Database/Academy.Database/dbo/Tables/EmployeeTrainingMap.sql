CREATE TABLE [dbo].[EmployeeTrainingMap]
(
	[EmployeeTrainingId] INT IDENTITY(1,1) NOT NULL, 
    [EmployeeId] INT NOT NULL, 
    [SkillId] SMALLINT NOT NULL, 
    [TrainingId] INT NOT NULL,
    [TrainingStatusId] TINYINT NOT NULL,
    [StartDate] DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(), 
    [ExpectedEndDate] DATETIME2(0) NULL, 
    [ActualEndDate] DATETIME2(0) NULL,
    [Progress] [int] NULL,
    [TrainingTimeSeniorityId] SMALLINT NOT NULL, -- Gives the seniority of emply when training was assigned.
    [TrainingTimeAccount] NVARCHAR(255) NULL, -- Gives the account to which employee was tagged when training was assigned.
    [TraingAssignmentSrc] VARCHAR(50) NOT NULL DEFAULT 'GLOBANT',
    [EmailSent] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
    CONSTRAINT [PK_EmployeeTrainingMap] PRIMARY KEY ([EmployeeTrainingId])
)
GO

CREATE NONCLUSTERED INDEX ix_EmployeeTrainingMap_EmployeeId
ON [dbo].[EmployeeTrainingMap] (EmployeeId ASC)
GO

CREATE NONCLUSTERED INDEX ix_EmployeeTrainingMap_SkillId
ON [dbo].[EmployeeTrainingMap] (SkillId ASC)
GO

CREATE NONCLUSTERED INDEX ix_EmployeeTrainingMap_TrainingId
ON [dbo].[EmployeeTrainingMap] (TrainingId ASC)
GO
