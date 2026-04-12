CREATE PROCEDURE [dbo].[usp_ProcessAutoTrainingAssignment]
	@transactionId VARCHAR(20),
	@isNminusEnabled BIT
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN

		DECLARE @pending TINYINT = (SELECT TOP 1 TrainingStatusId FROM TrainingStatusMaster WHERE TrainingStatusName = 'Pending');
		
		DECLARE @skill_proficiency_table TABLE 
		(
			EmployeeId INT, 
			EcosystemId INT,
			SeniorityId INT,
			CurrentProficiency INT,
			Leaders VARCHAR(MAX), 
			EmailId VARCHAR(255),
			SkillId INT,
			TrainingId INT, 
			ExpectedProficiency INT, 
			EmpAccount VARCHAR(255)
		)
	
		INSERT INTO @skill_proficiency_table 
		(EmployeeId, EcosystemId, SeniorityId, EmpAccount, CurrentProficiency,
		 Leaders, EmailId, SkillId, TrainingId, ExpectedProficiency)
		SELECT	E.Id, E.EcosystemId, E.SeniorityId, E.Client,
				COALESCE(SE.CurrentProficiency, (TP.ExpectedProficiency - 1), 1),
				CONCAT(E.BetterMeLeaderEmail, ',', E.GexLeaders), GlobantEmailAddress,
				TP.SkillId, TP.TrainingId, TP.ExpectedProficiency
		FROM Employee AS E
		JOIN TrainingProficiencyMap AS TP 
			ON E.EcosystemId = TP.EcosystemId
				AND E.SeniorityId = TP.SeniorityId
		LEFT JOIN SkillEndorsementMap AS SE
			ON E.Id = SE.EmployeeId
				AND TP.SkillId = SE.SkillId
		WHERE E.IsActive = 1
			AND TP.IsActive = 1
			AND ISNULL(SE.IsActive, 1) = 1

		INSERT INTO EmployeeTrainingMap
		(EmployeeId, ExpectedEndDate, IsActive, SkillId, StartDate, TrainingId, 
		 TrainingStatusId, TrainingTimeAccount, TrainingTimeSeniorityId, CreatedBy, 
		 CreatedOn, EmailSent)
		SELECT	EmployeeId, DATEADD(DAY,20,GETUTCDATE()), 1, skillId, GETUTCDATE(), TrainingId, 
				@pending, EmpAccount, SeniorityId, 0, GETUTCDATE(), 0
		FROM @skill_proficiency_table
		WHERE CurrentProficiency < ExpectedProficiency

		INSERT INTO EmailDump 
		(Cc, CreatedBy, CreatedOn, IsActive, [Subject], Template, [To])
		SELECT DISTINCT Leaders, 0, GETUTCDATE(), 1, 'Action Required: Mandatory Training Assigned To You', 'GU_USER_ADDED', EmailId
		FROM @skill_proficiency_table

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN;
		THROW
	END CATCH
END