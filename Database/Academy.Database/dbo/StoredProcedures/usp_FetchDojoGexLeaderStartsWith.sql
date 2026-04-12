CREATE PROCEDURE [dbo].[usp_FetchDojoGexLeaderStartsWith]
	@where VARCHAR(MAX)
AS    
BEGIN
    DECLARE @query VARCHAR(MAX) = 'SELECT Id,EmployeeName, GlobantEmailAddress, [Image], SeniorityId FROM Employee WHERE ' + @where +'';
    EXECUTE (@query); 
END