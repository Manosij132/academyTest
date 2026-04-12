CREATE PROCEDURE [dbo].[usp_FetchDashboardTrainings]  
	@employeeId INT  
AS  
BEGIN  
	SELECT s.SkillId,  
		   ISNULL (etm.progress, 0) AS TrainingScore,
		   t.TrainingId,  
		   etm.StartDate AS ActualEndDate,  
		   etm.StartDate,  
		   etm.ExpectedEndDate,  
		   etm.TrainingStatusId,  
		   TM.TrainingStatusName AS TrainingStatus,  
		   etm.EmployeeTrainingId AS EmployeeTrainingMapId,  
		   s.SkillName,  
		   t.TrainingName,  
		   t.TrainingUrl,  
		   ISNULL(tp.IsMVP, 0) AS IsMvp 
	FROM dbo.EmployeeTrainingMap etm  
	INNER JOIN Employee e  
		ON etm.EmployeeId = e.Id  
	INNER JOIN SkillMaster s   
		ON etm.SkillId = s.SkillId  
	INNER JOIN TrainingMaster t   
		ON etm.TrainingId = t.TrainingId
	INNER JOIN EcosystemMaster em
		ON e.EcosystemId = em.EcosystemId
	LEFT JOIN EcosystemMaster sem
		ON e.EcosystemId = sem.ParentEcosystemId
	LEFT JOIN TrainingProficiencyMap tp   
		ON s.SkillId = tp.SkillId   
			AND t.TrainingId = tp.TrainingId   
			AND e.SeniorityId = tp.SeniorityId 
			AND (sem.EcosystemId = tp.EcosystemId OR em.EcosystemId = tp.EcosystemId) 
	INNER JOIN TrainingStatusMaster AS TM 
		ON etm.TrainingStatusId = TM.TrainingStatusId
	WHERE e.Id = @employeeId
END