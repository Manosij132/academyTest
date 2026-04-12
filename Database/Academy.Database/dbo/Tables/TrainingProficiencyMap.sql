CREATE TABLE [dbo].[TrainingProficiencyMap]
(
	[TrainingProficiencyId] INT IDENTITY(1,1) NOT NULL,
    [EcosystemId] SMALLINT NOT NULL, 
    [SeniorityId] TINYINT NOT NULL, 
    [SkillId] SMALLINT NOT NULL, 
    [TrainingId] INT NOT NULL, 
    [ExpectedProficiency] TINYINT NOT NULL DEFAULT 0, 
    [ExpectedKnowledge] TINYINT NOT NULL DEFAULT 0, 
    [IsMVP] BIT NOT NULL DEFAULT 0, 
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_TrainingProficiencyMap] PRIMARY KEY ([TrainingProficiencyId])
)
GO

CREATE NONCLUSTERED INDEX ix_TrainingProficiencyMap_SkillIdTrainingIdEcosytemIdSeniorityId
ON [dbo].[TrainingProficiencyMap] (SkillId, TrainingId, EcosystemId, SeniorityId)
GO