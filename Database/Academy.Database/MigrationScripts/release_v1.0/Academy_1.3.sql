DECLARE @scriptName VARCHAR(255) = 'Academy_1.3.sql';
DECLARE @reqVersion VARCHAR(20) = '1.2';
DECLARE @newVersion VARCHAR(20) = '1.3';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;

			-- LearningPath table insertion script
			IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.LearningPath WHERE LearningPathId = 0)
			BEGIN
				SET IDENTITY_INSERT dbo.LearningPath ON

				INSERT INTO dbo.LearningPath 
				([LearningPathId],[LearningPathName],[LearningPathDescription],[LearningPathUrl],[IsActive],[CreatedBy],[CreatedOn],[UpdatedBy],[UpdatedOn])
				VALUES
				(1, 'Globant Enterprise AI','Globant Enterprise AI','https://university.globant.com/learning-track/globant-enterprise-ai',1,0, GETUTCDATE(), 0, GETUTCDATE()),
				(2, 'AI PODs - Tooling','AI PODs - Tooling','https://university.globant.com/learning-track/ai-pods-tooling',1,0, GETUTCDATE(), 0, GETUTCDATE()),
				(3, 'AI-Driven Transformation at Globant','AI-Driven Transformation at Globant','https://university.globant.com/learning-track/ai-driven-transformation-globant',1,0, GETUTCDATE(), 0, GETUTCDATE()),
				(4, 'AI POD Engineering Expert','AI POD Engineering Expert','https://university.globant.com/learning-track/ai-pod-engineering-expert',1,0, GETUTCDATE(), 0, GETUTCDATE()),
				(5, 'AI POD Architect Fundamentals','AI POD Architect Fundamentals','https://university.globant.com/learning-track/ai-pod-architect-fundamentals',1,0, GETUTCDATE(), 0, GETUTCDATE())
			END

			SET IDENTITY_INSERT dbo.LearningPath OFF
			SET IDENTITY_INSERT dbo.LearningPath OFF
			
			-- LearningPathTrainingMap table insertion script
			IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.LearningPathTrainingMap WHERE LearningPathTrainingMapId = 0)
			BEGIN
				SET IDENTITY_INSERT dbo.LearningPathTrainingMap ON
				INSERT INTO dbo.LearningPathTrainingMap 
				([LearningPathTrainingMapId],[SeniorityId],[TrainingId],[LearningPathId],[IsActive],[CreatedBy],[CreatedOn],[UpdatedBy],[UpdatedOn])
				VALUES
				(1, 4, 10248, 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(2, 5, 10248, 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(3, 6, 10248, 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(4, 5, 10248, 2, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(5, 4, 10158, 4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(6, 6, 10158, 4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(7, 5, 10158, 4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(8, 3, 10158, 4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(9, 3, 10158, 5, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(10, 4, 10158, 5, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(11, 5, 10158, 5, 1, 0, GETUTCDATE(), 0, GETUTCDATE())
			END
			SET IDENTITY_INSERT dbo.LearningPathTrainingMap OFF

			--ReportType table insertion script
			IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.ReportType WHERE ReportId = 0)
			BEGIN
				INSERT INTO [dbo].[ReportType] 
				([ReportId],[ReportName],[StoredProcName],[IsActive],[CreatedBy],[CreatedOn],[UpdatedBy],[UpdatedOn])
				VALUES  
				(1, 'Detailed Report','usp_FetchDynamicEmployeeTrainingReport_Detailed', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(2, 'Sumarised Report','usp_FetchDynamicEmployeeTrainingReport_Summary', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(3, 'Compliance Report','usp_FetchDynamicEmployeeTrainingReport_Compliance', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
				(4, 'Sumarised Compliance Report','usp_FetchDynamicEmployeeTrainingReport_ComplianceSummary', 1, 0, GETUTCDATE(), 0, GETUTCDATE())
			END

			--ReportColumnConfiguration table insertion script
			IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.ReportColumnConfiguration WHERE ReportColumnConfigId = 0)
			BEGIN
				INSERT INTO  [dbo].[ReportColumnConfiguration]
				([ReportColumnConfigId],[ReportColumnName],[ReportColumnDisplayName],[IsGroupBy],[IsActive],[CreatedBy],[CreatedOn],[UpdatedBy],[UpdatedOn])
				VALUES
				(1,'Employee.Tdc','TDC',1,1,1,GETDATE(),null,null),
				(2,'Employee.Community','Community',1,1,1,GETDATE(),null,null),
				(3,'Employee.Project','Project',1,1,1,GETDATE(),null,null),
				(4,'Employee.Seniority','Seniority',1,1,1,GETDATE(),null,null),
				(5,'TrainingMaster.TrainingName','Training',1,1,1,GETDATE(),null,null)
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