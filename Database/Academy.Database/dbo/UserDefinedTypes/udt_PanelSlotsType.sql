/****** Object:  UserDefinedTableType [dbo].[PanelSlotsType]    Script Date: 3/18/2026 5:49:25 PM ******/
CREATE TYPE [dbo].[udt_PanelSlotsType] AS TABLE(
	[PanelId] [int] NULL,
	[SlotDate] [datetime] NULL,
	[Recruiter] [nvarchar](255) NULL,
	[CandidateName] [nvarchar](255) NULL,
	[IsUtilized] [bit] NULL,
	[IsActive] [bit] NULL,
	[CreatedBy] [int] NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedOn] [datetime] NULL
)
GO


