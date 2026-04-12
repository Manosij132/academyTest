CREATE TABLE [dbo].[PositionPlan]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OpenPositionId] [decimal](18, 0) NOT NULL,
	[GlobarId] [decimal](18, 0) NULL,
	[GloberName] [nvarchar](255) NULL,
	[GloberType] [nvarchar](255) NULL,
	[PlanType] [nvarchar](255) NULL,
	[IsActive] [bit] NULL  DEFAULT (1), 
    CONSTRAINT [PK_PositionPlan] PRIMARY KEY ([Id]), 
    CONSTRAINT [FK_PositionPlan_Positions] FOREIGN KEY ([OpenPositionId]) REFERENCES [dbo].[Position]([PositionId]),
)
GO