BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
            [EcosystemId] INT NOT NULL, 
            [EcosystemName] VARCHAR(255) NOT NULL, 
            [IsPrimary] BIT NOT NULL DEFAULT 0, 
            [ParentEcosystemId] INT NULL,
            [DisplayName] VARCHAR(255),
            [IsActive] BIT NOT NULL DEFAULT 0,
	        [CreatedBy] INT NOT NULL, 
	        [CreatedOn] DATETIME2(0) NOT NULL, 
	        [UpdatedBy] INT NULL, 
	        [UpdatedOn] DATETIME2(0) NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([EcosystemId], [EcosystemName], [IsPrimary], [ParentEcosystemId], [DisplayName], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (101, '.NET Developer', 1, NULL, '.NET', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (102, 'Java Developer', 1, NULL, 'Java', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (103, 'NodeJS Developer', 1, NULL, 'NodeJS', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (104, 'Web UI Developer', 1, NULL, 'Web UI', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (105, 'Data Engineer', 1, NULL, 'Data Architecture', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (106, 'Business Intelligence', 1, NULL, 'Business Intelligence', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (107, 'Cloud Engineer AWS', 1, NULL, 'AWS', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (108, 'Cloud Engineer Azure', 1, NULL, 'Azure', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (109, 'Cloud Engineer GCP', 1, NULL,'GCP', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (110, 'DevOps Engineer', 1, NULL, 'DevSecOps', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (111, 'TAE', 1, NULL, 'Test Automation Engineer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (112, 'QC', 1, NULL, 'Quality Control Engineer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (113, 'SalesForce Functional', 1, NULL, 'Salesforce Functional Consultant', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (114, 'Salesforce Developer', 1, NULL, 'Salesforce Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (115, 'IOS Mobile Developer', 1, NULL, 'iOS', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (116, 'Android Mobile Developer', 1, NULL, 'Android', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (117, 'Python Developer', 1, NULL, 'Python', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (118, 'Admin', 1, NULL, 'Admin', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (119, 'User Experience Designer', 1, NULL, 'User Experience Designer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (120, 'Golang', 1, NULL, 'Golang',1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (121, 'Salesforce Commerce Cloud B2C Back End Developer', 1, NULL, 'Salesforce Commerce Cloud B2C Back End Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (122, 'PHP', 1, NULL, 'PHP', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (123, 'Performance Test Engineer', 1, NULL, 'Performance Test Engineer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (124, 'Ruby', 1, NULL, 'Ruby', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (125, 'Data Architecture', 1, NULL, 'Data Architecture', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (126, 'Operations', 1, NULL, 'Operations', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (127, 'Salesforce Commerce Cloud B2C Front End Developer', 1, NULL, 'Salesforce Commerce Cloud B2C Front End Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (128, 'Project Manager', 1, NULL, 'Project Manager', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (129, 'Salesforce Mulesoft Developer', 1, NULL, 'Salesforce Mulesoft Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (130, 'Visual Designer', 1, NULL, 'Visual Designer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (131, 'SAP Development SAPUI5', 1, NULL, 'SAP Development SAPUI5', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (132, 'UI Game Developer', 1, NULL, 'UI Game Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (133, 'Game Developer', 1, NULL, 'Game Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (134, 'Delivery Manager', 1, NULL, 'Delivery Manager', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (135, 'Business Analyst', 1, NULL, 'Business Analyst', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (136, 'Drupal', 1, NULL, 'Drupal', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (137, 'Flutter', 1, NULL, 'Flutter', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (138, 'ServiceNow Developer', 1, NULL, 'ServiceNow Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (139, 'Tech Manager', 1, NULL, 'Tech Manager', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (140, 'Subject Matter Expert', 1, NULL, 'Subject Matter Expert', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (141, 'SAP Tech', 1, NULL, 'SAP Tech', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (142, 'Sharepoint Developer', 1, NULL, 'Sharepoint Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (143, 'RPA Developer', 1, NULL, 'RPA Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (144, 'ServiceNow Consultant', 1, NULL, 'ServiceNow Consultant', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (145, 'Marketing Cloud Developer', 1, NULL, 'Marketing Cloud Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (146, 'SAP Functional', 1, NULL, 'SAP Functional', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),

        (147, 'AEM Backend Developer', 1, NULL, 'AEM Backend Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (148, 'Digital Marketing Specialist', 1, NULL, 'Digital Marketing Specialist', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (149, 'Magento Developer', 1, NULL, 'Magento Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (150, 'Data Scientist', 1, NULL, 'Data Scientist', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (151, 'Salesforce Consultant', 1, NULL, 'Salesforce Consultant', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (152, 'Cloud Engineer', 1, NULL, 'Cloud Engineer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (153, 'PHP Developer', 1, NULL, 'PHP Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (154, 'Sysadmin Engineer', 1, NULL, 'Sysadmin Engineer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (155, 'Salesforce Commerce Cloud Front End Developer', 1, NULL, 'Salesforce Commerce Cloud Front End Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (156, 'Process Analyst', 1, NULL, 'Process Analyst', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (157, 'Commerce Back End Developer', 1, NULL, 'Commerce Back End Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (158, 'Database Administrator', 1, NULL, 'Database Administrator', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (159, 'C++ Developer', 1, NULL, 'C++ Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (160, 'Lead to Revenue Developer', 1, NULL, 'Lead to Revenue Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (161, 'Technical Artist', 1, NULL, 'Technical Artist', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (162, 'Lead to Revenue Consultant', 1, NULL, 'Lead to Revenue Consultant', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (163, 'Content Operations', 1, NULL, 'Content Operations', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (164, 'SQL Developer', 1, NULL, 'SQL Developer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (165, 'Marketing Cloud Consultant', 1, NULL, 'Marketing Cloud Consultant', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (166, 'HTML Designer', 1, NULL, 'HTML Designer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (167, '3D Artist', 1, NULL, '3D Artist', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (168, 'Operations Controlling Analyst', 1, NULL, 'Operations Controlling Analyst', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (169, 'Net Engineer', 1, NULL, 'Net Engineer', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (170, 'UI Artist', 1, NULL, 'UI Artist', 1, 0, GETUTCDATE(), 0, GETUTCDATE()),
        (171, 'Tableau and Salesforce Analytics', 1, NULL, 'Tableau and Salesforce Analytics', 1, 0, GETUTCDATE(), 0, GETUTCDATE())

        SET IDENTITY_INSERT dbo.EcosystemMaster ON

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.EcosystemMaster AS target
        USING @SourceTable AS source
        ON target.EcosystemId = source.EcosystemId
        WHEN MATCHED AND (target.EcosystemName <> source.EcosystemName 
            OR target.DisplayName <> source.DisplayName) THEN
            UPDATE 
            SET target.EcosystemName = source.EcosystemName,
                target.DisplayName = source.DisplayName,
                target.UpdatedOn = source.UpdatedOn,
                target.UpdatedBy = source.UpdatedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([EcosystemId], [EcosystemName], [IsPrimary], [ParentEcosystemId], [DisplayName], [IsActive], [CreatedBy], [CreatedOn])
            VALUES 
            (source.EcosystemId, source.EcosystemName, source.IsPrimary, source.ParentEcosystemId, source.DisplayName, source.IsActive, source.CreatedBy, source.CreatedOn);

        SET IDENTITY_INSERT dbo.EcosystemMaster OFF

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: EcosystemMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO