CREATE TABLE [dbo].[DojoGxLeaderAssignment]
(
	[DojoGxLeaderAssignmentId] INT IDENTITY(1,1) NOT NULL,
	[DojoDetailId] INT NOT NULL,
	[AssignmentStartDate] DATETIME2(3) NOT NULL,
	[AssignmentEndDate] DATETIME2(3) NULL,
	[LeaderEmail] NVARCHAR(255) NULL,
	[Comments] VARCHAR(300) NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_DojoGxLeaderAssignment] PRIMARY KEY ([DojoGxLeaderAssignmentId]), 
)
GO

CREATE INDEX [IX_DojoGxLeaderAssignment_DojoDetailId] 
ON [dbo].[DojoGxLeaderAssignment] ([DojoDetailId])
INCLUDE (AssignmentStartDate, AssignmentEndDate, IsActive, LeaderEmail)
GO