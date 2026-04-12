CREATE PROCEDURE usp_GetActivitiesByEmployeeId
	@EmployeeId INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT  A.[EmployeeActivityId], A.[EmployeeId], A.[ActivityId], A.[StartDate], A.[EndDate], 
			A.[StatusId], A.[IsActive], A.[CreatedBy], A.[CreatedOn], A.[UpdatedBy],
			A.[UpdatedOn], B.ActivityName, A.ActivityDetail, A.Comments, A.ActivitySource, A.Account
	FROM EmployeeActivityMap A
	INNER JOIN ActivityMaster B	
		ON A.ActivityId = B.ActivityId
	WHERE A.IsActive = 1 
		AND B.IsActive = 1 
		AND A.EmployeeId = @EmployeeId ;
END