DECLARE @scriptName VARCHAR(255) = 'Academy_1.12.sql';
DECLARE @reqVersion VARCHAR(20) = '1.11';
DECLARE @newVersion VARCHAR(20) = '1.12';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;
			INSERT INTO RoleMaster
			(
				RoleId,
				RoleName,
				DisplayName,
				IsActive,
				CreatedBy,
				CreatedOn,
				UpdatedBy,
				UpdatedOn
			)
			VALUES
			(
				6,                       -- Next RoleId (change if needed)
				'Recruiter',             -- RoleName
				'Recruiter',             -- DisplayName
				1,                       -- IsActive (1 = Active)
				0,                       -- CreatedBy
				GETDATE(),               -- CreatedOn
				NULL,                    -- UpdatedBy
				NULL                     -- UpdatedOn
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