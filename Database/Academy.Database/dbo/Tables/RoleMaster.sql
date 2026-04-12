CREATE TABLE [dbo].[RoleMaster]
(
	[RoleId] TINYINT NOT NULL,
	[RoleName] NVARCHAR(50) NOT NULL,
	[DisplayName] NVARCHAR(50) NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
)
