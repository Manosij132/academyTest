CREATE TABLE [dbo].[ReportType]
(
	[ReportId] INT NOT NULL,
	[ReportName] VARCHAR(100) NOT NULL,
	[StoredProcName] VARCHAR(200) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
	CONSTRAINT [PK_ReportType] PRIMARY KEY ([ReportId])
)