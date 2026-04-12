CREATE TABLE [dbo].[ReportColumnConfiguration]
(
	[ReportColumnConfigId] INT NOT NULL,
	[ReportColumnName] VARCHAR(50) NOT NULL,
	[ReportColumnDisplayName] VARCHAR(50) NOT NULL,
	[IsGroupBy] BIT NOT NULL DEFAULT 1,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_ReportColumnConfiguration] PRIMARY KEY ([ReportColumnConfigId])
)