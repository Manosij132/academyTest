DECLARE @scriptName VARCHAR(255) = 'Academy_1.7.sql';
DECLARE @reqVersion VARCHAR(20) = '1.6';
DECLARE @newVersion VARCHAR(20) = '1.7';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;

			--ReportColumnConfiguration table insertion script
			Update ReportType set ReportName = 'Area Path Detailed Report', StoredProcName='usp_FetchAreaPathReport_Detailed' where ReportId=3
				
			Update ReportType set ReportName = 'Area Path Sumarised Report', StoredProcName='usp_FetchAreaPathReport_Summary' where ReportId=4
				
			delete ReportColumnConfiguration where ReportColumnConfigId=7

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