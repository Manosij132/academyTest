CREATE VIEW [dbo].[vwAssignedTrainingReport]
AS
    SELECT
        emp.EmployeeName AS Glober,
        emp.GlobantEmailAddress AS Email,
        emp.Community,
        CONVERT(DATE, dojo.DojoStartDate) AS StartDate,
        CONVERT(DATE, dojo.DojoEndDate) AS EndDate,
        dojo.AssignedThroughTraining,
        dojo.Comments,
        dojo.TicketNumber AS Ticket
    FROM
        dbo.DojoDetail dojo
        INNER JOIN dbo.Employee emp 
    	ON dojo.EmployeeId = emp.Id
    WHERE
        emp.IsActive = 1
        AND dojo.IsActive = 0
GO