BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
	         [ReportColumnConfigId] INT NOT NULL,
             [ReportColumnName] VARCHAR (50) NOT NULL,
             [ReportColumnDisplayName] VARCHAR (50) NOT NULL,
             [IsGroupBy] BIT NOT NULL,
             [IsActive] BIT NOT NULL,
             [CreatedBy] INT NOT NULL,
             [CreatedOn] DATETIME2 (0) NOT NULL,
             [UpdatedBy] INT NULL,
             [UpdatedOn] DATETIME2 (0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([ReportColumnConfigId], [ReportColumnName], [ReportColumnDisplayName], [IsGroupBy], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, N'Employee.Tdc', N'TDC', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, N'Employee.Community', N'Community', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, N'Employee.Project', N'Project', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, N'Employee.Seniority', N'Seniority', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (5, N'TrainingMaster.TrainingName', N'Training', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.ReportColumnConfiguration AS target
        USING @SourceTable AS source
        ON target.ReportColumnConfigId = source.ReportColumnConfigId
        WHEN MATCHED 
            AND (target.ReportColumnName <> source.ReportColumnName 
                 OR target.ReportColumnDisplayName <> source.ReportColumnDisplayName) THEN
            UPDATE 
            SET target.ReportColumnName = source.ReportColumnName,
                target.ReportColumnDisplayName = source.ReportColumnDisplayName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([ReportColumnConfigId], [ReportColumnName], [ReportColumnDisplayName], [IsGroupBy], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
            VALUES 
            (source.ReportColumnConfigId, source.ReportColumnName, source.ReportColumnDisplayName, source.IsGroupBy, source.IsActive, source.CreatedBy,source.CreatedOn,source.UpdatedBy,source.UpdatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;       

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: ReportColumnConfigurationMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO