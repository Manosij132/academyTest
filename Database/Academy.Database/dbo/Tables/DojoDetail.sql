CREATE TABLE [dbo].[DojoDetail]
(
	[DojoDetailId] INT IDENTITY(1,1) NOT NULL,
	[EmployeeId] INT NOT NULL,
	[DojoStartDate] DATETIME2(3) NOT NULL,
	[DojoEndDate] DATETIME2(3) NULL,
	[DojoGexLeaderEmail] NVARCHAR(255) NULL,
	[AssignedThroughTraining] BIT NULL,
	[Comments] VARCHAR(1000) NULL,
	[TicketNumber] INT NULL,
	[Account] VARCHAR(100) NULL,
	[AssignmentDate] DATE NULL,
	[DojoProjectsConfigurationId] INT NOT NULL DEFAULT 1,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_DojoDetail] PRIMARY KEY ([DojoDetailId]), 
)
GO

CREATE INDEX [IX_DojoDetail_EmployeeId] 
ON [dbo].[DojoDetail] ([EmployeeId])
INCLUDE (DojoStartDate, DojoEndDate, IsActive, DojoGexLeaderEmail)
GO
