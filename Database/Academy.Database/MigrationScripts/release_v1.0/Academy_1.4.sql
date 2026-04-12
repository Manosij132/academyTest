DECLARE @scriptName VARCHAR(255) = 'Academy_1.4.sql';
DECLARE @reqVersion VARCHAR(20) = '1.3';
DECLARE @newVersion VARCHAR(20) = '1.4';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;

			INSERT INTO dbo.DojoProjectsConfiguration 
			(ProjectName, IsAssignable, IsActive, CreatedBy, CreatedOn)
			VALUES
			('DOJO', 1, 1, 0, GETUTCDATE()),
			('Exusia - Dojo', 1, 1, 0, GETUTCDATE()),
			('eWave - Dojo', 1, 1, 0, GETUTCDATE()),
			('IN - Maternity Leave', 0, 1, 0, GETUTCDATE()),
			('IN - Leave wo Payment', 0, 1, 0, GETUTCDATE()),
			('IN - Vacations (PTO)', 0, 1, 0, GETUTCDATE()),
			('IN - Sick Leave', 0, 1, 0, GETUTCDATE())

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