CREATE PROCEDURE [dbo].[usp_UpdateDojoDetailForEmpSync] 
    @udtEmployees dbo.udt_Employees READONLY
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PendingTrainingStatusId TINYINT = (SELECT TOP 1 TrainingStatusId 
                                                    FROM dbo.TrainingStatusMaster 
                                                    WHERE TrainingStatusName = 'Pending');

        DROP TABLE IF EXISTS #EmployeesMovingOutOfDojo

        -- Deactivate employees who moved out of Dojo
        SELECT e.Id
        INTO #EmployeesMovingOutOfDojo
        FROM dbo.DojoDetail dd
        INNER JOIN dbo.Employee e ON dd.EmployeeId = e.Id
        INNER JOIN @udtEmployees udt ON e.GlobantEmailAddress = udt.GlobantEmailId
        INNER JOIN dbo.DojoProjectsConfiguration dpe ON e.Project = dpe.ProjectName
        LEFT JOIN dbo.DojoProjectsConfiguration dpudt ON udt.Project = dpudt.ProjectName
        WHERE ISNULL(udt.Project, '') <> '' 
            AND dd.IsActive = 1
            AND (dpudt.ProjectName IS NULL OR dpe.ProjectName <> dpudt.ProjectName)
            AND udt.Client NOT IN ('', ' ', '#n/a')
			AND udt.Project NOT IN ('', ' ', '#n/a')
			AND udt.Community NOT IN ('', ' ', '#n/a')

        --Deactivate entries for employees moving out of Dojo in DojoDetail
        UPDATE dd
        SET dd.IsActive = 0, 
            dd.DojoEndDate = GETUTCDATE(),
            dd.UpdatedOn = GETUTCDATE(),
            dd.UpdatedBy = 0
        FROM dbo.DojoDetail dd
        INNER JOIN #EmployeesMovingOutOfDojo emod ON dd.EmployeeId = emod.Id
        WHERE dd.IsActive = 1;

        -- Update EmployeeDocument entries for employees moving out of Dojo to set IsUpdateRequired to 0 and reset reminder counts
        UPDATE ed
        SET ed.IsUpdateRequired = 0,
            ed.ReminderCount = 0,
            ed.LastReminderSentOn = NULL,
            ed.UpdatedOn = GETUTCDATE(),
            ed.UpdatedBy = 0
        FROM dbo.EmployeeDocument ed
        INNER JOIN #EmployeesMovingOutOfDojo emod ON ed.EmployeeId = emod.Id
        WHERE ed.IsActive = 1;

        --For globers coming back on dojo
        -- Deactivate previous Dojo entries and insert new ones for employees moving into Dojo
        UPDATE dd
        SET dd.IsActive = 0,
            dd.DojoEndDate = GETUTCDATE(),
            dd.UpdatedOn = GETUTCDATE(),
            dd.UpdatedBy = 0
        FROM dbo.DojoDetail dd
        INNER JOIN dbo.Employee e ON dd.EmployeeId = e.Id
        INNER JOIN @udtEmployees udt ON e.GlobantEmailAddress = udt.GlobantEmailId
        INNER JOIN dbo.DojoProjectsConfiguration dpudt ON udt.Project = dpudt.ProjectName
        LEFT JOIN dbo.DojoProjectsConfiguration dpe ON e.Project = dpe.ProjectName
        WHERE dd.IsActive = 1 
            AND dd.DojoEndDate IS NULL
            AND dpe.DojoProjectsConfigurationId IS NULL
            AND dpudt.DojoProjectsConfigurationId IS NOT NULL;

        DROP TABLE IF EXISTS #NewDojoEntries;
            
        -- Insert new Dojo entry if employee is moving into Dojo or continuing in Dojo but has no active record
        SELECT DISTINCT e.Id AS EmployeeId, dpc.DojoProjectsConfigurationId, dpc.IsAssignable
        INTO #NewDojoEntries
        FROM dbo.Employee e
        INNER JOIN @udtEmployees udt ON e.GlobantEmailAddress = udt.GlobantEmailId
        INNER JOIN dbo.DojoProjectsConfiguration dpc ON udt.Project = dpc.ProjectName
        LEFT JOIN dbo.DojoDetail dd ON e.Id = dd.EmployeeId AND dd.IsActive = 1
        WHERE dd.EmployeeId IS NULL
            AND dpc.IsActive = 1;

        INSERT INTO dbo.DojoDetail 
        (EmployeeId, DojoStartDate, DojoProjectsConfigurationId, IsActive, CreatedOn, CreatedBy)
        SELECT DISTINCT EmployeeId, GETUTCDATE(), DojoProjectsConfigurationId, 1, GETUTCDATE(), 0
        FROM #NewDojoEntries

        --Update EmployeeTrainingMap Training Start date to dojo start date for all 
        --trainings that were already assigned but are still pending.
        UPDATE eam
        SET eam.StartDate = GETUTCDATE(),
            eam.ExpectedEndDate = DATEADD(DAY, 21, GETUTCDATE()),
            eam.UpdatedOn = GETUTCDATE(),
            eam.UpdatedBy = 0
        FROM EmployeeTrainingMap eam
        INNER JOIN #NewDojoEntries nde 
            ON eam.EmployeeId = nde.EmployeeId
        WHERE eam.TrainingStatusId = @PendingTrainingStatusId

        INSERT INTO dbo.Comment
        (EmployeeId, CommentText, IsActive, CreatedBy, CreatedOn)
        SELECT DISTINCT EmployeeId, 'Training dates realigned for all pending trainings as per dojo guidelines.', 1, 0, GETUTCDATE()
        FROM #NewDojoEntries;

        --Update EmployeeDocument IsUpdateRequired to 1 for all documents of employees moving into Dojo.
        UPDATE ed
        SET ed.IsUpdateRequired = 1,
            ed.UpdatedOn = GETUTCDATE(),
            ed.UpdatedBy = 0
        FROM dbo.EmployeeDocument ed
        INNER JOIN #NewDojoEntries nde 
            ON ed.EmployeeId = nde.EmployeeId
        WHERE ed.IsActive = 1
            AND ed.IsUpdateRequired = 0
            AND nde.IsAssignable = 1; -- Only set IsUpdateRequired for employees who are assignable Dojo as per Dojo Project Configuration
        
        DROP TABLE IF EXISTS #NewDojoEntries;
        DROP TABLE IF EXISTS #EmployeesMovingOutOfDojo

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
