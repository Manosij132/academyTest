CREATE TABLE [dbo].[EmployeeMetadata] 
(
	[EmployeeId] INT NOT NULL,
	[MetaKey] VARCHAR(100) NOT NULL,
	[MetaValue] VARCHAR(100) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_EmployeeMetadata] PRIMARY KEY ([EmployeeId], [MetaKey]), 
)
GO