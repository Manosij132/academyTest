CREATE TYPE [dbo].[udt_GloberTrainingStatus] AS TABLE
(
	[GloberEmail] NVARCHAR(500) NULL,
	[TrainingLink] VARCHAR(255) NULL,
	[TopicStatusId] INT NULL,
	[UpdatedOn] DATETIME2(3) NULL,
	[UpdatedByEmail] NVARCHAR(500) NULL
)