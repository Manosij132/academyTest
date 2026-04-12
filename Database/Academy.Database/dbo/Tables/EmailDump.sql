CREATE TABLE [dbo].[EmailDump]
(
	[EmailDumpId] INT IDENTITY(1,1) NOT NULL,
    [Subject] NVARCHAR(500) NOT NULL, 
    [Template] NVARCHAR(100) NULL, 
    [To] NVARCHAR(500) NOT NULL, 
    [Cc] NVARCHAR(500) NULL, 
    [Bcc] NVARCHAR(500) NULL, 
    [PlainText] VARCHAR(MAX) NULL, -- Will be used as plain text, if Template = null
    [ErrorText] NVARCHAR(500) NULL, -- Will be populated if failure in sending email.
    [IsActive] BIT NOT NULL DEFAULT 1, -- Will be set to 0 when processed.
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, -- Tells when record was created.
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, -- Tells when email was processed (sent/failed to send).
    CONSTRAINT [PK_EmailDump] PRIMARY KEY ([EmailDumpId])
)
