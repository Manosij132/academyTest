CREATE PROCEDURE [dbo].[usp_MapSeniority]
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN;

		UPDATE E 
		SET E.SeniorityId = ISNULL(S.SeniorityLevel, 0)
		FROM Employee AS E
		LEFT JOIN SeniorityMaster AS S 
			ON UPPER(E.Seniority) = UPPER(S.SeniorityName) 
		
		COMMIT TRAN;
		
		SELECT '';
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN;
		SELECT CONCAT('ERROR: ', ERROR_MESSAGE());
	END CATCH
END