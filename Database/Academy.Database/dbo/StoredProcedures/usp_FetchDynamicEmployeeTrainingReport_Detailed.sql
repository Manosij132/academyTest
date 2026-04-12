CREATE PROCEDURE [dbo].[usp_FetchDynamicEmployeeTrainingReport_Detailed]
    @SelectColumns NVARCHAR(MAX),       -- Columns to select
    @WhereClause NVARCHAR(MAX),          -- Additional filtering conditions
    @ActivityType nvarchar(max)=NULL 
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);
	Declare @ConfigureColumns NVARCHAR(MAX);
    Declare @ReportCol NVARCHAR(MAX);

    set @ReportCol = (Select
								STRING_AGG(ReportColumnName, ',')
							from
							(
								Select
									ReportColumnName as ReportColumnName
								from
									[dbo].[ReportColumnConfiguration]
								where
									ReportColumnConfigId in (
										SELECT
											CAST(value AS INT) AS Number
										FROM
											STRING_SPLIT(@SelectColumns, ',')
										WHERE
											TRY_CAST(value AS INT) IS NOT NULL
									)
							) as ReportColumns);

	set @ConfigureColumns='Employee.EmployeeName, Employee.globantemailaddress as Email, TrainingStatusName as Status, ' + @ReportCol

if @ActivityType = 1
 begin
    -- Construct the SQL query dynamically
    SET @SQL = 'SELECT ' + @ConfigureColumns + ' ' +
               ' FROM [dbo].[Employee] AS Employee ' +
               ' LEFT JOIN [dbo].[EmployeeTrainingMap] AS EmployeeTrainingMap ON EmployeeTrainingMap.EmployeeId = Employee.ID ' +
               ' LEFT JOIN [dbo].[TrainingMaster] AS TrainingMaster ON TrainingMaster.TrainingId = EmployeeTrainingMap.TrainingId ' +
			   ' LEFT JOIN [dbo].[TrainingStatusMaster] AS TrainingStatusMaster ON EmployeeTrainingMap.TrainingStatusId = TrainingStatusMaster.TrainingStatusId ' +
			   ' LEFT JOIN [dbo].[LearningPathTrainingMap] LearningPathTrainingMap ON TrainingMaster.TrainingId = LearningPathTrainingMap.TrainingId and LearningPathTrainingMap.SeniorityId = Employee.SeniorityId' +   -- :white_check_mark: New Join
               ' LEFT JOIN [dbo].[LearningPath] LearningPath ON LearningPathTrainingMap.LearningPathId = LearningPath.LearningPathId ' +
               ' WHERE Employee.IsActive = 1 ' +
               ' AND EmployeeTrainingMap.IsActive = 1 ' +
               CASE
                   WHEN @WhereClause IS NOT NULL AND @WhereClause <> ''
                   THEN ' AND ' + @WhereClause
                   ELSE ''
               END +
               ' ORDER BY ' + @ReportCol;
    -- Print the SQL for debugging purposes (optional)
		end
		else
		begin
	
   SET @SQL =
    'SELECT ' + @ConfigureColumns + ' ' +
    'FROM [dbo].[EmployeeActivityMap] AS EmployeeActivityMap ' +
    'LEFT JOIN [dbo].[ActivityMaster] AS ActivityMaster ' +
        'ON EmployeeActivityMap.ActivityId = ActivityMaster.ActivityId ' +
    'LEFT JOIN [dbo].[Employee] AS Employee ' +
        'ON EmployeeActivityMap.EmployeeId = Employee.Id ' +
    'LEFT JOIN [dbo].[TrainingStatusMaster] AS TrainingStatusMaster ' +
        'ON EmployeeActivityMap.StatusId = TrainingStatusMaster.TrainingStatusId ' +
    'WHERE Employee.IsActive = 1 ' +
      'AND EmployeeActivityMap.IsActive = 1 ' +
      CASE
          WHEN @WhereClause IS NOT NULL AND @WhereClause <> ''
          THEN ' AND ' + @WhereClause
          ELSE ''
      END + ' ' +
    'ORDER BY ' + @ReportCol + ';';
		end
    PRINT @SQL;
    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL;
	
END;
