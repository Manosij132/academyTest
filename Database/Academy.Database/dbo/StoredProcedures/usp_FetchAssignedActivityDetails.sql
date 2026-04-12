CREATE PROCEDURE [dbo].[usp_FetchAssignedActivityDetails]
	@EmailId NVARCHAR(255), --TODO: Remove emaiId paramater as EmployeeActivityId is sufficient.
	@ActivityId INT --TODO: Rename to EmployeeActivityId for better clarity
AS
BEGIN
	DECLARE @EmployeeId INT = (SELECT Id FROM Employee WHERE GlobantEmailAddress = @EmailId)
	IF @EmployeeId > 0
	BEGIN
		SELECT  AM.ActivityName,
				EAM.ActivityDetail,
				FORMAT(EAM.StartDate, 'dd MMMM yyyy') AS StartDate,
				FORMAT(EAM.EndDate, 'dd MMMM yyyy') AS EndDate,
				E.EmployeeName AS AssignedBy
		FROM EmployeeActivityMap EAM
		INNER JOIN ActivityMaster AM
			ON EAM.ActivityId = AM.ActivityId
		INNER JOIN Employee AS E
			ON EAM.CreatedBy = E.Id
		WHERE EAM.EmployeeId = @EmployeeId AND  EAM.EmployeeActivityId = @ActivityId
	END
	ELSE
	BEGIN
		RAISERROR (15600, -1, -1, 'usp_FetchAssignedActivityDetails: Employee does not exists.');
	END
END