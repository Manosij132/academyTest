CREATE TABLE [dbo].[ActivityMaster]
(
	[ActivityId] SMALLINT NOT NULL, 
    [ActivityName] VARCHAR(100) NOT NULL, 
    [ActivityDescription] VARCHAR(500) NULL, 
    [Priority] DECIMAL(2, 1) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL,
    CONSTRAINT [PK_ActivityMaster] PRIMARY KEY ([ActivityId])
)
GO
