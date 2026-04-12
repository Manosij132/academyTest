CREATE PROCEDURE [dbo].[usp_FetchProficiencyDataReport_Detailed]
    @WhereClause NVARCHAR(MAX) = NULL  -- Optional parameter for additional filtering
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = 'select BetterMeLeaderEmail,GlobantEmailAddress,Community,Project,TrainingsAssigned ,TrainingsCompleted,[% Completed],ProficiencyLag,
	Ecosystem,[On DOJO],ReminderCount,JoiningDate,GloberAccount,ConsolidatedSkills,EmployeeName,TDC,TrainingAssignedDate,
	ExpectedTrainingEndDate,DojoStartDate,GexLeaders,DojoGexLeaderEmail from [dbo].[vwFetchProficiencyDataReport]'

    -- Append the WHERE clause if provided
    IF @WhereClause IS NOT NULL AND @WhereClause <> ''
    BEGIN
        SET @SQL = @SQL + ' WHERE ' + @WhereClause
    END
    PRINT @SQL;  -- For debugging purposes, you can remove this in production
    EXEC sp_executesql @SQL;
END