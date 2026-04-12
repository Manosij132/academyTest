CREATE PROCEDURE [dbo].[usp_InsertDeleteToSyncEmployees] 
	@udtEmployees dbo.udt_Employees READONLY
AS
BEGIN
	BEGIN TRY
		DROP TABLE IF EXISTS #DeletedEmployees
		DROP TABLE IF EXISTS #NewEmployees

		BEGIN TRANSACTION SyncEmployee;

		EXEC dbo.usp_UpdateDojoDetailForEmpSync @udtEmployees;  -- Update DojoDetail table to Deactivate and update employees moving into Dojo
		
		-- Step 1: Mark Employees as Inactive if They No Longer Exist in `@udtEmployees

		--If an employee exists in employee table but not in udt,
		--deactivate the employee. Reason being employees who have already 
		--left the organization do not appear in @udtEmployees

		SELECT e.Id
		INTO #DeletedEmployees
		FROM Employee e
		LEFT JOIN @udtEmployees udt
			ON e.GlobantEmailAddress = udt.GlobantEmailId
		WHERE udt.GlobantEmailId IS NULL AND e.IsActive = 1;

		IF EXISTS (SELECT 1 FROM #DeletedEmployees)
		BEGIN
			UPDATE e
			SET IsActive = 0,
				[Status] = 'Out of Company',
				UpdatedBy = 0,
				UpdatedOn = GETUTCDATE()
			FROM dbo.Employee e
			INNER JOIN #DeletedEmployees de 
			ON e.Id = de.Id;

			UPDATE dd
			SET DojoEndDate = GETUTCDATE(), 
				IsActive = 0,
				UpdatedOn = GETUTCDATE(),
				UpdatedBy = 0
			FROM dbo.DojoDetail dd
			INNER JOIN #DeletedEmployees de 
			ON de.Id = dd.EmployeeId
			WHERE dd.IsActive = 1;
		END
		DROP TABLE IF EXISTS #DeletedEmployees;

		--  Step 2: Insert New Employees (Avoiding Duplicates)
		SELECT DISTINCT udt.*
		INTO #NewEmployees
		FROM @udtEmployees udt
		LEFT JOIN dbo.Employee e 
		ON udt.GlobantEmailId = e.GlobantEmailAddress
		WHERE e.Id IS NULL
			AND udt.Client NOT IN ('', ' ', '#n/a')
			AND udt.Project NOT IN ('', ' ', '#n/a')
			AND udt.Community NOT IN ('', ' ', '#n/a')

		IF EXISTS (SELECT 1 FROM #NewEmployees)
		BEGIN
			INSERT INTO Employee
			(GlobalId, GlobantEmailAddress, EmployeeName, Aging, 
			 BaseLocation, BetterMeLeaderEmail, Client, Community, 
			 Designation, Position, Gender, GexLeaders, [Image], 
			 JoiningDate, MobileNo, MyGrowthReminderCount, NoOfDays, 
			 NotificationSendCount, Project, [Status], Tdc, Seniority,  
			 TotalExperience, WorkingEcosystem, AiStudio, IsActive, 
			 CreatedBy, CreatedOn)
			SELECT	udt.GlobalId, udt.GlobantEmailId, udt.EmployeeName, udt.GlobantTenure, 
					udt.BaseLocation, udt.CareerLeader, udt.Client, udt.Community, 
					udt.Position , udt.Position, udt.Gender, udt.GexLeaders, NULL, 
					udt.JoiningDate, NULL , 0 , 0 , 
					0 , udt.Project, ISNULL(udt.[Status], 'Unknown'), udt.TDC, '',
					udt.TotalExperience, udt.WorkingEcosystem, ais.AiStudioName, 1, 
					0, GETUTCDATE()
			FROM #NewEmployees udt
			LEFT JOIN dbo.AiStudioClientMap ais
				ON udt.Client = ais.Client
			WHERE ISNULL(ais.IsActive, 1) = 1;

			-- Insert new employees Dojo data into DojoDetail table
			INSERT INTO dbo.DojoDetail 
			(EmployeeId, DojoStartDate, DojoProjectsConfigurationId, IsActive, CreatedOn, CreatedBy)
			SELECT DISTINCT e.Id, GETUTCDATE(), dpc.DojoProjectsConfigurationId, 1, GETUTCDATE(), 0
			FROM dbo.Employee e
			INNER JOIN #NewEmployees emp ON e.GlobantEmailAddress = emp.GlobantEmailId
			INNER JOIN dbo.DojoProjectsConfiguration dpc
				ON emp.Project = dpc.ProjectName
			WHERE dpc.IsActive = 1;
			
		END
		DROP TABLE IF EXISTS #NewEmployees;

		COMMIT TRANSACTION SyncEmployee;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION SyncEmployee;
		THROW
	END CATCH;
END
