
CREATE PROCEDURE [dbo].[usp_GetFTEEmployeeData]
AS
BEGIN
SELECT GloberId FROM [dbo].[GlowEmployeeData] WHERE EmployeeType = 'Full Time Glober';
END
