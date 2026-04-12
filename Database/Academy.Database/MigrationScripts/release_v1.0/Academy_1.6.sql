DECLARE @scriptName VARCHAR(255) = 'Academy_1.6.sql';
DECLARE @reqVersion VARCHAR(20) = '1.5';
DECLARE @newVersion VARCHAR(20) = '1.6';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;

			--ReportColumnConfiguration table insertion script
			IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.ReportColumnConfiguration WHERE ReportColumnConfigId = 7)
			BEGIN
			INSERT INTO  [dbo].[ReportColumnConfiguration]
				([ReportColumnConfigId],[ReportColumnName],[ReportColumnDisplayName],[IsGroupBy],[IsActive],[CreatedBy],[CreatedOn])
			VALUES
				(7,'LearningPath.LearningPathDescription','AreaPath',1,1,1,GETUTCDATE())
			END

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