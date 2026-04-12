CREATE VIEW [dbo].[vwDashboard] AS    
WITH pcte AS 
(
	SELECT DISTINCT 
		SE.EmployeeId
		, SE.SkillId
		, SE.CurrentProficiency
		, TP.ExpectedProficiency
	FROM SkillEndorsementMap AS SE
	JOIN Employee AS E 
		ON SE.EmployeeId = E.Id
	JOIN TrainingProficiencyMap AS TP 
		ON SE.SkillId = TP.SkillId
			AND TP.SeniorityId = E.SeniorityId
			AND TP.EcosystemId = E.EcosystemId
	WHERE SE.IsActive = 1
),
proficiency_table AS 
(
	SELECT EmployeeId,
	        (CASE WHEN ExpectedProficiency = 0 THEN 100
		          WHEN CurrentProficiency >= ExpectedProficiency THEN 100
		          ELSE CAST((1 - ((CAST(ExpectedProficiency AS DECIMAL(10, 2)) - CurrentProficiency) / CAST(ExpectedProficiency AS DECIMAL(10, 2)))) * 100  AS DECIMAL(10,2))
	         END) AS PScore
	FROM pcte
),
dashboard AS 
(    
    SELECT  E.GlobantEmailAddress,    
            CASE WHEN SUM(CASE WHEN ET.TrainingStatusId = 1 THEN 1 ELSE 0 END) = COUNT(1) THEN 'Pending'    
                 WHEN SUM(CASE WHEN ET.TrainingStatusId = 3 THEN 1 ELSE 0 END) > 0 THEN 'In Progress'    
                 WHEN SUM(CASE WHEN ET.TrainingStatusId = 2 THEN 1 ELSE 0 END) = COUNT(1) THEN 'Completed'    
                 ELSE 'Deferred'    
            END AS [Status],    
            ROUND((CAST(SUM(CASE WHEN ET.TrainingStatusId = 2 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(1)) * 100, 2) AS TrainingScore,    
            SUM(CASE WHEN ET.TrainingStatusId = 2 THEN 1 ELSE 0 END) AS C,    
            SUM(CASE WHEN ET.TrainingStatusId = 3 THEN 1 ELSE 0 END) AS O,    
            SUM(CASE WHEN ET.TrainingStatusId = 1 THEN 1 ELSE 0 END) AS P,    
            COUNT(1) AS T    
    FROM Employee AS E   
        LEFT JOIN EmployeeTrainingMap AS ET 
            ON ET.EmployeeId = E.Id     
        LEFT JOIN TrainingStatusMaster AS TS 
            ON ET.TrainingStatusId = TS.TrainingStatusId
    WHERE E.IsActive = 1
    GROUP BY E.GlobantEmailAddress    
),
avg_pcte AS
(
	SELECT EmployeeId,CAST(AVG(PScore) AS DECIMAL(10,2)) AS ProficiencyScore 
	FROM proficiency_table
	GROUP BY EmployeeId
),
employee_cv AS
(
    SELECT EmployeeId, DocumentLink, UpdatedOn
    FROM dbo.EmployeeDocument
    WHERE DocumentTypeId = 1 -- CV DocumentTypeId = 1
),
employee_profile AS
(
    SELECT EmployeeId, DocumentLink, UpdatedOn
    FROM dbo.EmployeeDocument
    WHERE DocumentTypeId = 2 -- Profile DocumentTypeId = 2
)
	
SELECT  E.Id AS EmployeeId    
        , E.EmployeeName    
        , D.GlobantEmailAddress AS EmployeeEmail    
        , E.BetterMeLeaderEmail AS CareerMentorEmail    
        , D.[Status]    
        , E.[Position]    
        , E.Client    
        , E.Project  
        , E.Seniority  
        , E.Designation  
        , E.Tdc    
        , E.[Image]    
        , E.Community    
        , D.TrainingScore    
        , ISNULL(P.ProficiencyScore, 0) AS ProficiencyScore    
        , E.IsActive    
        , E.GexLeaders
        , E.JoiningDate
        , E.WorkingEcosystem
        , PDL.ProposedDojoLeaderEmailId AS ProposedDojoGxLeader
        , CAST(CASE WHEN DPC.DojoProjectsConfigurationId IS NOT NULL AND DPC.IsAssignable = 1 THEN 1 ELSE 0 END AS BIT) AS IsProposedGxLeaderOnDojo
        ,CV.DocumentLink AS CVLink
        ,CV.UpdatedOn AS CVUpdatedOn
        ,EP.DocumentLink AS ProfileLink
        ,EP.UpdatedOn AS ProfileUpdatedOn
        ,'Engaged' AS Engaged
FROM dashboard D 
JOIN Employee E 
    ON E.GlobantEmailAddress = D.GlobantEmailAddress
LEFT JOIN avg_pcte P 
    ON E.Id = P.EmployeeId
LEFT JOIN ProposedDojoGxLeader PDL 
    ON PDL.EmployeeId  = E.Id AND ISNULL(PDL.IsActive, 1) = 1
LEFT JOIN Employee EGX
    ON EGX.GlobantEmailAddress = PDL.ProposedDojoLeaderEmailId
LEFT JOIN dbo.DojoProjectsConfiguration DPC 
    ON EGX.Project = DPC.ProjectName AND ISNULL(DPC.IsActive, 1) = 1
LEFT JOIN employee_cv CV 
    ON CV.EmployeeId = E.Id
LEFT JOIN employee_profile EP 
    ON EP.EmployeeId = E.Id;