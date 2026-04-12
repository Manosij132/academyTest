CREATE PROCEDURE [sysdata].[SetDbVersion] 
	@version VARCHAR(255),
	@script VARCHAR(255)
AS
BEGIN
	DECLARE @name NVARCHAR(255), @pos INT;

	SET @pos = CHARINDEX('.', @version)

	DECLARE @major INT = CAST(SUBSTRING(@version, 1, @pos - 1) AS INT);

	SET @version = SUBSTRING(@version, @pos + 1, LEN(@version) - @pos)
	SET @pos = CHARINDEX('.', @version)

	DECLARE @revision INT = CAST(@version AS INT);

	INSERT INTO [sysdata].[SystemVersion] 
	(Major, Revision, ScriptName, DateApplied)
	VALUES 
	(@major, @revision, @script, GETUTCDATE());
END