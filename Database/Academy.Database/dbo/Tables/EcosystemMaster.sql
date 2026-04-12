CREATE TABLE [dbo].[EcosystemMaster]
(
	[EcosystemId] INT IDENTITY(1,1) NOT NULL, 
    [EcosystemName] NVARCHAR(255) NOT NULL, 
    [IsPrimary] BIT NOT NULL DEFAULT 0, 
    [ParentEcosystemId] INT NULL,
	[DisplayName] VARCHAR(255) NULL,
    [IsActive] BIT NOT NULL DEFAULT 0,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_EcosystemMaster] PRIMARY KEY ([EcosystemId]),
)
GO

CREATE NONCLUSTERED INDEX ixEcosystemMaster_EcosystemName
ON [dbo].[EcosystemMaster] (EcosystemName ASC)
GO