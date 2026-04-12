CREATE VIEW [dbo].[vwDojoGxLeadersReport]
AS 
    SELECT  emp.GlobantEmailAddress DOJOGlober,
            emp.Community DOJOGloberCommunity,
            proposedGxLeader.ProposedDojoLeaderEmailId GXLeaderEmail,
            emp1.Community GXLeaderCommunity
    FROM ProposedDojoGxLeader AS proposedGxLeader
    INNER JOIN Employee AS emp
        ON proposedGxLeader.EmployeeId = emp.Id
    INNER JOIN Employee AS emp1
        ON proposedGxLeader.ProposedDojoLeaderEmailId = emp1.GlobantEmailAddress;
GO
