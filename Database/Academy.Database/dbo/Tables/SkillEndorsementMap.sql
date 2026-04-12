CREATE TABLE [dbo].[SkillEndorsementMap]
--Same table will be used for history. Previous rating isActive will be set to 0.
(
	[SkillEndorsementId] INT IDENTITY(1,1) NOT NULL,
    [EmployeeId] INT NOT NULL, 
    [SkillId] SMALLINT NOT NULL, 
    [CurrentProficiency] TINYINT NOT NULL, 
    [CurrentKnowledge] TINYINT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL,
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
    CONSTRAINT [PK_SkillEndorsementMap] PRIMARY KEY ([SkillEndorsementId])
)
GO

CREATE NONCLUSTERED INDEX ix_SkillEndorsementMap_EmployeeId
ON [dbo].[SkillEndorsementMap] (EmployeeId ASC)
GO

CREATE NONCLUSTERED INDEX ix_SkillEndorsementMap_SkillId
ON [dbo].[SkillEndorsementMap] (SkillId ASC)
GO