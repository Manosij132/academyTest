/****** Object:  UserDefinedTableType [dbo].[DefaulterType]    Script Date: 3/18/2026 5:51:59 PM ******/
CREATE TYPE [dbo].[udt_DefaulterType] AS TABLE(
	[PanelId] [int] NULL,
	[DefaulterCount] [int] NULL,
	[Quarter] [nvarchar](10) NULL,
	[StartDate] [datetime2](7) NULL,
	[EndDate] [datetime2](7) NULL,
	[IsActive] [bit] NULL,
	[CreatedBy] [int] NULL,
	[CreatedOn] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedOn] [datetime2](7) NULL
)
GO


