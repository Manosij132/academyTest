BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
	        [ReportId]       INT           NOT NULL,
            [ReportName]     VARCHAR (100) NOT NULL,
            [StoredProcName] VARCHAR (200) NOT NULL,
            [IsActive]       BIT           NOT NULL,
            [CreatedBy]      INT           NOT NULL,
            [CreatedOn]      DATETIME2 (0) NOT NULL,
            [UpdatedBy]      INT           NULL,
            [UpdatedOn]      DATETIME2 (0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([ReportId], [ReportName], [StoredProcName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES 
        (1, N'Detailed Report', N'usp_FetchDynamicEmployeeTrainingReport_Detailed', 1, 1, CAST(N'2025-07-15T17:51:30.0000000' AS DateTime2), 1, CAST(N'2025-07-15T17:51:30.0000000' AS DateTime2)),
        (2, N'Sumarised Report', N'usp_FetchDynamicEmployeeTrainingReport_Summary', 1, 1, CAST(N'2025-07-15T17:51:30.0000000' AS DateTime2), 1, CAST(N'2025-07-15T17:51:30.0000000' AS DateTime2)),
        (3, N'Compliance Report', N'usp_FetchDynamicEmployeeTrainingReport_Compliance', 1, 1, CAST(N'2025-07-01T00:00:00.0000000' AS DateTime2), 1, CAST(N'2025-07-01T00:00:00.0000000' AS DateTime2))
      
             
        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.[ReportType] AS target
        USING @SourceTable AS source
        ON target.ReportId = source.ReportId
        WHEN MATCHED 
            AND (target.ReportName <> source.ReportName 
                ) THEN
            UPDATE 
            SET target.ReportName = source.ReportName,
                target.StoredProcName = source.StoredProcName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
             ([ReportId], [ReportName], [StoredProcName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES 
             
            (source.ReportId, source.ReportName, source.StoredProcName, source.IsActive, source.CreatedBy, source.CreatedOn,source.UpdatedBy,source.UpdatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;


        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: ReportTypeMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO