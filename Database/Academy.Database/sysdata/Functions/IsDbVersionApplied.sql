CREATE FUNCTION [sysdata].[IsDbVersionApplied] (@DbVersion VARCHAR(20))
RETURNS BIT
AS
BEGIN
	IF EXISTS (	
				SELECT 1
				FROM 
				(
					SELECT CONVERT(VARCHAR, Major) + '.' + CONVERT(VARCHAR, Revision) [Version]
					FROM [sysdata].[SystemVersion]
				) v
				WHERE v.[Version] = @DbVersion
			  )
		RETURN 1

	RETURN 0
END