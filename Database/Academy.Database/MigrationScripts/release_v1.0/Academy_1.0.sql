IF NOT EXISTS (SELECT TOP 1 1 FROM [sysdata].[SystemVersion] WHERE Major = 1)
BEGIN
	INSERT INTO [sysdata].[SystemVersion] 
	([Major], [Revision], [ScriptName], [DateApplied])
	VALUES 
	('1', '0', 'Academy_1.0.sql', GETUTCDATE())
END