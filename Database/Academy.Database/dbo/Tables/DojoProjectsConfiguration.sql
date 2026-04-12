CREATE TABLE [dbo].[DojoProjectsConfiguration]
(
	[DojoProjectsConfigurationId] INT NOT NULL IDENTITY(1,1), 
    [ProjectName] VARCHAR(50) NOT NULL, 
    [IsAssignable] BIT NOT NULL, 
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_DojoProjectsConfiguration] PRIMARY KEY ([DojoProjectsConfigurationId])
)
