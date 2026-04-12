CREATE VIEW [dbo].[vwProficiencyTable]
AS
SELECT	E.GlobantEmailAddress, 
		SM.SkillName AS Skill, 
		TM.TrainingName AS Training, 
		E.Position AS Ecosystem,
		ISNULL(SE.CurrentProficiency, 1) AS GloberProf, 
		ISNULL(TPM.ExpectedProficiency, 1) AS ExpectedProf,
		TM.TrainingId, 
		(CASE WHEN SE.CurrentProficiency < TPM.ExpectedProficiency THEN CAST(CAST(ETM.StartDate AS DATE) AS VARCHAR)      
			  ELSE 'Not Required' END
		) AS StartDate,
		ISNULL(SE.UpdatedOn, SE.CreatedOn) AS LastEndorsementDate,
		(CASE WHEN ETM.TrainingStatusId = 2 THEN 100 
			  ELSE CASE WHEN SE.CurrentProficiency < TPM.ExpectedProficiency 
							THEN ROUND((CAST(SE.CurrentProficiency AS FLOAT)/CAST(TPM.ExpectedProficiency AS FLOAT)) * 100, 2)          
						ELSE 100                         
					END
		 END) AS [%Completed]
FROM dbo.EmployeeTrainingMap ETM
INNER JOIN Employee E
	ON ETM.EmployeeId = E.Id
INNER JOIN SkillMaster SM
	ON ETM.SkillId = SM.SkillId  
INNER JOIN TrainingMaster TM  
	ON ETM.TrainingId = TM.TrainingId
INNER JOIN EcosystemMaster EM
	ON E.EcosystemId = EM.EcosystemId
LEFT JOIN EcosystemMaster SEM
	ON E.EcosystemId = SEM.ParentEcosystemId
LEFT JOIN TrainingProficiencyMap TPM   
	ON SM.SkillId = TPM.SkillId   
		AND TM.TrainingId = TPM.TrainingId   
		AND E.SeniorityId = TPM.SeniorityId 
		AND (SEM.EcosystemId = TPM.EcosystemId OR EM.EcosystemId = TPM.EcosystemId) 
INNER JOIN TrainingStatusMaster AS TSM 
	ON ETM.TrainingStatusId = TSM.TrainingStatusId
LEFT JOIN SkillEndorsementMap SE
	ON E.Id = SE.EmployeeId AND SM.SkillId = SE.SkillId
WHERE E.IsActive = 1 
	AND ETM.IsActive = 1 
	AND ISNULL(SE.IsActive, 1) = 1
GO