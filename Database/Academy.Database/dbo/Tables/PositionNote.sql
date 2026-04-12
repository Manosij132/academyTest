CREATE TABLE [dbo].[PositionNote]
(
	[Id] [int] IDENTITY(1,1)  NOT NULL,
	[OpenPositionId] [decimal](18, 0) NOT NULL,
	[Author] [nvarchar](255) NULL,
	[Comment] [nvarchar](max) NULL,
	[NoteDate] [datetime] NULL, 
	CONSTRAINT [PK_PositionNote] PRIMARY KEY (Id),
    CONSTRAINT [FK_PositionNote_Position] FOREIGN KEY ([OpenPositionId]) REFERENCES [dbo].[Position](PositionId)
)
GO