CREATE TABLE [dbo].[JobRequestDetail]
(
	[JobRequestDetailId] INT NOT NULL IDENTITY(1,1), 
	[TransactionId] VARCHAR(20) NOT NULL,
	[GlobantEmailAddress] VARCHAR(255) NOT NULL,
	[Key] VARCHAR(255) NOT NULL,
	[Value] VARCHAR(255) NOT NULL,
	[Status] VARCHAR(15) NOT NULL,
	[Comment] VARCHAR(500) NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
    CONSTRAINT [PK_JobRequestDetail] PRIMARY KEY ([JobRequestDetailId])
)
