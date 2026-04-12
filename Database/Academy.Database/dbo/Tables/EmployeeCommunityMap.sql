CREATE TABLE [dbo].[EmployeeCommunityMap](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[CommunityId] [int] NOT NULL,
	[EmployeeId] [int] NOT NULL,	
	[IsActive] [bit] NOT NULL,
    [CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
 CONSTRAINT [PK_EmployeeCommunityMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EmployeeCommunityMap]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeCommunityMap_Community] FOREIGN KEY([CommunityId])
REFERENCES [dbo].[Community] ([Id])
GO

ALTER TABLE [dbo].[EmployeeCommunityMap]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeCommunityMap_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
GO


