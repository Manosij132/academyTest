DECLARE @scriptName VARCHAR(255) = 'Academy_1.9.sql';
DECLARE @reqVersion VARCHAR(20) = '1.8';
DECLARE @newVersion VARCHAR(20) = '1.9';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;
			
			INSERT INTO dbo.ScheduledJob
			(JobName, JobDescription, JobSchedule, JobState, IsActive, CreatedBy, CreatedOn)
			SELECT JobName, JobDescription, JobSchedule, JobState, IsActive, CreatedBy, CreatedOn
			FROM
			(
				VALUES
				('Academy -- ACADEMY_TRAINING_ASSIGNMENT', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- Buddy Connect', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- Daily Reminder', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- DOJO_WFO_REMINDER', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- EMPLOYEE_SYNC', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- GlowStaffRequests', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- Mail Engine', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- Raw Data Refresh', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- SYNC_GU_TRAINING_STATUS', '', NULL, 'Enabled', 1, 0, GETUTCDATE()),
				('Academy -- Training Feedback Reminder', '', NULL, 'Enabled', 1, 0, GETUTCDATE())
			) AS SJ 
			(JobName, JobDescription, JobSchedule, JobState, IsActive, CreatedBy, CreatedOn)
			WHERE NOT EXISTS 
			(
				SELECT 1 
				FROM dbo.ScheduledJob 
				WHERE JobName = SJ.JobName
			);

			COMMIT TRANSACTION;

			EXEC sysdata.SetDBVersion @newVersion, @scriptName;

			PRINT 'Script ' + @scriptName + ' completed successfully.';
		END TRY
		BEGIN CATCH
			-- Rollback the transactions
			PRINT 'ERROR OCCURRED! All changes will be rolled back ' + @scriptName;
			PRINT ERROR_MESSAGE();

			IF (@@TRANCOUNT > 0)
				ROLLBACK TRANSACTION;

			THROW
		END CATCH
	END
	ELSE
	BEGIN
		IF (sysdata.IsDbVersionApplied(@newVersion) = 1)
			PRINT 'Script (' + @scriptName + ') Version' + @newVersion + ' already applied!';

		IF (sysdata.IsDbVersionApplied(@reqVersion) = 0)
			PRINT 'ERROR: The script (' + @scriptName + ') requires DB version ' + @reqVersion;
	END
END
GO