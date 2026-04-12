CREATE PROCEDURE [dbo].[usp_FetchAreaPathReport_Detailed]
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
        e.GlobantEmailAddress,
        e.EmployeeName,
        e.Tdc,
        lp.LearningPathId,
        lp.LearningPathDescription,
        COUNT(DISTINCT lptm.TrainingId) AS TotalTrainings,
        COUNT(DISTINCT CASE WHEN letm.TrainingId IS NOT NULL THEN lptm.TrainingId END) AS EnrolledTrainings,
        COUNT(DISTINCT CASE WHEN letm.TrainingStatusId = 2 THEN lptm.TrainingId END) AS CompletedTrainings,
        COUNT(DISTINCT CASE WHEN letm.TrainingStatusId = 3 THEN lptm.TrainingId END) AS InProgressTrainings,
        COUNT(DISTINCT CASE WHEN letm.TrainingStatusId = 1 THEN lptm.TrainingId END) AS InPendingTrainings,
        COUNT(DISTINCT lptm.TrainingId)-COUNT(DISTINCT CASE WHEN letm.TrainingId IS NOT NULL THEN lptm.TrainingId END) AS NotEnrolledTrainings
        FROM dbo.Employee e
        CROSS JOIN dbo.LearningPath lp
        JOIN dbo.LearningPathTrainingMap lptm
        ON lptm.LearningPathId = lp.LearningPathId
        AND lptm.SeniorityId = e.SeniorityId
        LEFT JOIN LatestETM letm
        ON letm.EmployeeId = e.Id
        AND letm.TrainingId = lptm.TrainingId
        WHERE e.IsActive = 1
    '
    
    -- Append the WHERE clause if provided
    IF @WhereClause IS NOT NULL AND @WhereClause <> ''
    BEGIN
        SET @SQL = @SQL + ' AND ' + @WhereClause
    END

    SET @SQL = @SQL + '
        GROUP BY e.Id, e.Tdc, lp.LearningPathId, lp.LearningPathDescription, e.GlobantEmailAddress,e.EmployeeName
)
--select * from TrainingCounts


SELECT
    tc.EmployeeId,
    tc.LearningPathDescription,
    tc.GlobantEmailAddress,
    tc.EmployeeName,
    SUM(tc.TotalTrainings) As TotalTrainings,
    SUM(tc.CompletedTrainings) AS TotalCompleted,
    SUM(tc.InProgressTrainings) AS TotalInProgress,
    SUM(tc.InPendingTrainings) AS TotalPending,
    SUM(tc.NotEnrolledTrainings) AS TotalNotEnrolled,
    ROUND(
        CASE WHEN SUM(tc.TotalTrainings) = 0 THEN 0
        ELSE SUM(tc.CompletedTrainings) * 100.0 / SUM(tc.TotalTrainings)
        END, 2) AS OverallCompliancePct
FROM TrainingCounts tc
GROUP BY tc.EmployeeId, tc.LearningPathId, tc.LearningPathDescription, tc.GlobantEmailAddress, tc.EmployeeName
ORDER BY tc.LearningPathDescription;'

    PRINT @SQL;  -- For debugging purposes, you can remove this in production
    EXEC sp_executesql @SQL;
END;