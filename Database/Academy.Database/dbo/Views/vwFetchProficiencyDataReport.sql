CREATE VIEW [dbo].[vwFetchProficiencyDataReport]
AS
WITH completed_cte AS 
(
	SELECT ETM.EmployeeId, COUNT(ETM.EmployeeTrainingId) AS TrainingsCompleted
	FROM dbo.EmployeeTrainingMap AS ETM
	GROUP BY ETM.EmployeeId, ETM.TrainingStatusId
	HAVING ETM.TrainingStatusId = 2
), proficicency_cte AS
(
	SELECT GlobantEmailAddress, ROUND(AVG([%Completed]), 0) AS ProficiencyLag
    FROM dbo.vwProficiencyTable
    GROUP BY GlobantEmailAddress
), academy_reminder_cte AS
(
	SELECT R.EmployeeId, MAX(R.ReminderCount) AS ReminderCount
    FROM dbo.EmployeeTrainingReminder AS R
	WHERE R.IsActive = 1
    GROUP BY R.EmployeeId
), consolidatedSkill_cte AS
(
	SELECT ES.EmployeeId, STRING_AGG(SkillName, ',') AS Skills
    FROM 
	(
		SELECT DISTINCT ETM.EmployeeId, SM.SkillName
		FROM EmployeeTrainingMap ETM
		INNER JOIN SkillMaster SM
		ON ETM.SkillId = SM.SkillId
        WHERE SM.IsSkillRequiredInReport = 1
	) AS ES
	GROUP BY EmployeeId
), total_cte AS
(
	SELECT	EmployeeId, COUNT(EmployeeTrainingId) AS TrainingsAssigned, 
			MAX(StartDate) AS TrainingAssignedDate, MAX(ExpectedEndDate) AS ExpectedTrainingEndDate
    FROM dbo.EmployeeTrainingMap
    GROUP BY EmployeeId
)
    
SELECT	e.BetterMeLeaderEmail, e.GlobantEmailAddress,e.Community,e.Project, ISNULL(tc.TrainingsAssigned, 0) AS TrainingsAssigned, 
		ISNULL(cc.TrainingsCompleted, 0) AS TrainingsCompleted, 
        ISNULL(ISNULL(cc.TrainingsCompleted, 0) * 100 / tc.TrainingsAssigned, 0) AS [% Completed], 
		ISNULL(pc.ProficiencyLag, 100) AS ProficiencyLag, e.Position AS Ecosystem, 
        (CASE WHEN dpc.DojoProjectsConfigurationId IS NOT NULL AND dpc.IsAssignable = 1 THEN 1 ELSE 0 END) AS [On DOJO], -- this is assignable dojo only. Non assignable will be 0
		ISNULL(r.ReminderCount, 0) AS ReminderCount, 
		CONVERT(DATE, e.JoiningDate) AS JoiningDate, e.Client AS GloberAccount, 
		csk.Skills AS ConsolidatedSkills, e.EmployeeName, e.Status AS TDC, tc.TrainingAssignedDate, 
		tc.ExpectedTrainingEndDate, DD.DojoStartDate, ISNULL(E.GexLeaders, '') AS GexLeaders,
		ISNULL(DD.DojoGexLeaderEmail, '') AS DojoGexLeaderEmail
FROM dbo.Employee AS e
LEFT JOIN total_cte AS tc ON tc.EmployeeId = e.Id
LEFT JOIN completed_cte AS cc ON tc.EmployeeId = cc.EmployeeId
LEFT JOIN proficicency_cte AS pc ON e.GlobantEmailAddress = pc.GlobantEmailAddress
LEFT JOIN academy_reminder_cte AS r ON e.Id = r.EmployeeId
LEFT JOIN consolidatedSkill_cte AS csk ON e.Id = csk.EmployeeId
LEFT JOIN dbo.DojoDetail AS DD ON e.Id = DD.EmployeeId
LEFT JOIN dbo.DojoProjectsConfiguration AS dpc ON e.Project = DPC.ProjectName
WHERE e.IsActive = 1
	AND ISNULL(dpc.IsActive, 1) = 1