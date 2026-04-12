CREATE VIEW [dbo].[vwCMRawData]
AS 
SELECT	E.BetterMeLeaderEmail, 
		E.GlobantEmailAddress, 
		E.Position AS Ecosystem, 
		(CASE WHEN DPC.DojoProjectsConfigurationId IS NOT NULL AND DPC.IsAssignable = 1 THEN 1 ELSE 0 END) AS [On DOJO], -- this is assignable dojo only. Non assignable will be 0
		TM.TrainingName AS Title, 
		TM.TrainingUrl AS TrainingLink, 
		(CASE WHEN TPM.IsMVP = 1 THEN 'MVP' ELSE 'Non-MVP' END) AS [MVP/Non-MVP], 
		(CASE WHEN TM.IsAssignment = 1 THEN 'Assignment' ELSE 'Training' END) AS TrainingType, 
		TSM.TrainingStatusName AS Status, 
		CONVERT(DATE, ETM.StartDate) AS StartDate, 
		(CASE WHEN TSM.TrainingStatusName = 'Completed' THEN CONVERT(DATE, ETM.ActualEndDate) ELSE NULL END) AS ActualEndDate, 
		CONVERT(DATE, ETM.ExpectedEndDate) AS ExpectedEndDate, 
		E.Seniority, 
		(CASE WHEN SM.SkillName = 'Cloud' AND E.Position = '.Net Developer' THEN 'Azure Cloud' ELSE SM.SkillName END) AS Skill, 
		E.Client AS GloberAccount, 
		E.EmployeeName, 
		CONVERT(DATE, DD.DojoStartDate) AS DojoStartDate,
		E.Tdc AS TDC,
		CONVERT(DATE, E.JoiningDate) JoiningDate,
		ISNULL(E.GexLeaders, '') AS GexLeaders,
		ISNULL(ETR.ReminderCount, 0) AS ReminderCount,
		ISNULL(DD.DojoGexLeaderEmail, '') AS DojoGexLeaderEmail,
		ETM.TraingAssignmentSrc AS TrainingRequestBy
FROM dbo.EmployeeTrainingMap ETM
INNER JOIN dbo.Employee E
	ON ETM.EmployeeId = E.Id  
INNER JOIN dbo.SkillMaster SM
	ON ETM.SkillId = SM.SkillId  
INNER JOIN dbo.TrainingMaster TM
	ON ETM.TrainingId = TM.TrainingId
INNER JOIN dbo.EcosystemMaster EM
	ON E.EcosystemId = EM.EcosystemId
LEFT JOIN dbo.EcosystemMaster SEM
	ON E.EcosystemId = SEM.ParentEcosystemId
LEFT JOIN dbo.TrainingProficiencyMap TPM
	ON SM.SkillId = TPM.SkillId
		AND TM.TrainingId = TPM.TrainingId
		AND E.SeniorityId = TPM.SeniorityId
		AND (SEM.EcosystemId = TPM.EcosystemId OR EM.EcosystemId = TPM.EcosystemId)
INNER JOIN dbo.TrainingStatusMaster AS TSM
	ON ETM.TrainingStatusId = TSM.TrainingStatusId
LEFT JOIN dbo.DojoDetail DD
	ON E.Id = DD.EmployeeId AND DD.IsActive = 1
LEFT JOIN dbo.EmployeeTrainingReminder ETR
	ON ETM.EmployeeTrainingId = ETR.EmployeeTrainingId
LEFT JOIN dbo.DojoProjectsConfiguration DPC
	ON E.Project = DPC.ProjectName
WHERE ETM.IsActive = 1
	AND E.IsActive = 1
	AND ISNULL(TPM.IsActive, 1) = 1
	AND ISNULL(DPC.IsActive, 1) = 1