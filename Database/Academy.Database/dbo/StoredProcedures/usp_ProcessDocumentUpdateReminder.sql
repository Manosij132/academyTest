CREATE PROCEDURE [dbo].[usp_ProcessDocumentUpdateReminder]
	@NewEmailSubject VARCHAR(100),
	@UpdateEmailSubject VARCHAR(100),
	@IsDojoOnly BIT,
	@Tdc VARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- CTE to split the Tdc values
    WITH TdcValues AS (
        SELECT value AS Tdc
        FROM STRING_SPLIT(@Tdc, ',')
    )

    -- Insert active employees into the EmployeeDocument table
    INSERT INTO dbo.EmployeeDocument 
    (EmployeeId, DocumentTypeId, IsUpdateRequired, ReminderCount, IsActive, CreatedBy, CreatedOn)
    SELECT  e.Id, edtm.EmployeeDocumentTypeId, 0, 0, 1, 0, GETUTCDATE()
    FROM dbo.Employee e
    CROSS JOIN dbo.EmployeeDocumentTypeMaster edtm
    LEFT JOIN TdcValues t ON e.Tdc = t.Tdc -- Left join with TdcValues
    WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.EmployeeDocument ed
            WHERE ed.EmployeeId = e.Id
              AND ed.DocumentTypeId = edtm.EmployeeDocumentTypeId
        
        )
        AND e.IsActive = 1
	    AND e.Id > 0
        AND ISNULL(edtm.IsActive, 1) = 1
        AND (@Tdc IS NULL OR t.Tdc IS NOT NULL); -- Include Tdc condition

    -- Update IsUpdateRequired to 1 for documents that are stale or not uploaded based on @IsDojoOnly
    UPDATE ed
    SET IsUpdateRequired = 1,
        UpdatedBy = 0,
        UpdatedOn = GETUTCDATE()
    FROM dbo.EmployeeDocument ed WITH (ROWLOCK)
    LEFT JOIN dbo.DojoDetail dd ON ed.EmployeeId = dd.EmployeeId
    LEFT JOIN dbo.DojoProjectsConfiguration dpc ON dd.DojoProjectsConfigurationId = dpc.DojoProjectsConfigurationId
    WHERE (@IsDojoOnly = 0 OR dd.DojoDetailId IS NOT NULL)
        AND ISNULL(dd.IsActive, 1) = 1
        AND ISNULL(dpc.IsAssignable, 1) = 1
        AND ed.IsUpdateRequired = 0
        AND ISNULL(ed.DocumentLink, '') = '';

    DROP TABLE IF EXISTS #EmployeeDocumentsToRemind;

    --Add entry in dbo.EmailDump for each employee whose document needs Update.
    SELECT  e.Id, e.GlobantEmailAddress, e.BetterMeLeaderEmail, dd.DojoGexLeaderEmail, 
            e.GexLeaders, ed.ReminderCount,
            ROW_NUMBER() OVER (PARTITION BY ed.EmployeeId ORDER BY ed.ReminderCount DESC) AS RowNum
    INTO #EmployeeDocumentsToRemind
    FROM dbo.Employee e
    INNER JOIN dbo.EmployeeDocument ed 
        ON e.Id = ed.EmployeeId
    INNER JOIN dbo.EmployeeDocumentTypeMaster edtm 
        ON ed.DocumentTypeId = edtm.EmployeeDocumentTypeId
    LEFT JOIN dbo.DojoDetail dd 
        ON e.Id = dd.EmployeeId AND dd.IsActive = 1
    WHERE ed.IsUpdateRequired = 1
        AND edtm.IsEligibleForReminder = 1
        AND e.IsActive = 1
    
    -- Update the EmployeeDocument table for the employees whose emails were sent
    UPDATE ed
    SET ed.ReminderCount = ed.ReminderCount + 1, -- Increment ReminderCount by 1
        ed.LastReminderSentOn = GETUTCDATE(), -- Set LastReminderSent to current UTC date
        ed.UpdatedBy = 0,
        ed.UpdatedOn = GETUTCDATE()
    FROM dbo.EmployeeDocument ed
    INNER JOIN #EmployeeDocumentsToRemind edtr 
        ON ed.EmployeeId = edtr.Id
	LEFT JOIN dbo.DojoDetail dd -- Join with DojoDetail to check if the employee is active in Dojo
		ON ed.EmployeeId = dd.EmployeeId 
    WHERE ed.IsActive = 1 -- Ensure that only active records are updated
		AND dd.IsActive = 1
		AND ed.IsUpdateRequired = 1
        AND (ISNULL(@IsDojoOnly, 0) = 0 OR (dd.EmployeeId IS NOT NULL AND @IsDojoOnly = 1)); -- Filter based on @IsDojoOnly

    INSERT INTO dbo.EmailDump
	([To], Cc, [Subject], Template, PlainText, IsActive, CreatedBy, CreatedOn)
    SELECT  GlobantEmailAddress, 
            STRING_AGG(TRIM(email), ',') AS Cc,
            CASE WHEN ReminderCount = 0 THEN @NewEmailSubject ELSE @UpdateEmailSubject END AS [Subject],
            CASE WHEN ReminderCount = 0 THEN 'GLOBER_DOCUMENT_UPLOAD_REMINDER' ELSE 'GLOBER_DOCUMENT_UPDATE_REMINDER' END AS Template,
            CAST(ReminderCount AS VARCHAR) AS PlainText,
            1, 0, GETUTCDATE()
    FROM #EmployeeDocumentsToRemind
	CROSS APPLY (
		VALUES 
			(BetterMeLeaderEmail),
			(DojoGexLeaderEmail),
			(GexLeaders)
	) AS email(email)
    WHERE RowNum = 1
	GROUP BY GlobantEmailAddress, ReminderCount;

    DROP TABLE IF EXISTS #EmployeeDocumentsToRemind;
END;