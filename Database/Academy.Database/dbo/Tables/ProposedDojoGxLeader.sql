CREATE TABLE [dbo].[ProposedDojoGxLeader]
(
	[ProposedDojoGxLeaderId] INT IDENTITY(1,1) NOT NULL,
	[EmployeeId] INT NOT NULL,
	[ProposedDojoLeaderEmailId] NVARCHAR(255) NULL,
	[GloberName] NVARCHAR(255) NULL,
	[ProposedLeaderName] NVARCHAR(255) NULL,
	[ProposedLeaderSeniority] NVARCHAR(255) NULL,
	[GloberSeniority] NVARCHAR(255) NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_ProposedDojoGxLeader] PRIMARY KEY (ProposedDojoGxLeaderId), 
)
GO

CREATE INDEX [IX_ProposedDojoGxLeader_EmployeeId] 
ON [dbo].[ProposedDojoGxLeader] ([EmployeeId])
INCLUDE (IsActive, ProposedDojoLeaderEmailId)
GO
