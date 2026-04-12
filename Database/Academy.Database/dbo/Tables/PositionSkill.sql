CREATE TABLE [dbo].[PositionSkill]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OpenPositionId] [decimal](18, 0) NOT NULL,
	[ExternalSkillId] [int] NULL,
	[SkillName] [nvarchar](255) NULL,
	[SkillValue] [decimal](5, 2) NULL,
	[Importance] [nvarchar](255) NULL, 
    CONSTRAINT [PK_PositionSkill] PRIMARY KEY ([Id]), 
    CONSTRAINT [FK_PositionSkill_Positions] FOREIGN KEY ([OpenPositionId]) REFERENCES [dbo].[Position]([PositionId])
)
GO