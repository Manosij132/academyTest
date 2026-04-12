DECLARE @scriptName VARCHAR(255) = 'Academy_1.8.sql';
DECLARE @reqVersion VARCHAR(20) = '1.7';
DECLARE @newVersion VARCHAR(20) = '1.8';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;
			IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.ReportType WHERE ReportId = 5)
			BEGIN
			INSERT INTO [dbo].[ReportType] 
			([ReportId],[ReportName],[StoredProcName],[IsActive],[CreatedBy],[CreatedOn],[UpdatedBy],[UpdatedOn])
			VALUES  
			(5, 'Proficiency Data Report','usp_FetchProficiencyDataReport_Detailed', 1, 0, GETUTCDATE(), 0, GETUTCDATE())

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