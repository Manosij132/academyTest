CREATE TABLE [dbo].[EmployeeTrainingReminder]
(
	[EmployeeTrainingReminderId] INT IDENTITY(1,1) NOT NULL, 
    [EmployeeId] INT NULL,
	[EmployeeTrainingId] INT NOT NULL,
	[ReminderCount] SMALLINT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_EmployeeTrainingReminder] PRIMARY KEY ([EmployeeTrainingReminderId])
)
GO

CREATE NONCLUSTERED INDEX ix_EmployeeTrainingReminder_EmployeeId_EmployeeTrainingId
ON [dbo].[EmployeeTrainingReminder] (EmployeeId ASC, EmployeeTrainingId)
GO
