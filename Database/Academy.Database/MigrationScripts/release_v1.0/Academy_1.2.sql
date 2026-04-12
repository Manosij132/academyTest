DECLARE @scriptName VARCHAR(255) = 'Academy_1.2.sql';
DECLARE @reqVersion VARCHAR(20) = '1.1';
DECLARE @newVersion VARCHAR(20) = '1.2';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;

			IF NOT EXISTS (SELECT TOP 1 1 FROM Employee WHERE Id = 0 AND GlobantEmailAddress = 'india-training-focal@globant.com')
			BEGIN
				SET IDENTITY_INSERT dbo.Employee ON 

				INSERT INTO Employee
				([Id], [EmployeeName], [GlobantEmailAddress], [BetterMeLeaderEmail], [Seniority], [SeniorityId], [Tdc], 
				 [Community], [Client], [Project], [BaseLocation], [Designation], [Position], [JoiningDate], [MobileNo], 
				 [TotalExperience], [Aging], [Gender], [NoOfDays], [NotificationSendCount], [ProjectManagerEmail], 
				 [ProjectTL], [ProjectTLEmailsCsv], [ProposedLeaderEmail], [GlobalId], [Status], [Image], [OnHoldBy], 
				 [OnHoldForProject], [OtherInfo], [ProfileLink], [ResumeLink], [IsNewJoiner], [Comments], [GexLeaders], 
				 [MyGrowthReminderCount], [WorkingEcosystem], [EcosystemId], [IsActive], [CreatedBy], [CreatedOn], 
				 [UpdatedBy], [UpdatedOn])
				VALUES
				(0, 'System Administrator', 'india-training-focal@globant.com', NULL, 'NA', 10, 'Asia',
				 'DOTNET', 'Globant', 'Academy', 'Hinjewadi', 'Admin', 'Admin', '2021-08-09', NULL,
				 0, 0, 'Male', 0, 0, NULL,
				 NULL, NULL, NULL, NULL, 'Resigned', 'https://ik.imagekit.io/3dyoaljqt/system_adminstrator.png', NULL,
				 NULL, NULL, NULL, NULL, 0, NULL, NULL,
				 0, 'Admin', NULL, 1, 12170, '2021-08-09',
				 NULL, NULL)
			END

			SET IDENTITY_INSERT dbo.Employee OFF
			
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