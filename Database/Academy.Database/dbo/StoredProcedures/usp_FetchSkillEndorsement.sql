CREATE PROCEDURE [dbo].[usp_FetchSkillEndorsement]
(@employeeId XML)
AS
BEGIN
	SELECT	SE.EmployeeId
			, E.SeniorityId
			, SE.SkillId
			, ISNULL(SE.CurrentProficiency,1) AS CurrentProficiency
			, ISNULL(SE.CurrentKnowledge,1) AS CurrentKnowledge
	FROM SkillEndorsementMap AS SE
	JOIN Employee AS E 
		ON SE.EmployeeId = E.Id AND E.IsActive = 1
	WHERE SE.EmployeeId IN (SELECT T.c.value('.', 'INT') AS UserID FROM  @employeeId.nodes('/root/user') AS T(c)) 
		AND SE.IsActive = 1;
END