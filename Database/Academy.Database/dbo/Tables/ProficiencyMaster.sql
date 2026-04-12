CREATE TABLE [dbo].[ProficiencyMaster]
(
	[ProficiencyId] SMALLINT IDENTITY(1,1) NOT NULL,
	[ProficiencyRating] TINYINT NOT NULL,
	[ProficiencyName] NVARCHAR(50) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_ProficiencyMaster] PRIMARY KEY ([ProficiencyId])
)
