CREATE PROCEDURE [dbo].[usp_FetchEmployeesStartsWith]
	@where VARCHAR(MAX), 
	@ecosystemId INT,
	@client VARCHAR(500)=null
AS    
BEGIN
	DECLARE @ecosystem VARCHAR(255) = (SELECT EcosystemName FROM EcosystemMaster WHERE EcosystemId = @ecosystemId);

    DECLARE @query VARCHAR(MAX) = 'SELECT Id, GlobantEmailAddress, [Image], SeniorityId FROM Employee WHERE ' + @where + ' AND WorkingEcosystem = ''' + @ecosystem + '''';

    -- Conditionally add the client filter
    IF @client IS NOT NULL
    BEGIN
        SET @query = @query + ' AND Client = ''' + REPLACE(@client, '''', '''''') + '''';
    END;

    --Print the query for debugging (Important!)
    --PRINT @query;

    EXECUTE (@query); 
END