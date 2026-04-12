CREATE PROCEDURE [dbo].[usp_UpdateToSyncEmployees] 
	@udtEmployees dbo.udt_Employees READONLY
AS
BEGIN
	BEGIN TRY
		DROP TABLE IF EXISTS #ExistingEmployees
		DROP TABLE IF EXISTS #EmployeeCTE;

		BEGIN TRANSACTION SyncEmployee;

		--  Step 1: Update Existing Employees
		SELECT DISTINCT udt.*
		INTO #ExistingEmployees
		FROM @udtEmployees udt
		INNER JOIN dbo.Employee e 
		ON udt.GlobantEmailId = e.GlobantEmailAddress;

		UPDATE e
		SET e.BaseLocation = ee.BaseLocation,
			e.Aging = ee.GlobantTenure,
			e.BetterMeLeaderEmail = ee.CareerLeader,
			e.Client = CASE WHEN ISNULL(ee.Client, '') IN ('', ' ', '#n/a') THEN e.Client ELSE ee.Client END,
			e.Project = CASE WHEN ISNULL(ee.Project, '') IN ('', ' ', '#n/a') THEN e.Project ELSE ee.Project END,
			e.Community = CASE WHEN ISNULL(ee.Community, '') IN ('', ' ', '#n/a') THEN e.Community ELSE ee.Community END,
			e.Designation = ee.Position,
			e.Position = ee.Position,
			e.EmployeeName = ee.EmployeeName,
			e.GexLeaders = ee.GexLeaders,
			e.Seniority = CASE
							WHEN ee.Position IN ('Tech Director', 'Tech Manager', 'Subject Matter Expert') THEN ee.Position
							WHEN ee.Position IN ('Studio Partner', 'Tech Partner', 'VP Technology', 'SVP Tech Staff',
												 'SVP Technology') THEN 'Studio Partner'
							WHEN ee.Seniority IN ('Architect', 'Sr Level 3', 'Software Designer', 'Sr Level 2', 'Sr', 
												'Sr Level 1', 'SSr Adv', 'SSr', 'Jr Adv', 'Jr') THEN ee.Seniority
							WHEN ee.Seniority IN ('Manager Sr Level 2') THEN 'Sr Level 2'
							ELSE 'NA' 
						END,
			e.[Status] = ISNULL(ee.[Status], 'Unknown'),
			e.Tdc = ee.Tdc,
			e.TotalExperience = ee.TotalExperience,
			e.WorkingEcosystem = CASE WHEN e.Position = 'QC Analyst' THEN 'QC' 
									  WHEN e.Position = 'Test Automation Engineer' THEN 'TAE'
									  WHEN e.Position = 'Drupal Developer' THEN 'Drupal'
									  WHEN e.Position = 'PHP Developer' THEN 'PHP'
									  WHEN e.Position = 'Flutter Developer' THEN 'Flutter'
									  ELSE e.Position 
								 END,
			e.AiStudio = ais.AiStudioName,
			--Set Isactive as 1 irrespecive of employee resigned or active. 
			--IsActive will be set to 0 only once employee is out of company (in usp_InsertDeleteToSyncEmployees)
			e.IsActive = 1,
			UpdatedBy = 0,
			UpdatedOn = GETUTCDATE()
		FROM dbo.Employee e
		INNER JOIN #ExistingEmployees ee 
			ON e.GlobantEmailAddress = ee.GlobantEmailId
		LEFT JOIN dbo.AiStudioClientMap ais 
			ON ee.Client = ais.Client
		WHERE ISNULL(ais.IsActive, 1) = 1
		
		DROP TABLE IF EXISTS #ExistingEmployees;

		--  Step 2: Update Ecosystem based on WorkingEcosystem set above
		UPDATE E
		SET EcosystemId = EM.EcosystemId
		FROM Employee E
		INNER JOIN EcosystemMaster EM 
		ON E.WorkingEcosystem = EM.EcosystemName;
		
		-- Step 3: Update SeniorityId based on Seniority name
		UPDATE E
		SET E.SeniorityId = ISNULL(SM.SeniorityId, 0) --Defaulted to 0 on purpose to track if any seniority is missed. No seniority id should be 0.
		FROM Employee E
		LEFT JOIN SeniorityMaster SM 
		ON E.Seniority = SM.SeniorityName;
		
		-- Step 4: Create a temporary table to store Employee IDs based on the Global IDs and Projects
        CREATE TABLE #EmployeeCTE (EmployeeId INT, IsEmployeeToBeBypassed BIT);

        -- Step 5: Insert Employee IDs into the temporary table with project criteria
        INSERT INTO #EmployeeCTE (EmployeeId, IsEmployeeToBeBypassed)
        SELECT DISTINCT e.Id AS EmployeeId,
				CASE WHEN dpc.DojoProjectsConfigurationId IS NOT NULL AND dpc.IsActive = 1 AND dpc.IsAssignable = 0 THEN 1 
					 ELSE 0 
				END AS IsEmployeeToBeBypassed
        FROM @udtEmployees udt
        INNER JOIN dbo.Employee e
			ON udt.GlobalId = e.GlobalID
		LEFT JOIN dbo.DojoProjectsConfiguration dpc
			ON e.Project = dpc.ProjectName

        -- Step 6: Insert or Update into EmployeeMetaData Table
        MERGE INTO dbo.EmployeeMetaData AS target
        USING 
		(
            SELECT EmployeeId, 'BypassTrainingReminder' AS MetaKey, CAST(IsEmployeeToBeBypassed AS VARCHAR(2)) AS MetaValue, 1 AS IsActive
            FROM #EmployeeCTE
        ) AS source
        ON target.EmployeeId = source.EmployeeId AND target.MetaKey = source.MetaKey
        WHEN MATCHED AND source.MetaValue = '0' THEN
            DELETE
        WHEN NOT MATCHED AND source.MetaValue = '1' THEN
            INSERT 
			(EmployeeId, MetaKey, MetaValue, IsActive, CreatedBy, CreatedOn)
            VALUES 
			(source.EmployeeId, source.MetaKey, source.MetaValue, source.IsActive, 0, GETUTCDATE());

        DROP TABLE IF EXISTS #EmployeeCTE;

		-- Step 7: Add leadership to Mail Exception list in EmployeeMetadata table.
		INSERT INTO dbo.EmployeeMetadata
		(EmployeeId, MetaKey, MetaValue, IsActive, CreatedBy, CreatedOn)
		SELECT e.Id, 'MailException' MetaKey, '1' MetaValue, 1, 0, GETUTCDATE()
		FROM dbo.Employee e
		LEFT JOIN dbo.EmployeeMetadata em 
			ON e.Id = em.EmployeeId AND em.MetaKey = 'MailException'
		WHERE (e.SeniorityId IN (1, 14, 15)
				OR Position IN ('People Rewards', 'IT Experience Manager', 'People Manager', 'Regional Procurement',
								'Corporate Travel', 'People Journey', 'Site Manager', 'Client Partner',
								'Corporate Treasury Analyst', 'Legal Business Counsel')
			)
			AND e.IsActive = 1
			AND em.EmployeeId IS NULL
			AND ISNULL(em.IsActive, 1) = 1

		-- Step 8: Commit Transaction
		COMMIT TRANSACTION SyncEmployee;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION SyncEmployee;
		THROW
	END CATCH;
END
