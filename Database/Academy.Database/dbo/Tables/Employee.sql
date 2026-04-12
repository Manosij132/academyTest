CREATE TABLE [dbo].[Employee]
(
	[Id] INT IDENTITY(10000,1) NOT NULL,
	[EmployeeName] NVARCHAR(255) NULL,
	[GlobantEmailAddress] NVARCHAR(255) NOT NULL UNIQUE,
	[BetterMeLeaderEmail] NVARCHAR(255) NULL,
	[Seniority] NVARCHAR(255) NOT NULL,
	[SeniorityId] SMALLINT NULL,
	[Tdc] VARCHAR(255) NULL,
	[Community] NVARCHAR(255) NULL,
	[Client] NVARCHAR(100) NULL,
	[Project] NVARCHAR(100) NULL,
	[BaseLocation] NVARCHAR(255) NULL,
	[Designation] NVARCHAR(255) NULL,
	[Position] NVARCHAR(255) NULL,
	[JoiningDate] DATETIME2(7) NULL,
	[MobileNo] NVARCHAR(100) NULL,
	[TotalExperience] DECIMAL(5, 2) NULL,
	[Aging] DECIMAL(5, 2) NULL,
	[Gender] NVARCHAR(50) NULL,
	[NoOfDays] SMALLINT NULL,
	[NotificationSendCount] INT NULL,
	[ProjectManagerEmail] NVARCHAR(255) NULL,
	[ProjectTL] NVARCHAR(255) NULL,
	[ProjectTLEmailsCsv] NVARCHAR(100) NULL,
	[ProposedLeaderEmail] NVARCHAR(100) NULL,
	[GlobalId] NVARCHAR(100) NULL,
	[Status] NVARCHAR(100) NULL,
	[Image] NVARCHAR(200) NULL,
	[OnHoldBy] INT NULL,
	[OnHoldForProject] BIT NULL,
	[OtherInfo] NVARCHAR(max) NULL,
	[ProfileLink] NVARCHAR(255) NULL,
	[ResumeLink] NVARCHAR(255) NULL,
	[IsNewJoiner] BIT NULL,
	[Comments] NVARCHAR(1000) NULL,
	[GexLeaders] NVARCHAR(MAX) NULL,
	[MyGrowthReminderCount] SMALLINT NULL,
	[WorkingEcosystem] NVARCHAR(255) NULL,
	[EcosystemId] INT NULL,
	[AiStudio] VARCHAR(100) NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedBy] INT NOT NULL, 
    [CreatedOn] DATETIME2(0) NOT NULL, 
    [UpdatedBy] INT NULL, 
    [UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_Employee] PRIMARY KEY ([Id])
)
GO

CREATE NONCLUSTERED INDEX ix_Employee_SeniorityId
ON [dbo].[Employee] (SeniorityId ASC)
GO

CREATE NONCLUSTERED INDEX ix_Employee_GlobantEmailAddress
ON [dbo].[Employee] (GlobantEmailAddress)
GO
