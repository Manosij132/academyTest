CREATE TYPE [dbo].[udt_EmployeeTrainingDetail] AS TABLE(
	[EmployeeId] [int] NULL,
	[TrainingId] [int] NULL,
	[ActualEndDate] [datetime] NULL,
	[TrainingName] [nvarchar](150) NULL,
	[Status] [nvarchar](50) NULL,
	[StartDate] [datetime] NULL,
	[Progress] [int] NULL
)
GO