CREATE TABLE [dbo].[PanelSlotUtilization](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PanelId] [int] NOT NULL,
	[TotalSlots] [int] NOT NULL,
	[SlotUnutilized] [int] NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
 CONSTRAINT [PK_PanelSlotUtilization] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PanelSlotUtilization]  WITH CHECK ADD  CONSTRAINT [FK_PanelSlotUtilization_InterviewPanelDetails_PanelId] FOREIGN KEY([PanelId])
REFERENCES [dbo].[InterviewPanelDetails] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PanelSlotUtilization] CHECK CONSTRAINT [FK_PanelSlotUtilization_InterviewPanelDetails_PanelId]
GO


