CREATE PROCEDURE [dbo].[usp_SyncTrainingReminders]
AS
BEGIN
	-- Insert entries in reminder table for the trainings assigned to employee and does not exists in reminder table.
	INSERT INTO dbo.EmployeeTrainingReminder
	(EmployeeId, EmployeeTrainingId, ReminderCount, IsActive, CreatedBy, CreatedOn)
	SELECT ETM.EmployeeId, ETM.EmployeeTrainingId, 0, 1, 0, GETUTCDATE()
	FROM EmployeeTrainingMap ETM
	INNER JOIN Employee E
		ON ETM.EmployeeId = E.Id
	LEFT JOIN EmployeeTrainingReminder ETR
		ON ETM.EmployeeId = ETR.EmployeeId
			AND ETM.EmployeeTrainingId = ETR.EmployeeTrainingId
	WHERE ETR.EmployeeTrainingReminderId IS NULL
		AND E.IsActive = 1
		AND ETM.TrainingStatusId NOT IN (2,4)
		AND E.Tdc IN ('Asia', 'India')

	-- If Training status is complete then deactivate the entry
	UPDATE ETR
	SET ETR.IsActive = 0,
		ETR.UpdatedOn = GETUTCDATE(),
		ETR.UpdatedBy = 0
	FROM EmployeeTrainingReminder ETR
	INNER JOIN EmployeeTrainingMap ETM
		ON ETR.EmployeeTrainingId = ETM.EmployeeTrainingId
	WHERE ETM.TrainingStatusId IN (2,4) AND ETR.IsActive = 1

	-- If EmployeeMetadata has MetaKey as "BypassTrainingReminder" then deactivate the entry.
	UPDATE ETR
	SET ETR.IsActive = 0,
		ETR.UpdatedOn = GETUTCDATE(),
		ETR.UpdatedBy = 0
	FROM EmployeeTrainingReminder ETR
	INNER JOIN EmployeeMetadata EM
		ON ETR.EmployeeId = EM.EmployeeId
	WHERE EM.MetaKey IN ('BypassTrainingReminder', 'MailException')
		AND EM.MetaValue = '1'
		AND ETR.IsActive = 1;

	-- Delete entry from Reminder table if employee resigns.
	DELETE ETR
	FROM EmployeeTrainingReminder ETR
	INNER JOIN Employee E ON ETR.EmployeeId = E.Id
	WHERE E.IsActive = 0;
END
