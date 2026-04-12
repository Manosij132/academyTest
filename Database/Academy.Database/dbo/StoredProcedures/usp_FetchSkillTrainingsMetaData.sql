CREATE PROCEDURE [dbo].[usp_FetchSkillTrainingsMetaData]
	@ecosystemId INT
AS
BEGIN
	DECLARE @secondaryEcosystems TABLE (EcosystemId INT);
	
	INSERT INTO @secondaryEcosystems 
	(EcosystemId)
	SELECT EcosystemId
	FROM EcosystemMaster
	WHERE ParentEcosystemId = @ecosystemId
		AND IsActive = 1;

	--TODO: AK: Include secondary ecosystem in query
	SELECT	TE.EcosystemId
			, TE.SeniorityId
			, TE.SkillId
			, S.SkillName
			, TE.TrainingId
			, T.TrainingName
			, T.TrainingUrl AS TrainingLink
			, ISNULL(TE.ExpectedProficiency, 1) AS ExpectedProficiency
			, ISNULL(TE.ExpectedKnowledge, 1) AS ExpectedKnowledge
			, TE.IsMVP AS IsMvp
	FROM TrainingProficiencyMap AS TE
	JOIN SkillMaster AS S 
		ON TE.SkillId = S.SkillId 
			AND S.IsActive = 1
	JOIN TrainingMaster AS T 
		ON TE.TrainingId = T.TrainingId 
			AND T.IsActive = 1
	WHERE TE.EcosystemId = @ecosystemId 
		AND TE.IsActive = 1
END