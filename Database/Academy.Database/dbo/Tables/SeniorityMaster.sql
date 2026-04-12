CREATE TABLE [dbo].[SeniorityMaster]
(
	[SeniorityId] SMALLINT IDENTITY(1,1) NOT NULL, 
	[SeniorityLevel] SMALLINT NOT NULL,
    [SeniorityName] NVARCHAR(50) NOT NULL,
	[Experience] VARCHAR(10) NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL,
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_SeniorityMaster] PRIMARY KEY ([SeniorityId])
)
GO