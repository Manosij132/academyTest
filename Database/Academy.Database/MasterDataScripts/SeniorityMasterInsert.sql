BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @SourceTable TABLE (
            [SeniorityId] SMALLINT NOT NULL, 
	        [SeniorityLevel] TINYINT NOT NULL,
            [SeniorityName] NVARCHAR(50) NOT NULL,
            [Experience] VARCHAR(10) NULL,
	        [IsActive] BIT NOT NULL DEFAULT 1,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL,
            [UpdatedBy] INT NULL,
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([SeniorityId], [SeniorityLevel], [SeniorityName], [Experience], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 1, 'Tech Director', NULL, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 2, 'Tech Manager', NULL, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, 2, 'Subject Matter Expert', NULL, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, 3, 'Architect', '10-14', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (5, 3, 'Sr Level 3', '10-14', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (6, 4, 'Software Designer', '8-10', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (7, 4, 'Sr Level 2', '8-10', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (8, 5, 'Sr', '6-8', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (9, 5, 'Sr Level 1', '6-8', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (10, 6, 'SSr Adv', '4-6', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (11, 7, 'SSr', '2-4', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (12, 8, 'Jr Adv', '1-2', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (13, 9, 'Jr', '0-1', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (14, 10, 'NA', NULL, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (15, 0, 'Studio Partner', NULL, 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        SET IDENTITY_INSERT dbo.SeniorityMaster ON

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.SeniorityMaster AS target
        USING @SourceTable AS source
        ON target.SeniorityId = source.SeniorityId
        WHEN MATCHED AND (target.SeniorityName <> source.SeniorityName
                            OR target.Experience <> source.Experience)
        THEN
            UPDATE 
            SET target.SeniorityName = source.SeniorityName,
                target.Experience = source.Experience,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT
            ([SeniorityId], SeniorityLevel, SeniorityName, Experience, IsActive, CreatedBy, CreatedOn)
            VALUES 
            (source.SeniorityId, source.SeniorityLevel, source.SeniorityName, source.Experience,
             source.IsActive, source.CreatedBy, source.CreatedOn);

        SET IDENTITY_INSERT dbo.SeniorityMaster OFF
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: SeniorityMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO