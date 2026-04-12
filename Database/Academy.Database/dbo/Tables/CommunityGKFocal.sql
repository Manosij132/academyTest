CREATE TABLE [dbo].[CommunityGKFocal]
(
	[Id] INT IDENTITY(1,1) NOT NULL,
	[CommunityId] INT NOT NULL,	
	[GKFocalEmailId] NVARCHAR(MAX) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [FK_CommunityGKFocal_Community] FOREIGN KEY([CommunityId]) REFERENCES [dbo].[Community] ([Id])
)