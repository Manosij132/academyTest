CREATE PROCEDURE [dbo].[usp_GetEmployeeDocumentDetails]
    @IsDojoOnly BIT,
    @Tdc VARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH TdcValues AS
    (
        SELECT value AS Tdc
        FROM STRING_SPLIT(@Tdc, ',')
    ),
    LatestDocPerType AS
    (
        SELECT  ed.EmployeeId, dt.DocumentType, ed.DocumentLink, ed.ReminderCount
        FROM dbo.EmployeeDocument ed
        INNER JOIN dbo.EmployeeDocumentTypeMaster dt
            ON dt.EmployeeDocumentTypeId = ed.DocumentTypeId
        WHERE ed.IsActive = 1 AND dt.IsActive = 1
          AND ed.DocumentTypeId IN (1, 2) --  1 = CV, 2 = Profile based on the master data insert script
    )

    SELECT  e.EmployeeName, e.Tdc, e.GlobantEmailAddress, e.BetterMeLeaderEmail,
            e.GexLeaders, CONVERT(DATE, e.JoiningDate) AS JoiningDate,
            e.Position AS Ecosystem,  e.Client AS GloberAccount,  dd.DojoStartDate,
            ISNULL(dd.DojoGexLeaderEmail, '') AS DojoGexLeaderEmail,
            -- CV Columns
			'=HYPERLINK("' + MAX(CASE WHEN UPPER(d.DocumentType) = 'CV' THEN d.DocumentLink END) + '", "CV Link")' AS CV_DocumentLink,
            MAX(CASE WHEN UPPER(d.DocumentType) = 'CV' THEN d.ReminderCount END) AS CV_ReminderCount,
            -- Profile
			'=HYPERLINK("' + MAX(CASE WHEN UPPER(d.DocumentType) = 'PROFILE' THEN d.DocumentLink END) + '", "Profile Link")' AS Profile_DocumentLink,
            MAX(CASE WHEN UPPER(d.DocumentType) = 'PROFILE' THEN d.ReminderCount END) AS Profile_ReminderCount,
            --In future if more document types are expected in report, add them here following the same pattern
            CASE WHEN dpc.DojoProjectsConfigurationId IS NOT NULL AND dpc.IsAssignable = 1 THEN 1
                ELSE 0 
            END AS [On DOJO]
    FROM dbo.Employee e
    LEFT JOIN dbo.DojoDetail dd
        ON e.Id = dd.EmployeeId
    LEFT JOIN dbo.DojoProjectsConfiguration dpc
        ON e.Project = dpc.ProjectName
    LEFT JOIN TdcValues t
        ON e.Tdc = t.Tdc
    LEFT JOIN LatestDocPerType d
        ON e.Id = d.EmployeeId
    WHERE e.IsActive = 1
        AND ISNULL(dd.IsActive, 1) = 1
        AND (@IsDojoOnly = 0 OR (dpc.IsAssignable = 1 AND dpc.IsActive = 1))
        AND (@Tdc IS NULL OR t.Tdc IS NOT NULL)
    GROUP BY e.EmployeeName, e.Tdc, e.GlobantEmailAddress, e.BetterMeLeaderEmail,
             e.GexLeaders, CONVERT(DATE, e.JoiningDate), e.Position, e.Client,
             dd.DojoStartDate, ISNULL(dd.DojoGexLeaderEmail, ''),
             CASE WHEN dpc.DojoProjectsConfigurationId IS NOT NULL AND dpc.IsAssignable = 1 THEN 1
                ELSE 0
             END;
END