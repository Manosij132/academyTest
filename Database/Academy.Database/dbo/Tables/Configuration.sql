CREATE TABLE [dbo].[Configuration]
(
	[ConfigurationId] SMALLINT IDENTITY(1,1) NOT NULL,
	[Environment] VARCHAR(10) NOT NULL, 
    [Key] VARCHAR(200) NOT NULL, 
    [Value] NVARCHAR(3000) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 0,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(), 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_Configuration] PRIMARY KEY ([ConfigurationId])
)
