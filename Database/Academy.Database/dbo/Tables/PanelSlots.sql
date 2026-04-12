CREATE TABLE [dbo].[PanelSlots](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PanelId] [int] NOT NULL,
	[SlotDate] [datetime2](7) NOT NULL,
	[Recruiter] [nvarchar](max) NULL,
	[CandidateName] [nvarchar](max) NULL,	
	[IsUtilized] [bit] NULL,
	[CalenderEventID] [nvarchar](1000) NULL,
	[CandidateEmail] [nvarchar](1000) NULL,
	[FileEncoded] [nvarchar](max) NULL,
	[LoggedinUserEmailId] [nvarchar](1000) NULL,
	[ResumeFileName] [nvarchar](max) NULL,
	[EventTitle] [nvarchar](1000) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
 CONSTRAINT [PK_PanelSlots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[PanelSlots]  WITH CHECK ADD  CONSTRAINT [FK_PanelSlots_InterviewPanelDetails_PanelId] FOREIGN KEY([PanelId])
REFERENCES [dbo].[InterviewPanelDetails] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PanelSlots] CHECK CONSTRAINT [FK_PanelSlots_InterviewPanelDetails_PanelId]
GO


