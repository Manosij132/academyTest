CREATE TABLE [dbo].[JobRequest]
(
	[RequestId] INT NOT NULL IDENTITY(1,1),
	[TransactionId] VARCHAR(20) NOT NULL,
	[RequestType] VARCHAR(50) NOT NULL,
	[RequestMetadata] VARCHAR(500) NULL,
	[Status] VARCHAR(15) NOT NULL DEFAULT 'Pending',
	[HasErrors] BIT NOT NULL DEFAULT 0,
	[ErrorDetail] VARCHAR(MAX) NULL,
	[RetryCount] TINYINT NOT NULL DEFAULT 0,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
    CONSTRAINT [PK_JobRequest] PRIMARY KEY ([RequestId])
)
GO

CREATE NONCLUSTERED INDEX ix_JobRequest_TransactionId
ON dbo.JobRequest (TransactionId)
INCLUDE (RequestType, Status, HasErrors, ErrorDetail)
GO