CREATE TABLE [dbo].[EmployeeRoleMap]
(
	[EmployeeRoleId] INT IDENTITY(1,1) NOT NULL,
	[EmployeeId] INT NOT NULL,
	[RoleId] TINYINT NOT NULL,
	[RoleAssignment] NVARCHAR(100) NOT NULL, -- Implies Role assign to specific domain.
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_EmployeeRoleMap] PRIMARY KEY ([EmployeeRoleId])
)
GO

CREATE NONCLUSTERED INDEX ix_EmployeeRoleMap_EmployeeId
ON [dbo].[EmployeeRoleMap] (EmployeeId ASC)
GO

CREATE NONCLUSTERED INDEX ix_EmployeeRoleMap_RoleId
ON [dbo].[EmployeeRoleMap] (RoleId ASC)
INCLUDE (RoleAssignment)
GO