CREATE TABLE [dbo].[ScheduledJob]
(
	[ScheduledJobId] SMALLINT NOT NULL IDENTITY(1,1),
	[JobName] VARCHAR(50) NOT NULL,
	[JobDescription] VARCHAR(255) NULL,
	[JobSchedule] VARCHAR(100) NULL,
	[JobState] VARCHAR(10) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL,
	[CreatedOn] DATETIME2(0) NOT NULL,
	[UpdatedBy] INT NULL,
	[UpdatedOn] DATETIME2(0) NULL,
	CONSTRAINT [PK_ScheduledJob] PRIMARY KEY ([ScheduledJobId])
)
GO

CREATE INDEX IX_ScheduledJob_JobName
ON [dbo].[ScheduledJob] ([JobName])
INCLUDE ([JobDescription], [JobSchedule], [JobState], [IsActive])
GO