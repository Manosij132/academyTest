CREATE PROCEDURE [dbo].[usp_GetDojoGlobarDetails]
    @SearchTerm NVARCHAR(200) = NULL, -- The value to search for (will be applied to EmployeeName OR GlobantEmailAddress)
    @PageNumber INT = 1,              -- The current page number (1-based), default is 1
    @PageSize INT = 20,                -- The number of records per page, default is 20
	@community VARCHAR(1000) = NULL,
	@Country VARCHAR(1000) = NULL,
    @AiStudio VARCHAR(1000) = NULL,
	@Account VARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON; -- Prevents the count of the number of rows affected from being returned.

    -- Declare variables for dynamic SQL
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @Offset INT;   

    -- Calculate the OFFSET for pagination
    SET @Offset = (@PageNumber - 1) * @PageSize;

    -- Base query for selecting data
    DECLARE @BaseSelect NVARCHAR(MAX) = N'
        SELECT DD.DojoDetailId
             , DD.EmployeeId
             , E.EmployeeName
             , E.GlobantEmailAddress
             , DD.DojoStartDate
             , DD.DojoEndDate
             , DD.DojoGexLeaderEmail
             , DD.AssignedThroughTraining
             , DD.Comments
             , DD.TicketNumber
			 , E.Community
             , E.AiStudio
             , E.Client as Account
        FROM DojoDetail DD
        INNER JOIN Employee E ON DD.EmployeeId = E.Id
        WHERE E.IsActive = 1 AND DD.IsActive = 0 AND DD.AssignedThroughTraining IS NULL';

    -- Base query for counting records
    DECLARE @BaseCountSelect NVARCHAR(MAX) = N'
        SELECT COUNT(*) AS TotalFilteredRecords
        FROM DojoDetail DD
        INNER JOIN Employee E ON DD.EmployeeId = E.Id
        WHERE E.IsActive = 1 AND DD.IsActive = 0 AND DD.AssignedThroughTraining IS NULL';

    -- Add filter condition if SearchTerm is provided
    IF @SearchTerm IS NOT NULL
    BEGIN
        -- Append the filter condition to both the main select and count queries
        -- The filter now applies to both EmployeeName OR GlobantEmailAddress
        SET @BaseSelect = @BaseSelect + N' AND (E.EmployeeName LIKE @P_SearchTerm + ''%'' OR E.GlobantEmailAddress LIKE @P_SearchTerm + ''%'')';
        SET @BaseCountSelect = @BaseCountSelect + N' AND (E.EmployeeName LIKE @P_SearchTerm + ''%'' OR E.GlobantEmailAddress LIKE @P_SearchTerm + ''%'')';
    END

    -- Add filter condition if Community is provided (NEW ADDITION)
    IF @Community IS NOT NULL AND @Community<>''
    BEGIN
        SET @BaseSelect = @BaseSelect + N' AND E.Community IN (SELECT value FROM STRING_SPLIT(@P_Community, '',''))';
        SET @BaseCountSelect = @BaseCountSelect + N' AND E.Community IN (SELECT value FROM STRING_SPLIT(@P_Community, '',''))';
    END

     -- Add filter condition if Country is provided (NEW ADDITION)
    IF @Country IS NOT NULL AND @Country<>''
    BEGIN
        SET @BaseSelect = @BaseSelect + N' AND E.Tdc IN (SELECT value FROM STRING_SPLIT(@P_Country, '',''))';
        SET @BaseCountSelect = @BaseCountSelect + N' AND E.Tdc IN (SELECT value FROM STRING_SPLIT(@P_Country, '',''))';
    END

     -- Add filter condition if AiStudio is provided (NEW ADDITION)
    IF @AiStudio IS NOT NULL AND @AiStudio<>''
    BEGIN
        SET @BaseSelect = @BaseSelect + N' AND E.AiStudio IN (SELECT value FROM STRING_SPLIT(@P_AiStudio, '',''))';
        SET @BaseCountSelect = @BaseCountSelect + N' AND E.AiStudio IN (SELECT value FROM STRING_SPLIT(@P_AiStudio, '',''))';
    END

     -- Add filter condition if Account is provided (NEW ADDITION)
    IF @Account IS NOT NULL AND @Account<>''
    BEGIN
        SET @BaseSelect = @BaseSelect + N' AND E.Client IN (SELECT value FROM STRING_SPLIT(@P_Account, '',''))';
        SET @BaseCountSelect = @BaseCountSelect + N' AND E.Client IN (SELECT value FROM STRING_SPLIT(@P_Account, '',''))';
    END

    -- Finalize the main SQL query with ORDER BY and pagination
    SET @SQL = @BaseSelect + N'
        ORDER BY DD.DojoDetailId DESC
        OFFSET @P_Offset ROWS
        FETCH NEXT @P_PageSize ROWS ONLY;';

    -- Execute the main query
    -- The parameter list now includes @P_Community and its type
	EXEC sp_executesql
		@SQL,
		N'@P_SearchTerm NVARCHAR(MAX), @P_Offset INT, @P_PageSize INT, @P_Community VARCHAR(1000), 
        @P_Country VARCHAR(1000), @P_AiStudio VARCHAR(1000), @P_Account VARCHAR(1000)',
		@P_SearchTerm = @SearchTerm,
		@P_Offset = @Offset,
		@P_PageSize = @PageSize,
		@P_Community = @Community, -- You must pass the value here
        @P_Country = @Country,
        @P_AiStudio = @AiStudio,
        @P_Account = @Account;

    -- Finalize the count SQL query
    SET @CountSQL = @BaseCountSelect + N';';

    -- The parameter list now includes @P_Community and its type
	EXEC sp_executesql
		@CountSQL,
		N'@P_SearchTerm NVARCHAR(MAX), @P_Community VARCHAR(1000), @P_Country VARCHAR(1000), @P_AiStudio VARCHAR(1000), @P_Account VARCHAR(1000)',
		@P_SearchTerm = @SearchTerm,
		@P_Community = @Community, -- Pass the value
        @P_Country = @Country,
        @P_AiStudio = @AiStudio,
        @P_Account = @Account;

END;