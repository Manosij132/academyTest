CREATE PROCEDURE [dbo].[usp_BulkInsertEmployeeActivityMaps]
    @EmployeeActivityMaps [dbo].[EmployeeActivityMapType] READONLY,
	@LoggedInUserId INT,
	@EmailSubject VARCHAR(100),
	@EmailTemplate VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @MergeResults TABLE (
        ActionType NVARCHAR(10),
        MergedId INT,
        OldActivityId INT NULL,
        NewActivityId INT NULL,
        OldEmployeeId INT NULL, 
        NewEmployeeId INT NULL  
    );

	MERGE [dbo].[EmployeeActivityMap] AS T
	USING @EmployeeActivityMaps AS S
	ON (T.EmployeeId=S.EmployeeId AND T.ActivityId=S.ActivityId 
     AND ISNULL(T.ActivitySource, '') = ISNULL(S.ActivitySource, '')
     AND T.ActivityDetail = S.ActivityDetail)
	WHEN MATCHED THEN
		UPDATE SET 
			T.EndDate=S.EndDate,
			T.ActivityDetail=S.ActivityDetail,
			T.ActivitySource = S.ActivitySource,
			T.Account=S.Account,
			T.UpdatedOn = GETDATE(),
			T.UpdatedBy = @LoggedInUserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (EmployeeId,ActivityId,ActivitySource,ActivityDetail,IsActive,StartDate,EndDate,Account,CreatedBy,CreatedOn)
		VALUES (S.EmployeeId,S.ActivityId,S.ActivitySource,S.ActivityDetail,1,S.StartDate,S.EndDate,S.Account,@LoggedInUserId,GETDATE())
	OUTPUT $action,
           INSERTED.EmployeeActivityId,
           DELETED.ActivityId,
           INSERTED.ActivityId,
           DELETED.EmployeeId,  
           INSERTED.EmployeeId 
    INTO @MergeResults;

    -- Step 4: Insert records into EmailDump table
    -- This uses a JOIN to get the employee email and then inserts into EmailDump.
	DECLARE @LoggedInUserEmail VARCHAR(255)= (SELECT GlobantEmailAddress from Employee where Id=@LoggedInUserId) 
    INSERT INTO [dbo].[EmailDump] (
		[Subject],
		Template,
		[To],
		Cc,
		PlainText,
		IsActive,
		CreatedBy,
		CreatedOn
    )
    SELECT
		@EmailSubject,
		@EmailTemplate,
		emp.GlobantEmailAddress,
		@LoggedInUserEmail,
        eam.NewActivityId,
        1,
		@LoggedInUserId,
        GETDATE() -- Current timestamp for when the dump occurred
    FROM
        @MergeResults AS eam
    INNER JOIN
        [dbo].[Employee] AS emp ON eam.NewEmployeeId = emp.Id;

	Select COUNT(*) from @EmployeeActivityMaps;
END
GO
