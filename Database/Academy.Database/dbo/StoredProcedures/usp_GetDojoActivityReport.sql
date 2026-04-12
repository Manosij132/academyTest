CREATE PROCEDURE [dbo].[usp_GetDojoActivityReport]
	@community VARCHAR(1000) = NULL,
	@Country VARCHAR(1000) = NULL,
	@DojoStartDate NVARCHAR(100) = NULL,
	@DojoEndDate NVARCHAR(100) = NULL,
	@IsPrimary BIT = 1,
	@SearchText VARCHAR(200) = NULL,
	@AiStudio VARCHAR(1000) = NULL,
	@Account VARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON; -- Prevents the count of the number of rows affected from being returned.
	DECLARE @CountryList TABLE (Country NVARCHAR(100));
	IF @Country IS NOT NULL
	BEGIN
		INSERT INTO @CountryList (Country)
		SELECT TRIM(value) AS Country
		FROM STRING_SPLIT(@Country, ',');
	END
	DECLARE @CommunityList TABLE (Community NVARCHAR(100));
	IF @community IS NOT NULL
	BEGIN
		INSERT INTO @CommunityList (Community)
		SELECT TRIM(value) AS Community
		FROM STRING_SPLIT(@Community, ',');
	END
	DECLARE @AiStudioList TABLE (AiStudio NVARCHAR(100));
	IF @AiStudio IS NOT NULL
	BEGIN
		INSERT INTO @AiStudioList (AiStudio)
		SELECT TRIM(value) AS AiStudio
		FROM STRING_SPLIT(@AiStudio, ',');
	END
	DECLARE @AccountList TABLE (Account NVARCHAR(100));
	IF @Account IS NOT NULL
	BEGIN
		INSERT INTO @AccountList (Account)
		SELECT TRIM(value) AS Account
		FROM STRING_SPLIT(@Account, ',');
	END
	DROP TABLE IF EXISTS #OrderedBaseData
    ;WITH cte AS
	(
		SELECT	DD.EmployeeId, MAX(ETM.StartDate) StartDate,
				CASE WHEN COUNT(ETM.EmployeeTrainingId) > 0 THEN 1 ELSE 0 END cnt,
				MIN(ETM.TrainingStatusId) AS StatusId
		FROM DojoDetail DD
		INNER JOIN EmployeeTrainingMap ETM
			ON DD.EmployeeId = ETM.EmployeeId
		WHERE ETM.TrainingStatusId NOT IN (2, 4)
		GROUP BY DD.EmployeeId
	),
	ActivityData AS
	(
		SELECT	DD.EmployeeId, DD.DojoStartDate, DD.DojoEndDate, AM.ActivityId,
				AM.ActivityName, ISNULL(EAM.ActivityDetail, AM.ActivityName) ActivityDescription,
				CASE WHEN EAM.EmployeeActivityId IS NULL THEN 99 ELSE AM.[Priority] END [Priority],
				EAM.StartDate AS StartDate, EAM.EndDate AS EndDate, 'Activity' AS 'Type', EAM.StatusId,
				CAST(DD.IsActive AS INT) As 'IsActive', DD.DojoProjectsConfigurationId, dp.IsAssignable,
				ISNULL(EAM.Comments,'') AS 'ActivityComment',dp.ProjectName AS 'DojoProjectName'
		FROM DojoDetail DD
		INNER JOIN DojoProjectsConfiguration dp
			ON DD.DojoProjectsConfigurationId = dp.DojoProjectsConfigurationId
		LEFT JOIN EmployeeActivityMap EAM
			ON DD.EmployeeId = EAM.EmployeeId
		LEFT JOIN ActivityMaster AM
			ON EAM.ActivityId = AM.ActivityId 
		
		UNION ALL
	
		SELECT	DD.EmployeeId, DD.DojoStartDate, DD.DojoEndDate, 1 ActivityId,
				'Upskilling - Globant University Academy' AS ActivityName,
				'GU based MVP trainings' AS ActivityDescription, 1.5 AS Priority,
				NULL StartDate, NULL EndDate, 'Training' AS 'Type', cte.StatusId AS StatusId,
				MAX(CAST(DD.IsActive AS INT)) AS 'IsActive', DD.DojoProjectsConfigurationId, dp.IsAssignable,
				'' AS 'ActivityComment',dp.ProjectName AS 'DojoProjectName'
		FROM DojoDetail DD
		INNER JOIN DojoProjectsConfiguration dp
			ON DD.DojoProjectsConfigurationId = dp.DojoProjectsConfigurationId
		INNER JOIN cte
			ON DD.EmployeeId = cte.EmployeeId
		GROUP BY DD.EmployeeId, DD.DojoStartDate, DD.DojoEndDate, DD.DojoProjectsConfigurationId,
					dp.IsAssignable, cte.StatusId, dp.ProjectName
	)
	
	SELECT	D.EmployeeId, E.GlobantEmailAddress, E.Tdc as Country, E.EmployeeName, E.BaseLocation, E.Community,
			E.Seniority, E.IsActive AS EmployeeIsActive, E.[Status] AS EmployeeActiveStatus, E.AiStudio, E.Client AS Account,
			D.DojoStartDate, D.DojoEndDate, D.IsActive As 'DojoActiveStatus', D.ActivityId,
			D.ActivityName, ISNULL(D.ActivityDescription, D.ActivityName) ActivityDescription, D.[Priority] AS ActivityPriority,
			D.StartDate AS StartDate, D.EndDate AS EndDate, D.Type, D.DojoProjectsConfigurationId, D.IsAssignable,D.ActivityComment, D.DojoProjectName,
			ROW_NUMBER() OVER (PARTITION BY D.EmployeeId, D.DojoStartDate ORDER BY D.[Priority] ASC) AS OrderedPriority
	INTO #OrderedBaseData
	FROM ActivityData D
	INNER JOIN dbo.Employee E
		ON D.EmployeeId = E.Id
	ORDER BY EmployeeId
	
	--Result Set 1: Dojo Activity Report
	SELECT *
	FROM #OrderedBaseData
	WHERE IsAssignable = 1
		AND (@DojoStartDate IS NOT NULL OR (EmployeeIsActive = 1 AND DojoActiveStatus = 1))
		AND (@IsPrimary = 0 OR (OrderedPriority = 1))
		AND (@community IS NULL OR Community IN (SELECT Community FROM @CommunityList))
		AND (@Country IS NULL OR Country IN (SELECT Country FROM @CountryList))
		AND (@AiStudio IS NULL OR AiStudio IN (SELECT AiStudio FROM @AiStudioList))
		AND (@Account IS NULL OR Account IN (SELECT Account FROM @AccountList))
		AND (@DojoStartDate IS NULL OR (DojoStartDate BETWEEN CAST(@DojoStartDate AS DATETIME2) AND CAST(@DojoEndDate AS DATETIME2)))
		AND ((@SearchText IS NULL OR @SearchText = '')
			 OR EmployeeName LIKE '%' + @SearchText + '%'
			 OR GlobantEmailAddress   LIKE '%' + @SearchText + '%')
		
	--Result Set 2: Summary Count
	SELECT	COUNT(CASE WHEN ActivityName IS NOT NULL AND IsAssignable = 1 THEN EmployeeId END) AS Engaged,
			COUNT(CASE WHEN ActivityName IS NULL AND IsAssignable = 1 THEN EmployeeId END) AS NotEngaged,
			COUNT(CASE WHEN IsAssignable = 0 THEN EmployeeId END) AS NonAssignable,
			COUNT(EmployeeId) AS TotalDojo
	FROM #OrderedBaseData
	WHERE (@DojoStartDate IS NOT NULL OR (EmployeeIsActive = 1 AND DojoActiveStatus = 1))
		AND (@IsPrimary = 0 OR (OrderedPriority = 1))
		AND (@community IS NULL OR Community IN (SELECT Community FROM @CommunityList))
		AND (@Country IS NULL OR Country IN (SELECT Country FROM @CountryList))
		AND (@AiStudio IS NULL OR AiStudio IN (SELECT AiStudio FROM @AiStudioList))
		AND (@Account IS NULL OR Account IN (SELECT Account FROM @AccountList))
		AND (@DojoStartDate IS NULL OR (DojoStartDate BETWEEN CAST(@DojoStartDate AS DATETIME2) AND CAST(@DojoEndDate AS DATETIME2)))
		AND ((@SearchText IS NULL OR @SearchText = '')
			 OR EmployeeName LIKE '%' + @SearchText + '%'
			 OR GlobantEmailAddress   LIKE '%' + @SearchText + '%')
	--Result Set 3: Activity Distribution
	SELECT	ISNULL(ActivityName, 'Not Engaged') AS ActivityName,
			COUNT(CASE WHEN EmployeeIsActive = 1 THEN EmployeeId END) AS ActiveEmployeesInActivity,
			COUNT(EmployeeId) AS TotalEmployeesInActivity
	FROM #OrderedBaseData
	WHERE IsAssignable = 1
		AND (@DojoStartDate IS NOT NULL OR (EmployeeIsActive = 1 AND DojoActiveStatus = 1))
		AND (@IsPrimary = 0 OR (OrderedPriority = 1))
		AND (@community IS NULL OR Community IN (SELECT Community FROM @CommunityList))
		AND (@Country IS NULL OR Country IN (SELECT Country FROM @CountryList))
		AND (@AiStudio IS NULL OR AiStudio IN (SELECT AiStudio FROM @AiStudioList))
		AND (@Account IS NULL OR Account IN (SELECT Account FROM @AccountList))
		AND (@DojoStartDate IS NULL OR (DojoStartDate BETWEEN CAST(@DojoStartDate AS DATETIME2) AND CAST(@DojoEndDate AS DATETIME2)))
		AND ((@SearchText IS NULL OR @SearchText = '')
			 OR EmployeeName LIKE '%' + @SearchText + '%'
			 OR GlobantEmailAddress   LIKE '%' + @SearchText + '%')
	GROUP BY ActivityName
	--Result Set 4: Non Assignable Report
	SELECT *
	FROM #OrderedBaseData
	WHERE IsAssignable = 0
		AND (@DojoStartDate IS NOT NULL OR (EmployeeIsActive = 1 AND DojoActiveStatus = 1))
		AND (@IsPrimary = 0 OR (OrderedPriority = 1))
		AND (@community IS NULL OR Community IN (SELECT Community FROM @CommunityList))
		AND (@Country IS NULL OR Country IN (SELECT Country FROM @CountryList))
		AND (@AiStudio IS NULL OR AiStudio IN (SELECT AiStudio FROM @AiStudioList))
		AND (@Account IS NULL OR Account IN (SELECT Account FROM @AccountList))
		AND (@DojoStartDate IS NULL OR (DojoStartDate BETWEEN CAST(@DojoStartDate AS DATETIME2) AND CAST(@DojoEndDate AS DATETIME2)))
		AND ((@SearchText IS NULL OR @SearchText = '')
			 OR EmployeeName LIKE '%' + @SearchText + '%'
			 OR GlobantEmailAddress   LIKE '%' + @SearchText + '%')
END;
