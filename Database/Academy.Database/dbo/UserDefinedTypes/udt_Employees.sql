CREATE TYPE [dbo].[udt_Employees] AS TABLE
(
	[GlobalId] VARCHAR(20) NULL,
	[EmployeeName] NVARCHAR(255) NULL,
	[JoiningDate] DATETIME2(3) NULL,
	[Status] NVARCHAR(100) NULL,
	[BaseLocation] NVARCHAR(255) NOT NULL,
	[GlobantEmailId] NVARCHAR(255) NOT NULL,
	[Community] NVARCHAR(255) NULL,
	[Client] NVARCHAR(100) NULL,
	[Project] NVARCHAR(100) NULL,
	[Position] NVARCHAR(255) NULL,
	[Seniority] NVARCHAR(255) NULL,
	[Gender] NVARCHAR(50) NULL,
	[GlobantTenure] DECIMAL(5, 2) NULL,
	[TotalExperience] DECIMAL(5, 2) NULL,
	[CareerLeader] NVARCHAR(255) NULL,
	[GexLeaders] NVARCHAR(255) NULL,
	[InTP] BIT NULL DEFAULT 0,
	[WorkingEcosystem] NVARCHAR(255) NULL,
	[TDC] VARCHAR(255) NULL
)
