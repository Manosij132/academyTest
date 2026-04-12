/****** Object:  UserDefinedTableType [dbo].[udt_PanelSeniority]    Script Date: 3/18/2026 5:45:01 PM ******/
CREATE TYPE [dbo].[udt_PanelSeniority] AS TABLE(
	[Email] [varchar](255) NULL,
	[PanelType] [varchar](100) NULL,
	[Seniority] [varchar](200) NULL
)
GO


