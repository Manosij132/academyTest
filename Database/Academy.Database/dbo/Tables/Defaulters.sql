CREATE TABLE [dbo].[Defaulters](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PanelId] [int] NOT NULL,
	[DefaulterCount] [int] NOT NULL,
	[Quarter] [nvarchar](max) NULL,	
	[StartDate] [datetime2](7) NULL,
	[EndDate] [datetime2](7) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
 CONSTRAINT [PK_Defaulters] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Defaulters]  WITH CHECK ADD  CONSTRAINT [FK_Defaulters_InterviewPanelDetails_PanelId] FOREIGN KEY([PanelId])
REFERENCES [dbo].[InterviewPanelDetails] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Defaulters] CHECK CONSTRAINT [FK_Defaulters_InterviewPanelDetails_PanelId]
GO


