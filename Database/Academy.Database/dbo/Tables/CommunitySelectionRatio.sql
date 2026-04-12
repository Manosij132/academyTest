CREATE TABLE [dbo].[CommunitySelectionRatio]
(
	[Id] INT IDENTITY(1,1) NOT NULL,
	[TDC] VARCHAR(50) NOT NULL,
	[CommunityId] INT NOT NULL,
	[L1SelectionRatio] DECIMAL(18, 2) NULL,
	[StartDate] DATETIME2(7) NULL,
	[EndDate] DATETIME2(7) NULL,
	[GKSelectionRatio] DECIMAL(18, 2) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_CommunitySelectionRatio] PRIMARY KEY ([Id]), 
    CONSTRAINT [FK_CommunitySelectionRatio_Community] FOREIGN KEY ([CommunityId]) REFERENCES [dbo].[Community] ([Id])
)


