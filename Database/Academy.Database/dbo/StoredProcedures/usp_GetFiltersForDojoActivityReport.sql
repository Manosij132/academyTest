CREATE PROCEDURE [dbo].[usp_GetFiltersForDojoActivityReport]
AS
BEGIN
    SET NOCOUNT ON;
	SELECT DISTINCT e.Tdc AS 'Country' 
	FROM dbo.Employee as e
	INNER JOIN DojoDetail as d
		ON e.Id = d.EmployeeId

	SELECT DISTINCT e.Community AS 'Community' 
	FROM dbo.Employee as e
	INNER JOIN DojoDetail as d
		ON e.Id = d.EmployeeId

	SELECT DISTINCT e.AiStudio AS 'AiStudio' 
	FROM dbo.Employee as e
	INNER JOIN DojoDetail as d
		ON e.Id = d.EmployeeId

	SELECT DISTINCT e.AiStudio, e.Client AS 'Account' 
	FROM dbo.Employee as e
	INNER JOIN DojoDetail as d
		ON e.Id = d.EmployeeId

	
END;