CREATE FUNCTION [sysdata].[GetDbVersion] ()
RETURNS VARCHAR(20)
AS
BEGIN
	DECLARE @maj INT
	DECLARE @rev INT
	DECLARE @result NVARCHAR(50)

	IF EXISTS (SELECT 1 FROM [sysdata].[SystemVersion])
	BEGIN
		SELECT TOP 1 @maj = Major, @rev = Revision
		FROM [sysdata].[SystemVersion]
		ORDER by Id DESC

		SET @result = CONVERT(VARCHAR, @maj) + '.' + CONVERT(VARCHAR, @rev)
	END
	ELSE
		SET @result = '0.0';

	RETURN @result;
END