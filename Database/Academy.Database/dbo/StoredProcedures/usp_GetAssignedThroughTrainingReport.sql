CREATE PROCEDURE [dbo].[usp_GetAssignedThroughTrainingReport]
	@Community NVARCHAR(MAX) = NULL,
	@Country NVARCHAR(MAX) = NULL,
    @AiStudio NVARCHAR(MAX) = NULL,
	@Account NVARCHAR(MAX) = NULL,
	@DojoStartDate NVARCHAR(100) = NULL,
	@DojoEndDate NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON; -- Prevents the count of the number of rows affected from being returned.

    SELECT DD.DojoDetailId
            , DD.EmployeeId
            , E.EmployeeName
            , E.GlobantEmailAddress
            , DD.DojoStartDate
            , DD.DojoEndDate
            , DD.AssignedThroughTraining
            , DD.Comments
            , DD.TicketNumber
            , E.Community
            , E.AiStudio
            , E.Client as Account
    FROM DojoDetail DD
    INNER JOIN Employee E ON DD.EmployeeId = E.Id
    WHERE E.IsActive = 1 AND DD.IsActive = 0 AND DD.AssignedThroughTraining IS NOT NULL
		AND (@Community IS NULL 
        OR E.Community IN (
            SELECT TRIM(value) 
            FROM STRING_SPLIT(@Community, ',')))
            AND (@Country IS NULL 
        OR E.Tdc IN (
            SELECT TRIM(value) 
            FROM STRING_SPLIT(@Country, ',')))
		AND (@AiStudio IS NULL 
        OR E.AiStudio IN (
           SELECT TRIM(value) 
           FROM STRING_SPLIT(@AiStudio, ',')))
        AND (@Account IS NULL 
        OR E.Client IN (
            SELECT TRIM(value) 
            FROM STRING_SPLIT(@Account, ',')))
		AND (@DojoStartDate IS NULL OR (DD.DojoStartDate BETWEEN CAST(@DojoStartDate AS DATETIME2) AND CAST(@DojoEndDate AS DATETIME2)))
    ORDER BY DD.EmployeeId;
END;
GO
