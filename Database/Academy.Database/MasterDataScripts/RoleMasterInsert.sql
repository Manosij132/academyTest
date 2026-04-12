BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
            [RoleId] TINYINT NOT NULL,
	        [RoleName] NVARCHAR(50) NOT NULL,
            [DisplayName] NVARCHAR(50) NOT NULL,
	        [IsActive] BIT NOT NULL DEFAULT 1,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL,
            [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([RoleId], [RoleName], [DisplayName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 'SystemAdmin', 'System Admin', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 'TdcAdmin', 'TDC Admin', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, 'CommunityAdmin', 'Community Admin', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, 'EcosystemAdmin', 'Ecosystem Admin', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (5, 'AccountAdmin', 'Account Admin', 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.RoleMaster AS target
        USING @SourceTable AS source
        ON target.RoleId = source.RoleId
        WHEN MATCHED AND target.RoleName <> source.RoleName THEN
            UPDATE 
            SET target.RoleName = source.RoleName,
                target.DisplayName = source.DisplayName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([RoleId], [RoleName], [DisplayName], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.RoleId, source.RoleName, source.DisplayName, source.IsActive, source.CreatedBy, source.CreatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: RoleMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO