CREATE PROCEDURE [dbo].[usp_FetchTrainingReminderDetails]
	@EmailId NVARCHAR(255)
AS
BEGIN
	DECLARE @EmployeeId INT
	DECLARE @Project NVARCHAR(200)
	 
	SELECT @EmployeeId=Id, @Project=Project 
	FROM Employee 
	WHERE GlobantEmailAddress = @EmailId
	
	IF @EmployeeId > 0
	BEGIN
		SELECT	TM.TrainingName,
			TM.TrainingUrl,
			FORMAT(ETM.ExpectedEndDate, 'dd MMMM yyyy') AS ExpectedEndDate,
			ETR.ReminderCount
		FROM EmployeeTrainingReminder ETR
		INNER JOIN EmployeeTrainingMap ETM
			ON ETR.EmployeeTrainingId = ETM.EmployeeTrainingId
		INNER JOIN TrainingMaster TM
			ON ETM.TrainingId = TM.TrainingId
		WHERE ETR.EmployeeId = @EmployeeId
			AND ETM.IsActive = 1
			AND ETM.TrainingStatusId NOT IN (2,4)
			AND ETR.IsActive = 1
			AND TM.IsPriortize = 1

		SELECT  EAM.ActivityId,
			EAM.ActivityDetail,
			FORMAT(EAM.StartDate, 'dd MMMM yyyy') AS StartDate,
			FORMAT(EAM.EndDate, 'dd MMMM yyyy') AS EndDate,
			RequestedByEmp.EmployeeName AS RequestedBy 
		FROM EmployeeActivityMap EAM
		INNER JOIN Employee AssignedEmp
			ON EAM.EmployeeId = AssignedEmp.Id  
		INNER JOIN Employee RequestedByEmp
			ON EAM.CreatedBy = RequestedByEmp.Id  
		INNER JOIN DojoProjectsConfiguration DPC
			ON AssignedEmp.Project = DPC.ProjectName  
		WHERE EAM.EmployeeId = @EmployeeId
			AND DPC.IsAssignable = 1
			AND DPC.IsActive = 1
			AND EAM.StatusId NOT IN (2,4)
			AND EAM.IsActive = 1
			AND EAM.EndDate < CAST(GETUTCDATE() AS DATE)
	END
	ELSE
	BEGIN
		RAISERROR (15600, -1, -1, 'usp_FetchTrainingReminderDetails: Employee does not exists.');
	END
END
