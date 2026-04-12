CREATE TABLE [dbo].[PanelDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[Name] [nvarchar](50) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime2](0) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedOn] [datetime2](0) NULL,
 CONSTRAINT [PK_PanelDetails] PRIMARY KEY ([Id])
 )
GO

ALTER TABLE [dbo].[PanelDetails]  WITH CHECK ADD  CONSTRAINT [FK_PanelDetails_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
GO


