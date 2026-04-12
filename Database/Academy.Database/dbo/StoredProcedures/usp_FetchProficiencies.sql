CREATE PROCEDURE [dbo].[usp_FetchProficiencies]
	@employeeId INT
AS
BEGIN
	DECLARE @employeeEcosystemId INT;
	DECLARE @snrId INT;

	SELECT TOP 1 @employeeEcosystemId = EcosystemId,
			@snrId = SeniorityId
	FROM Employee 
	WHERE Id = @employeeId;

	SELECT	SE.SkillEndorsementId,
			@employeeId AS EmployeeId,
			@snrId AS SeniorityId,
			A.EcosystemId,
			A.SkillId,
			S.SkillName,
			IsMVP,
			ExpectedProficiency,
			ExpectedKnowledge,
			ISNULL(SE.CurrentKnowledge, 1) AS CurrentKnowledge,
			ISNULL(SE.CurrentProficiency, 1) AS CurrentProficiency
	FROM
	(
		SELECT	@employeeEcosystemId AS EcosystemId,
				TP.SkillId,
				TP.IsMVP,
				TP.ExpectedProficiency,
				ISNULL(TP.ExpectedKnowledge, 1) AS ExpectedKnowledge
		FROM TrainingProficiencyMap AS TP
		WHERE TP.EcosystemId = @employeeEcosystemId 
			AND TP.SeniorityId = @snrId
			
		UNION

		SELECT	sem.EcosystemId AS EcosystemId,
				TP.SkillId,
				TP.IsMVP,
				TP.ExpectedProficiency,
				ISNULL(TP.ExpectedKnowledge, 1) AS ExpectedKnowledge
		FROM TrainingProficiencyMap AS TP
		LEFT JOIN EcosystemMaster sem
			ON TP.EcosystemId = sem.EcosystemId
		WHERE TP.SeniorityId = @snrId
			AND sem.IsPrimary = 0
			AND sem.ParentEcosystemId = @employeeEcosystemId
	) A
	LEFT JOIN SkillEndorsementMap AS SE 
		ON A.SkillId = SE.SkillId 
	INNER JOIN SkillMaster AS S 
		ON SE.SkillId = S.SkillId 
			AND S.IsActive = 1
	WHERE SE.IsActive = 1
		AND SE.EmployeeId = @employeeId
END