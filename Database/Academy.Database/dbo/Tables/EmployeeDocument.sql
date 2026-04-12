CREATE TABLE [dbo].[EmployeeDocument]
(
	[EmployeeDocumentId] INT IDENTITY(1,1),
	[EmployeeId] INT NOT NULL,
	[DocumentLink] NVARCHAR(1024) NULL,
	[DocumentTypeId] TINYINT NOT NULL,
	[ReminderCount] INT NOT NULL DEFAULT 0,
	[LastReminderSentOn] DATETIME NULL,
	[IsUpdateRequired] BIT NOT NULL DEFAULT 0,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_EmployeeDocument] PRIMARY KEY ([EmployeeDocumentId])
)
GO

CREATE INDEX [IX_EmployeeDocument_EmployeeId]
ON [dbo].[EmployeeDocument] ([EmployeeId])
INCLUDE ([DocumentTypeId], [DocumentLink], [ReminderCount], [LastReminderSentOn], [IsActive])
GO