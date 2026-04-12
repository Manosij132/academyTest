CREATE PROCEDURE [dbo].[usp_FetchDynamicEmployeeTrainingReport_Summary]
	@SelectColumns NVARCHAR(MAX),       -- Columns to select
	@WhereClause NVARCHAR(MAX),         -- Additional filtering conditions
	@GroupByColumns NVARCHAR(MAX),       -- Columns to group by
	@ActivityType nvarchar(max)
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);
	Declare @ConfigureColumns NVARCHAR(MAX);
	SET @ConfigureColumns=	(SELECT STRING_AGG(ReportColumnName, ',')
							 from
							 (
								SELECT ReportColumnName as ReportColumnName
								FROM [dbo].[ReportColumnConfiguration]
								WHERE ReportColumnConfigId in ( SELECT CAST(value AS INT) AS Number
										                        FROM STRING_SPLIT(@GroupByColumns, ',')
										                        WHERE TRY_CAST(value AS INT) IS NOT NULL)
							 ) as ReportColumns);
	
    IF @ActivityType = 1
    BEGIN
        -- Construct the SQL query dynamically
        SET @SQL = 'SELECT ' + @ConfigureColumns + ',' +
                   ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 1 THEN 1 ELSE 0 END) AS PendingCount,' +
                   ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 2 THEN 1 ELSE 0 END) AS CompletedCount,' +
                   ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 3 THEN 1 ELSE 0 END) AS InProgressCount,' +
                   ' SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 4 THEN 1 ELSE 0 END) AS NotEnrolledCount,' +
                   ' COUNT(EmployeeTrainingMap.EmployeeTrainingId) AS GrandTotal, ' +
                   ' CASE WHEN SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 2 THEN 1 ELSE 0 END) = 0 THEN 0 ' +
                   ' ELSE ROUND((SUM(CASE WHEN EmployeeTrainingMap.TrainingStatusId = 2 THEN 1 ELSE 0 END) * 100.0 / COUNT(EmployeeTrainingMap.EmployeeTrainingId)),2) END AS [Compliance (%)] ' +
                   ' FROM [dbo].[Employee] Employee ' +
                   ' LEFT JOIN [dbo].[EmployeeTrainingMap] EmployeeTrainingMap ON Employee.Id = EmployeeTrainingMap.EmployeeId ' +
                   ' LEFT JOIN [dbo].TrainingMaster TrainingMaster ON EmployeeTrainingMap.TrainingId = TrainingMaster.TrainingId ' +
			       ' LEFT JOIN [dbo].[LearningPathTrainingMap] LearningPathTrainingMap ON TrainingMaster.TrainingId = LearningPathTrainingMap.TrainingId and LearningPathTrainingMap.SeniorityId = Employee.SeniorityId' +
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
        -- Print the SQL for debugging purposes (optional)
        --PRINT @SQL;
    
        -- Execute the dynamic SQL
        EXEC sp_executesql @SQL;
	END
	ELSE
    BEGIN
          -- Construct the SQL query dynamically
	         SET @SQL =
        'SELECT ' + @ConfigureColumns + ', ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 1 THEN 1 ELSE 0 END) AS [Pending Count], ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 2 THEN 1 ELSE 0 END) AS [Completed Count], ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 3 THEN 1 ELSE 0 END) AS [In Progress Count], ' +
        'SUM(CASE WHEN EmployeeActivityMap.StatusId = 4 THEN 1 ELSE 0 END) AS [Not Enrolled Count], ' +
        'COUNT(EmployeeActivityMap.EmployeeActivityId) AS [Grand Total], ' +
        'CASE WHEN SUM(CASE WHEN EmployeeActivityMap.StatusId =2 THEN 1 ELSE 0 END) = 0 THEN 0 ' +
        'ELSE ROUND((SUM(CASE WHEN EmployeeActivityMap.StatusId = 2 THEN 1 ELSE 0 END) * 100.0 / COUNT(EmployeeActivityMap.EmployeeActivityId)), 2) END AS [Compliance (%)] ' +
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
    
        -- Print the SQL for debugging purposes (optional)
        --PRINT @SQL;
    
        -- Execute the dynamic SQL
        EXEC sp_executesql @SQL;
    END
END