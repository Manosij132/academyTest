CREATE TABLE [dbo].[SkillMaster]
(
	[SkillId] SMALLINT NOT NULL, 
    [SkillName] NVARCHAR(255) NOT NULL,
	[DisplayName] NVARCHAR(255) NULL,
    [SkillDescription] NVARCHAR(MAX) NULL,
	[CategoryId] SMALLINT NULL,
	[Mandatory] BIT NULL,
	[Grouping] VARCHAR(255) NULL,
	[IsDefaultInGroup] BIT NOT NULL DEFAULT 0,
	[Specification] VARCHAR(255) NULL,
	[IsSkillRequiredInReport] BIT NOT NULL DEFAULT 1,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_SkillMaster] PRIMARY KEY ([SkillId])
)
