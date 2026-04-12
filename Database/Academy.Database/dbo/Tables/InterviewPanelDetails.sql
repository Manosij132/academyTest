CREATE TABLE [dbo].[InterviewPanelDetails]
(
	[Id] INT IDENTITY(1,1) NOT NULL,
	[PrimaryPanelId] INT NOT NULL,
	[SecondaryPanelId] INT NULL,
	[Type] VARCHAR(4) NULL,
	[SeniorityUpTo] VARCHAR(40) NULL,
	[CommunityId] INT NOT NULL,	
	[SeniorityId] SMALLINT NOT NULL DEFAULT 0,
	[TDC] VARCHAR(40) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_InterviewPanelDetails] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_InterviewPanelDetails_Community_CommunityId] FOREIGN KEY([CommunityId]) 
		REFERENCES [dbo].[Community] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_InterviewPanelDetails_Seniority_SeniorityId] FOREIGN KEY([SeniorityId])
		REFERENCES [dbo].[SeniorityMaster] ([SeniorityId]) ON DELETE CASCADE,
	CONSTRAINT [FK_PrimaryPanel_Employee] FOREIGN KEY([PrimaryPanelId])
		REFERENCES [dbo].[Employee] ([Id]),
	CONSTRAINT [FK_SecondaryPanel_Employee] FOREIGN KEY([SecondaryPanelId])
		REFERENCES [dbo].[Employee] ([Id])
)

