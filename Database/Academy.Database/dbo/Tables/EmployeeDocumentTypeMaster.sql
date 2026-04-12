CREATE TABLE [dbo].[EmployeeDocumentTypeMaster]
(
	[EmployeeDocumentTypeId] TINYINT NOT NULL IDENTITY(1,1),
	[DocumentType] VARCHAR(30) NOT NULL,
	[IsEligibleForReminder] BIT NOT NULL DEFAULT 0,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_EmployeeDocumentType] PRIMARY KEY ([EmployeeDocumentTypeId])
)
