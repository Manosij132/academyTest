CREATE PROCEDURE [dbo].[usp_FetchAreaPathReport_Summary]
    @WhereClause NVARCHAR(MAX) = NULL  -- Optional parameter for additional filtering
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = '
    WITH LatestETM AS (
        SELECT etm.EmployeeId, etm.TrainingId, etm.TrainingStatusId
        FROM dbo.EmployeeTrainingMap etm
        WHERE etm.IsActive = 1
        GROUP BY etm.EmployeeId, etm.TrainingId, etm.TrainingStatusId
    ),
    TrainingCounts AS (
        SELECT
            e.Id AS EmployeeId,
            e.Tdc,
            lp.LearningPathId,
            lp.LearningPathDescription,
            COUNT(DISTINCT lptm.TrainingId) AS TotalTrainings,
            COUNT(DISTINCT CASE WHEN letm.TrainingId IS NOT NULL THEN lptm.TrainingId END) AS EnrolledTrainings,
            COUNT(DISTINCT CASE WHEN letm.TrainingStatusId = 2 THEN lptm.TrainingId END) AS CompletedTrainings,
            COUNT(DISTINCT CASE WHEN letm.TrainingStatusId = 3 THEN lptm.TrainingId END) AS InProgressTrainings,
            COUNT(DISTINCT CASE WHEN letm.TrainingStatusId = 1 THEN lptm.TrainingId END) AS InPendingTrainings,
            COUNT(DISTINCT lptm.TrainingId) - COUNT(DISTINCT CASE WHEN letm.TrainingId IS NOT NULL THEN lptm.TrainingId END) AS NotEnrolledTrainings
        FROM dbo.Employee e
        CROSS JOIN dbo.LearningPath lp
        JOIN dbo.LearningPathTrainingMap lptm
            ON lptm.LearningPathId = lp.LearningPathId
            AND lptm.SeniorityId = e.SeniorityId
        LEFT JOIN LatestETM letm
            ON letm.EmployeeId = e.Id
            AND letm.TrainingId = lptm.TrainingId
        WHERE e.IsActive = 1 '

    -- Append the WHERE clause if provided
    IF @WhereClause IS NOT NULL AND @WhereClause <> ''
    BEGIN
        SET @SQL = @SQL + ' AND ' + @WhereClause
    END

    SET @SQL = @SQL + '
        GROUP BY e.Id, e.Tdc, lp.LearningPathId, lp.LearningPathDescription
    )
    SELECT
        tc.LearningPathDescription,
        COUNT(DISTINCT tc.EmployeeId) AS EmployeeCount,
        SUM(CASE WHEN tc.TotalTrainings > 0 AND tc.CompletedTrainings >= tc.TotalTrainings THEN 1 ELSE 0 END) AS CompletedCount,
        SUM(CASE WHEN tc.TotalTrainings > 0
            AND tc.EnrolledTrainings > 0
            AND (tc.InProgressTrainings > 0 OR (tc.CompletedTrainings > 0 AND tc.CompletedTrainings < tc.TotalTrainings) OR tc.InPendingTrainings > 0)
            THEN 1 ELSE 0 END) AS InProgressCount,
        SUM(CASE WHEN tc.TotalTrainings > 0 AND tc.TotalTrainings = tc.InPendingTrainings THEN 1 ELSE 0 END) AS PendingCount,
        SUM(CASE WHEN tc.EnrolledTrainings = 0 THEN 1 ELSE 0 END) AS NotEnrolledCount,
        ROUND(CASE WHEN SUM(tc.TotalTrainings) = 0 THEN 0
            ELSE SUM(tc.CompletedTrainings) * 100.0 / SUM(DISTINCT tc.EmployeeId)
        END, 2) AS OverallCompliancePct
    FROM TrainingCounts tc
    GROUP BY tc.LearningPathId, tc.LearningPathDescription
    ORDER BY tc.LearningPathDescription;'

    PRINT @SQL;  -- For debugging purposes, you can remove this in production
    EXEC sp_executesql @SQL;
END;