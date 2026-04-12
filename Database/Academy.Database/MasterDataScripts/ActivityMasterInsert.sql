BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
            [ActivityId] SMALLINT NOT NULL, 
            [ActivityName] VARCHAR(100) NOT NULL, 
            [ActivityDescription] VARCHAR(500) NULL, 
            [Priority] INT NOT NULL DEFAULT 0,
            [IsActive] BIT NOT NULL DEFAULT 1,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL, 
	        [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([ActivityId], [ActivityName], [ActivityDescription], [Priority], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (1, 'Upskilling - Globant University Academy', 'GU based MVP trainings', 1.5, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (2, 'Business Oriented Academy', 'GU based Account specific trainings', 1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (3, 'Reskilling', 'GU based New Skills Introduced trainings', 1.3, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (4, 'English Academy', '', 1.4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (5, 'External Trainings', 'External Trainer coming from outside', 1.4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (6, 'Required Globant Trainings', 'Mandatory Trainings assigned from Knowbe portal', 1.4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (7, 'Self Paced Training', 'Udemy or any other portal', 1.4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (8, 'Simulated Projects', 'Assignments for Hands ON', 1.4, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (9, 'Collaborative Projects', 'DAL projects ', 1.2, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (10, 'PoC / research projects', 'POC on new tech research projects', 1.2, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (11, 'Code Challenges', '', 1.2, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (12, 'DOJO Leads', 'GX manager of DOJO', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (13, 'Area / Business initiatives', 'Account specific initiatives', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (14, 'Employer branding support', 'Work related to events and support for Branding', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (15, 'Training development', 'Globers involved in creating Content', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (16, 'Google Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (17, 'Microsoft Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (18, 'AWS Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (19, 'Salesforce Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (20, 'ServiceNow Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (21, 'SAP Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (22, 'Oracle Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (23, 'TOSCA Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (24, 'ISTQB Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (25, 'MongoDB Certification', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (26, 'AI Workshop', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (27, 'Other Certifications', '', 1.1, 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.ActivityMaster AS target
        USING @SourceTable AS source
        ON target.[ActivityId] = source.[ActivityId]
        WHEN MATCHED 
                AND ( target.[ActivityName] <> source.[ActivityName] 
                        OR target.[ActivityDescription] <> source.[ActivityDescription] 
                        OR target.[ActivityName] <> source.[ActivityName] 
                    )
                THEN
            UPDATE 
            SET target.[ActivityName] = source.[ActivityName],
                target.[ActivityDescription] = source.[ActivityDescription],
                target.[Priority] = source.[Priority],
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([ActivityId], [ActivityName], [ActivityDescription], [Priority], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.[ActivityId], source.[ActivityName], source.[ActivityDescription], source.[Priority], source.IsActive, source.CreatedBy, source.CreatedOn)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: ActivityMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO