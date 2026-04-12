BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
	        [EmployeeDocumentTypeId] TINYINT NOT NULL,
	        [DocumentType] VARCHAR(30) NOT NULL,
	        [IsEligibleForReminder] BIT NOT NULL,
	        [IsActive] BIT NOT NULL,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL, 
	        [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([EmployeeDocumentTypeId], [DocumentType], [IsEligibleForReminder], 
         [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 'CV', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 'Profile', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        SET IDENTITY_INSERT dbo.EmployeeDocumentTypeMaster ON

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.EmployeeDocumentTypeMaster AS target
        USING @SourceTable AS source
        ON target.EmployeeDocumentTypeId = source.EmployeeDocumentTypeId
        WHEN MATCHED 
            AND (target.DocumentType <> source.DocumentType 
                 OR target.IsEligibleForReminder <> source.IsEligibleForReminder) THEN
            UPDATE 
            SET target.DocumentType = source.DocumentType,
                target.IsEligibleForReminder = source.IsEligibleForReminder,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([EmployeeDocumentTypeId], [DocumentType], [IsEligibleForReminder], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.EmployeeDocumentTypeId, source.DocumentType, source.IsEligibleForReminder, source.IsActive, source.CreatedBy, source.CreatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        SET IDENTITY_INSERT dbo.EmployeeDocumentTypeMaster OFF

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: EmployeeDocumentTypeMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO