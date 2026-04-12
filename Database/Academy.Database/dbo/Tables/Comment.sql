CREATE TABLE [dbo].[Comment]
(
	[CommentId] INT IDENTITY(1,1) NOT NULL,
	[EmployeeId] INT NOT NULL,
    [CommentText] NVARCHAR(500) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_Comment] PRIMARY KEY ([CommentId])
)
GO

CREATE NONCLUSTERED INDEX ix_Comment_EmployeeId
ON [dbo].[Comment] (EmployeeId ASC)
INCLUDE (CommentText)
GO