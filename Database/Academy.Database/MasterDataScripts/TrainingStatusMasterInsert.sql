BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
            [TrainingStatusId] TINYINT NOT NULL, 
            [TrainingStatusName] NVARCHAR(50) NULL, 
            [IsActive] BIT NOT NULL DEFAULT 1,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL, 
	        [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([TrainingStatusId], [TrainingStatusName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 'Pending', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 'Completed', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, 'Ongoing', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, 'Deferred', 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.TrainingStatusMaster AS target
        USING @SourceTable AS source
        ON target.TrainingStatusId = source.TrainingStatusId
        WHEN MATCHED AND target.TrainingStatusName <> source.TrainingStatusName THEN
            UPDATE 
            SET target.TrainingStatusName = source.TrainingStatusName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([TrainingStatusId], [TrainingStatusName], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.TrainingStatusId, source.TrainingStatusName, source.IsActive, source.CreatedBy, source.CreatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: TrainingStatusMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO