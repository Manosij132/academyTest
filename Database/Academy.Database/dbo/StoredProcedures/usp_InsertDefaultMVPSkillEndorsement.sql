CREATE PROCEDURE [dbo].[usp_InsertDefaultMVPSkillEndorsement]
	@isNminus BIT,
	@employeeId INT = NULL
AS
BEGIN
	IF @employeeId IS NOT NULL
	BEGIN
		DECLARE @ecosystemId INT
		DECLARE @seniorityId INT

		SELECT @ecosystemId = EcosystemId, @seniorityId = SeniorityId
		FROM Employee
		WHERE Id = @employeeId

		INSERT INTO dbo.SkillEndorsementMap
		(EmployeeId, SkillId, CurrentProficiency, CurrentKnowledge, IsActive, CreatedBy, CreatedOn)
		SELECT DISTINCT @employeeId, TPM.SkillId, 
				CASE WHEN @isNminus = 1 AND ISNULL(ExpectedProficiency, 0) > 1 THEN ExpectedProficiency - 1 
					 ELSE ExpectedProficiency 
				END AS CurrentProficiency, 
				CASE WHEN @isNminus = 1 AND ISNULL(ExpectedKnowledge, 0) > 1 THEN ExpectedKnowledge - 1 
					 ELSE CurrentKnowledge 
				END AS CurrentKnowledge,
				1, 0, GETUTCDATE()
		FROM dbo.TrainingProficiencyMap TPM
		LEFT JOIN dbo.SkillEndorsementMap SE
			ON TPM.SkillId = SE.SkillId 
				AND SE.EmployeeId = @employeeId 
		WHERE TPM.EcosystemId = @ecosystemId
			AND TPM.SeniorityId = @seniorityId
			AND IsMVP = 1
			AND SE.SkillEndorsementId IS NULL
	END
	ELSE
	BEGIN
		INSERT INTO dbo.SkillEndorsementMap
		(EmployeeId, SkillId, CurrentProficiency, CurrentKnowledge, IsActive, CreatedBy, CreatedOn)
		SELECT DISTINCT E.Id, TPM.SkillId, 
				CASE WHEN @isNminus = 1 AND ISNULL(ExpectedProficiency, 0) > 1 THEN ExpectedProficiency - 1 
					 ELSE ExpectedProficiency 
				END AS CurrentProficiency, 
				CASE WHEN @isNminus = 1 AND ISNULL(ExpectedKnowledge, 0) > 1 THEN ExpectedKnowledge - 1 
					 ELSE CurrentKnowledge 
				END AS CurrentKnowledge,
				1, 0, GETUTCDATE()
		FROM Employee E
		INNER JOIN dbo.TrainingProficiencyMap TPM
			ON E.EcosystemId = TPM.EcosystemId
				AND E.SeniorityId = TPM.SeniorityId
		LEFT JOIN dbo.SkillEndorsementMap SE
			ON TPM.SkillId = SE.SkillId 
				AND SE.EmployeeId = E.Id 
		WHERE TPM.EcosystemId = E.EcosystemId
			AND TPM.SeniorityId = E.SeniorityId
			AND IsMVP = 1
			AND SE.SkillEndorsementId IS NULL
			AND E.IsActive = 1
	END
END