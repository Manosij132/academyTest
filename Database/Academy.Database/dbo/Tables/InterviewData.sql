CREATE TABLE [dbo].[InterviewData](
	[Id] INT IDENTITY(1,1) NOT NULL,
	[GKReject] INT NULL,
	[GKSelect] INT NULL,
	[L1Select] INT NULL,
	[L1Reject] INT NULL,
	[GrandTotal] INT NULL,	
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_InterviewData] PRIMARY KEY ([Id])
)


