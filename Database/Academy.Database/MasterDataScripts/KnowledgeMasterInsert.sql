BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
	        [KnowledgeId] SMALLINT NOT NULL,
	        [KnowledgeRating] TINYINT NOT NULL,
	        [KnowledgeName] NVARCHAR(50) NOT NULL,
	        [IsActive] BIT NOT NULL DEFAULT 1,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL, 
	        [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([KnowledgeId], [KnowledgeRating], [KnowledgeName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 1, 'Novice', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 2, 'Beginner', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, 3, 'Intermediate', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, 4, 'Advanced', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (5, 5, 'Expert', 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        SET IDENTITY_INSERT dbo.KnowledgeMaster ON

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.KnowledgeMaster AS target
        USING @SourceTable AS source
        ON target.KnowledgeId = source.KnowledgeId
        WHEN MATCHED 
            AND (target.KnowledgeRating <> source.KnowledgeRating 
                 OR target.KnowledgeName <> source.KnowledgeName) THEN
            UPDATE 
            SET target.KnowledgeRating = source.KnowledgeRating,
                target.KnowledgeName = source.KnowledgeName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([KnowledgeId], [KnowledgeRating], [KnowledgeName], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.KnowledgeId, source.KnowledgeRating, source.KnowledgeName, source.IsActive, source.CreatedBy, source.CreatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        SET IDENTITY_INSERT dbo.KnowledgeMaster OFF

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: KnowledgeMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO