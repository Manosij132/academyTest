BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
	        [ProficiencyId] SMALLINT NOT NULL,
	        [ProficiencyRating] TINYINT NOT NULL,
	        [ProficiencyName] NVARCHAR(50) NOT NULL,
	        [IsActive] BIT NOT NULL DEFAULT 1,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL, 
	        [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([ProficiencyId], [ProficiencyRating], [ProficiencyName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 1, 'Can''t Perform', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 2, 'With Supervision', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, 3, 'With Limited Supervision', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, 4, 'Without Supervision', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (5, 5, 'Can Teach Others', 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        SET IDENTITY_INSERT dbo.ProficiencyMaster ON

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.ProficiencyMaster AS target
        USING @SourceTable AS source
        ON target.ProficiencyId = source.ProficiencyId
        WHEN MATCHED 
            AND (target.ProficiencyRating <> source.ProficiencyRating 
                 OR target.ProficiencyName <> source.ProficiencyName) THEN
            UPDATE 
            SET target.ProficiencyRating = source.ProficiencyRating,
                target.ProficiencyName = source.ProficiencyName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([ProficiencyId], [ProficiencyRating], [ProficiencyName], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.ProficiencyId, source.ProficiencyRating, source.ProficiencyName, source.IsActive, source.CreatedBy, source.CreatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        SET IDENTITY_INSERT dbo.ProficiencyMaster OFF

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: ProficiencyMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO