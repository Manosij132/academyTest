CREATE PROCEDURE [dbo].[usp_UpdateEmployeeTrainingStatus]
    @TrainingDetails dbo.udt_EmployeeTrainingDetail READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SkillId INT;

    -- Step 1: Retrieve the SkillId for 'Not Available'
    SELECT TOP 1 @SkillId = SkillId
    FROM dbo.SkillMaster
    WHERE SkillName = 'Not Available';

    IF @SkillId IS NULL
    BEGIN
        RAISERROR('SkillId for Not Available not found in SkillMaster.', 16, 1);
        RETURN;
    END

    -- Step 2: Create a temporary table to hold distinct TrainingIds and TrainingNames
    CREATE TABLE #DistinctTrainings (
        TrainingId INT,
        TrainingName NVARCHAR(255)
    );

    -- Step 3: Insert distinct TrainingId and TrainingName from the user-defined table type
    INSERT INTO #DistinctTrainings (TrainingId, TrainingName)
    SELECT DISTINCT td.TrainingId, td.TrainingName
    FROM @TrainingDetails AS td
    WHERE td.TrainingId IS NOT NULL; -- Ensure valid TrainingId

    -- Step 4: Use MERGE to insert new trainings or update existing ones
    MERGE INTO dbo.TrainingMaster AS tm
    USING 
    (
        SELECT  dt.TrainingId, dt.TrainingName, dt.TrainingName AS TrainingDescription,
                CONCAT('https://university.globant.com/group/', dt.TrainingId) AS TrainingUrl,
                0 AS TrainingCompletionHours, 1 AS IsActive
        FROM #DistinctTrainings AS dt
    ) AS src
    ON tm.TrainingId = src.TrainingId
    WHEN MATCHED AND tm.TrainingName <> src.TrainingName THEN
        UPDATE 
        SET tm.TrainingName = src.TrainingName,
            tm.UpdatedBy = 0,
            tm.UpdatedOn = GETUTCDATE()
    WHEN NOT MATCHED THEN
        INSERT 
        (TrainingId, TrainingName, TrainingUrl, TrainingCompletionHours, IsActive, CreatedBy, CreatedOn)
        VALUES 
        (src.TrainingId, src.TrainingName, src.TrainingUrl, src.TrainingCompletionHours,
         src.IsActive, 0, GETUTCDATE());

    -- Step 5: Use MERGE to update or insert records in EmployeeTrainingMap
    DROP TABLE IF EXISTS #SrcEmployeeTraining;

    SELECT  td.EmployeeId, td.TrainingId, td.ActualEndDate, td.TrainingName,
            td.[Status], td.StartDate, td.Progress, e.SeniorityId  AS TrainingTimeSeniorityId,
            CASE WHEN tsm.TrainingStatusId IS NOT NULL THEN tsm.TrainingStatusId ELSE 1 END AS TrainingStatusId
    INTO #SrcEmployeeTraining
    FROM @TrainingDetails AS td
    INNER JOIN dbo.Employee e
        ON td.EmployeeId = e.Id
    LEFT JOIN dbo.TrainingStatusMaster tsm
        ON LOWER(ISNULL(td.[Status], '')) = LOWER(TrainingStatusName)

    -- Update existing records in EmployeeTrainingMap as per source
    UPDATE etm
    SET etm.TrainingStatusId = src.TrainingStatusId,
        etm.ActualEndDate = src.ActualEndDate,
		etm.Progress = src.Progress,
        etm.UpdatedOn = GETUTCDATE(),
        etm.UpdatedBy = 0
    FROM dbo.EmployeeTrainingMap etm
    INNER JOIN #SrcEmployeeTraining src
        ON etm.EmployeeId = src.EmployeeId AND etm.TrainingId = src.TrainingId

    -- insert entries in EmployeeTrainingMap which are not present in EmployeeTrainingMap but present in source
    INSERT INTO dbo.EmployeeTrainingMap
    (
        EmployeeId, SkillId, TrainingId, TrainingStatusId, StartDate, ExpectedEndDate, 
        ActualEndDate, EmailSent, TrainingTimeSeniorityId, Progress, IsActive, CreatedBy, CreatedOn
    )
    SELECT  src.EmployeeId, @SkillId, src.TrainingId, src.TrainingStatusId, src.StartDate, DATEADD(DAY, 21, src.StartDate),  -- 3 weeks added to StartDate
            src.ActualEndDate, 1, src.TrainingTimeSeniorityId, src.Progress, 1, 0, GETUTCDATE()
    FROM #SrcEmployeeTraining AS src
    LEFT JOIN dbo.EmployeeTrainingMap etm
        ON src.EmployeeId = etm.EmployeeId AND src.TrainingId = etm.TrainingId
    WHERE etm.EmployeeId IS NULL AND src.TrainingId IS NOT NULL

    -- Delete records from EmployeeTrainingMap which are not present in source but present in EmployeeTrainingMap for the employees present in source
    -- This will handle the scenario when user unsubscribes a training in GU.
    DELETE etm
    FROM dbo.EmployeeTrainingMap etm
    INNER JOIN #SrcEmployeeTraining srcEmp
        ON etm.EmployeeId = srcEmp.EmployeeId
    LEFT JOIN #SrcEmployeeTraining src
        ON etm.EmployeeId = src.EmployeeId AND etm.TrainingId = src.TrainingId
    WHERE src.EmployeeId IS NULL

    -- Clean up the temporary tables
    DROP TABLE IF EXISTS #DistinctTrainings;
    DROP TABLE IF EXISTS #SrcEmployeeTraining;
END