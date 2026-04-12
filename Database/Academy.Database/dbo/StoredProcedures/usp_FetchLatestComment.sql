CREATE PROCEDURE [dbo].[usp_FetchLatestComment](@employeeId INT)
AS
BEGIN
	WITH C AS 
	(
		SELECT TOP 1 * FROM Comment 
		WHERE EmployeeId = @employeeId 
		ORDER BY CreatedOn DESC
	) 
	
	SELECT	C.CommentId
			, E.[Image] AS CommentByImage
			, C.CommentText
			, C.CreatedOn AS CommentDate
			, E.GlobantEmailAddress AS CommentBy
			, E.Id AS CommentByEmpId
	FROM C 
	JOIN Employee AS E 
		ON C.CreatedBy = E.Id;
END