CREATE PROCEDURE [dbo].[usp_GetEmployeesToSyncGUTraining]
    @Offset INT,
    @BatchSize INT,
    @Tdc VARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;  

    -- CTE to split the Tdc values into a table
    WITH TdcValues AS (
        SELECT value AS Tdc
        FROM STRING_SPLIT(@Tdc, ',')
    )
    
    SELECT DISTINCT e.GlobalId, e.Id as EmployeeId
    FROM Employee e
    LEFT JOIN TdcValues t 
        ON e.Tdc = t.Tdc
    WHERE e.IsActive = 1
        AND (@Tdc IS NULL OR t.Tdc IS NOT NULL)
    ORDER BY e.Id
    OFFSET @Offset ROWS
    FETCH NEXT @BatchSize ROWS ONLY;
END