CREATE TABLE [dbo].[PanelSlotsRequirement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TDC] [nvarchar](max) NULL,
	[CommunityId] [int] NULL,
	[SeniorityId] [smallint] NOT NULL,
	[StartDate] [datetime2](7) NULL,
	[PositionToBeFilled] [int] NOT NULL,
	[DropRatio] [decimal](18, 2) NOT NULL,
	[OffersToBeRolledOut] [int] NOT NULL,
	[L1SlotsRequired] [int] NULL,
	[L1SlotsActual] [int] NULL,
	[GKSlotsRequired] [int] NULL,
	[GKSlotsActual] [int] NULL,
	[EndDate] [datetime2](7) NULL,
	[L1Panels] [int] NULL,
	[GKPanels] [int] NULL,
	[L1SelectionRatio] [decimal](18, 2) NULL,
	[GKSelectionRatio] [decimal](18, 2) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
 CONSTRAINT [PK_PanelSlotsRequirement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[PanelSlotsRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PanelSlotsRequirement_Community_CommunityId] FOREIGN KEY([CommunityId])
REFERENCES [dbo].[Community] ([Id])
GO

ALTER TABLE [dbo].[PanelSlotsRequirement] CHECK CONSTRAINT [FK_PanelSlotsRequirement_Community_CommunityId]
GO

ALTER TABLE [dbo].[PanelSlotsRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PanelSlotsRequirement_Seniority_SeniorityId] FOREIGN KEY([SeniorityId])
REFERENCES [dbo].[SeniorityMaster] ([SeniorityId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PanelSlotsRequirement] CHECK CONSTRAINT [FK_PanelSlotsRequirement_Seniority_SeniorityId]
GO


