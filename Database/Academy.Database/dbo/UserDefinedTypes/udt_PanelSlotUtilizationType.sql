/****** Object:  UserDefinedTableType [dbo].[PanelSlotUtilizationType]    Script Date: 3/18/2026 5:57:57 PM ******/
CREATE TYPE [dbo].[udt_PanelSlotUtilizationType] AS TABLE(
	[PanelId] [int] NULL,
	[TotalSlots] [int] NULL,
	[SlotUnutilized] [int] NULL,
	[IsActive] [bit] NULL,
	[CreatedBy] [int] NULL,
	[CreatedOn] [datetime] NULL
)
GO


