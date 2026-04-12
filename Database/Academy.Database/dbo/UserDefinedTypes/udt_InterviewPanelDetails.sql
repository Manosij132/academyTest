
/****** Object:  UserDefinedTableType [dbo].[udt_InterviewPanelDetails]    Script Date: 3/18/2026 5:39:56 PM ******/
CREATE TYPE [dbo].[udt_InterviewPanelDetails] AS TABLE(
	[Email] [nvarchar](255) NOT NULL,
	[PanelType] [varchar](2) NOT NULL
)
GO


