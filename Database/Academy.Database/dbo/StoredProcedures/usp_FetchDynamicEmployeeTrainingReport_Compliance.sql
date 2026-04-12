CREATE PROCEDURE [dbo].[usp_FetchDynamicEmployeeTrainingReport_Compliance]
    @SelectColumns NVARCHAR(MAX),       -- Columns to select
    @WhereClause NVARCHAR(MAX),         -- Additional filtering conditions
    @GroupByColumns NVARCHAR(MAX),      -- Columns to group by
    @TrainingStatusId INT,               -- TrainingStatusId for compliance calculation
    @ActivityType nvarchar(max)
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @ConfigureColumns NVARCHAR(MAX);
    SET @ConfigureColumns = (
        SELECT STRING_AGG(ReportColumnName, ',')
        FROM (
            SELECT ReportColumnName AS ReportColumnName
            FROM [dbo].[ReportColumnConfiguration]
            WHERE ReportColumnConfigId IN (
                SELECT CAST(value AS INT) AS Number
                FROM STRING_SPLIT(@GroupByColumns, ',')
                WHERE TRY_CAST(value AS INT) IS NOT NULL
            )
        ) AS ReportColumns
    );
	if @ActivityType=1
	begin
    SET @SQL = 'SELECT ' + @ConfigureColumns + ', ' +
               ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 1 THEN 1 ELSE 0 END) AS [Pending Count],' +
               ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 2 THEN 1 ELSE 0 END) AS [Completed Count],' +
               ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 3 THEN 1 ELSE 0 END) AS [In Progress Count],' +
               ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 4 THEN 1 ELSE 0 END) AS [Not Enrolled Count],' +
               ' COUNT(EmployeeTrainingMap.EmployeeTrainingId) AS [Grand Total], ' +
               ' CASE WHEN SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = ' + CAST(@TrainingStatusId AS NVARCHAR) + ' THEN 1 ELSE 0 END) = 0 THEN 0 ' +
               ' ELSE Round((SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = ' + CAST(@TrainingStatusId AS NVARCHAR) + ' THEN 1 ELSE 0 END) * 100.0 / COUNT(EmployeeTrainingMap.EmployeeTrainingId)),2) END AS [Compliance (%)] ' +
               ' FROM [dbo].[Employee] Employee ' +
               ' LEFT JOIN [dbo].[EmployeeTrainingMap] EmployeeTrainingMap ON Employee.Id = EmployeeTrainingMap.EmployeeId ' +
               ' LEFT JOIN [dbo].TrainingMaster TrainingMaster ON EmployeeTrainingMap.TrainingId = TrainingMaster.TrainingId ' +
			   ' LEFT JOIN [dbo].[LearningPathTrainingMap] LearningPathTrainingMap ON TrainingMaster.TrainingId = LearningPathTrainingMap.TrainingId and LearningPathTrainingMap.SeniorityId = Employee.SeniorityId' +   -- :white_check_mark: New Join
               ' LEFT JOIN [dbo].[LearningPath] LearningPath ON LearningPathTrainingMap.LearningPathId = LearningPath.LearningPathId ' +
               ' WHERE Employee.IsActive = 1 ' +
               ' AND EmployeeTrainingMap.IsActive = 1 ' +
               CASE
                   WHEN @WhereClause IS NOT NULL AND @WhereClause <> ''
                   THEN ' AND ' + @WhereClause
                   ELSE ''
               END +
               ' GROUP BY ' + @ConfigureColumns +
               ' ORDER BY ' + @ConfigureColumns;
    PRINT @SQL;
    EXEC sp_executesql @SQL;
	end
	else
	begin
	 SET @SQL =
        'SELECT DISTINCT ' + @ConfigureColumns + ', ' +  -- :white_check_mark: add a comma after dynamic columns
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 1 THEN 1 ELSE 0 END) AS [Pending Count], ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 2 THEN 1 ELSE 0 END) AS [Completed Count], ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 3 THEN 1 ELSE 0 END) AS [In Progress Count], ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 4 THEN 1 ELSE 0 END) AS [Not Enrolled Count], ' +
        'COUNT(EmployeeActivityMap.EmployeeActivityId) AS [Grand Total], ' +
        'CASE WHEN SUM(CASE WHEN EmployeeActivityMap.StatusId = ' + CAST(@TrainingStatusId AS NVARCHAR(10)) + ' THEN 1 ELSE 0 END) = 0 THEN 0 ' +
        'ELSE ROUND((SUM(CASE WHEN EmployeeActivityMap.StatusId = ' + CAST(@TrainingStatusId AS NVARCHAR(10)) + ' THEN 1 ELSE 0 END) * 100.0 / COUNT(EmployeeActivityMap.EmployeeActivityId)), 2) END AS [Compliance (%)] ' +
        'FROM [dbo].[EmployeeActivityMap] AS EmployeeActivityMap ' +
        'LEFT JOIN [dbo].[ActivityMaster] AS ActivityMaster ' +
        '    ON EmployeeActivityMap.ActivityId = ActivityMaster.ActivityId ' +
        'LEFT JOIN [dbo].[Employee] AS Employee ' +
        '    ON EmployeeActivityMap.EmployeeId = Employee.Id ' +
        'LEFT JOIN [dbo].[TrainingStatusMaster] AS TrainingStatusMaster ' +
        '    ON EmployeeActivityMap.StatusId = TrainingStatusMaster.TrainingStatusId ' +
        'WHERE Employee.IsActive = 1 ' +
        '  AND EmployeeActivityMap.IsActive = 1 ' +
        '  AND ActivityMaster.IsActive = 1 ' +
        CASE
            WHEN @WhereClause IS NOT NULL AND @WhereClause <> ''
            THEN ' AND ' + @WhereClause
            ELSE ''
        END +
        ' GROUP BY ' + @ConfigureColumns +
        ' ORDER BY ' + @ConfigureColumns;
    PRINT @SQL;
    EXEC sp_executesql @SQL;
	end
END;
GO

