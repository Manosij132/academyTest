CREATE TABLE [dbo].[EmployeeActivityMap]
(
	[EmployeeActivityId] INT NOT NULL IDENTITY(1,1),
	[EmployeeId] INT NOT NULL,
	[ActivityId] SMALLINT NOT NULL,
	[ActivitySource] VARCHAR(255) NULL,
	[ActivityDetail] VARCHAR(255) NULL,
	[Comments] VARCHAR(2048) NULL,
	[StartDate] DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(),
	[EndDate] DATETIME2(0) NULL,
	[StatusId] TINYINT NOT NULL DEFAULT 1,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    [Account] VARCHAR(255) NULL, 
    CONSTRAINT [PK_EmployeeActivityMap] PRIMARY KEY ([EmployeeActivityId]),
)
