CREATE VIEW [dbo].[vwDojoActivityReport]
AS 
SELECT E.TDC,
	E.Community,
	CONVERT(DATE, E.JoiningDate) AS JoiningDate,
	E.GlobantEmailAddress,
	E.EmployeeName,
	CONVERT(DATE, DD.DojoStartDate) AS DOJOStartDate,
	AM.ActivityName AS ActivityName,
	ISNULL(EAM.ActivityDetail, AM.ActivityName) AS ActivityDetail,
	CONVERT(DATE, EAM.StartDate) AS ActivityStartDate,
	ISNULL(CONVERT(DATE, EAM.EndDate), '') AS ActivityEndDate,
	SM.TrainingStatusName AS [Status],
	AM.[Priority],
	E.Client,
	1 OnDojo,
	E.[Status] AS GloberStatus,
	E.WorkingEcosystem AS Ecosystem,
	RE.EmployeeName AS RequestedBy,
	RE.GlobantEmailAddress AS RequesterEmail
FROM dbo.EmployeeActivityMap EAM
INNER JOIN dbo.ActivityMaster AM
	ON EAM.ActivityId = AM.ActivityId
INNER JOIN dbo.Employee E
	ON EAM.EmployeeId = E.Id
INNER JOIN dbo.Employee RE 
	ON EAM.CreatedBy = RE.Id
INNER JOIN dbo.TrainingStatusMaster SM
	ON EAM.StatusId = SM.TrainingStatusId
INNER JOIN dbo.DojoProjectsConfiguration DPC
	ON E.Project = DPC.ProjectName
LEFT JOIN dbo.DojoDetail DD
	ON E.Id = DD.EmployeeId
WHERE DPC.IsAssignable = 1
	AND DPC.IsActive = 1
	AND EAM.IsActive = 1
	AND DD.IsActive = 1