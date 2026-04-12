CREATE PROCEDURE [dbo].[usp_ProcessReminders]
    @Audience NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
	
	-- Step 0: Create table variable to hold EmployeeIds from EmployeeActivityMap where endDate is already past
    DECLARE @EmpActivity TABLE (
        EmployeeId INT PRIMARY KEY
    );

    INSERT INTO @EmpActivity (EmployeeId)
    SELECT DISTINCT eam.EmployeeId
    FROM dbo.EmployeeActivityMap eam
    INNER JOIN dbo.Employee e 
        ON eam.EmployeeId = e.Id
    LEFT JOIN dbo.DojoProjectsConfiguration dpc
        ON e.Project = dpc.ProjectName
    WHERE EndDate < CAST(GETUTCDATE() AS DATE) 
        AND e.IsActive = 1 
	    AND eam.IsActive = 1
	    AND eam.StatusId NOT IN (2,4)
        AND (@Audience = 'DOJO' AND ISNULL(dpc.IsAssignable, 0) = 1)  -- Only include mappings that ended before today
        AND ISNULL(dpc.IsActive, 1) = 1

    -- Step 1: Identify eligible Reminder entries and keep in temp table
    DECLARE @EligibleReminders TABLE (
        ReminderId INT PRIMARY KEY,
        EmployeeId INT
    );

    -- Step 1: Identify eligible Reminder entries and keep in temp table
    INSERT INTO @EligibleReminders 
	(ReminderId, EmployeeId)
    SELECT etr.EmployeeTrainingReminderId AS ReminderId, etr.EmployeeId
    FROM dbo.EmployeeTrainingReminder etr
    INNER JOIN dbo.EmployeeTrainingMap ETM
		ON ETR.EmployeeTrainingId = ETM.EmployeeTrainingId
	INNER JOIN dbo.TrainingMaster TM
		ON ETM.TrainingId = TM.TrainingId
    INNER JOIN dbo.Employee e 
        ON etr.EmployeeId = e.Id
    LEFT JOIN dbo.DojoProjectsConfiguration dpc
        ON e.Project = dpc.ProjectName
    WHERE ((@Audience = 'DOJO' AND dpc.IsAssignable = 1) OR
		  (@Audience = 'NON_DOJO' AND dpc.DojoProjectsConfigurationId IS NULL) OR
		  (@Audience NOT IN ('DOJO', 'NON_DOJO')))
        AND e.IsActive = 1
        AND ETR.IsActive = 1
        AND TM.IsPriortize = 1
	    AND ETM.TrainingStatusId NOT IN (2,4)
        AND ISNULL(dpc.IsActive, 1) = 1

    -- Step 2: Update ReminderCount (+1) for matched records
    UPDATE etr
	SET etr.ReminderCount = ISNULL(etr.ReminderCount, 0) + 1, 
        etr.UpdatedBy = 0,
        etr.UpdatedOn = GETUTCDATE()
	FROM EmployeeTrainingReminder etr
	INNER JOIN @EligibleReminders er 
        ON etr.EmployeeTrainingReminderId = er.ReminderId;

    -- Step 3: Insert one EmailDump record per matched employee

	WITH cteEmpForReminder AS (
		SELECT DISTINCT EmployeeId 
        FROM @EligibleReminders
		UNION
        SELECT DISTINCT EmployeeId FROM @EmpActivity
	)

    INSERT INTO EmailDump 
	([Subject], [Template], [To], [Cc], [Bcc],[IsActive], [CreatedBy], [CreatedOn])
    SELECT  'Academy Progress Reminder',
			'DAILY_REMINDER',
			e1.GlobantEmailAddress,
			CASE 
				WHEN e2.tdc = 'India' THEN e1.BetterMeLeaderEmail 
				ELSE '' 
			END AS Cc,
			'',
			1,                
			0,
			GETUTCDATE()
    FROM cteEmpForReminder er
    INNER JOIN dbo.Employee e1 
        ON er.EmployeeId = e1.Id
	LEFT JOIN dbo.Employee e2 
	    ON e1.BetterMeLeaderEmail = e2.GlobantEmailAddress;
END
