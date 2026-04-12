BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SourceTable TABLE (
            [Id] INT NOT NULL,
	        [Name] NVARCHAR(255) NOT NULL,
            [SheetId] NVARCHAR(100) NOT NULL,
	        [RefType] NVARCHAR(50) NOT NULL,
	        [Range] NVARCHAR(100) NOT NULL,
            [EmailSendFreq] INT NULL,
            [SheetName] NVARCHAR(255) NULL, 
            [PointOfContact] NVARCHAR(1000) NULL, 
            [IsActive] BIT NULL
        )

        -- Insert the source data (this is just a setup for the example)
        INSERT INTO @SourceTable 
        ([Id], [Name], [SheetId], [RefType], [Range], [EmailSendFreq], [SheetName], [PointOfContact], [IsActive])
        VALUES
        (1, N'Data Entry', N'11sD0Fagww1z0DQff1l9Ba-L4i7U-HP6UJZGRzyb8aKM', N'DataEntry', N'A:S', 1, N'Globant India Data Entry', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (2, N'Sheet1', N'1nWpVG_-95wfsGoUWNXPVNY5_WT6o5TMtlmKcmcr_L-I', N'SSF', N'A:AN', 1, N'APAC SSF Dump', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (3, N'Data', N'1QefU0LAcRjpLn1O6zLetgwBy_LKhohDU7PFFa2Ks_W4', N'Location', N'B:E', 1, N'Active Employees Base Location and Upcoming location change', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (5, N'Resignation', N'1YBkEffi9RSPvP62-9Y--6sIaU_yKb2hvwqbC2DjhcjY', N'Resignation', N'A:P', 1, N'Globant India Resignation Data Entry', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (6, N'India Anchors - 2025', N'1ElQAwEWdW506alIhPRrSOlHlGimGsTGu2m38CXNh84M', N'Portfolio', N'A:Q', 1, NULL, N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (7, N'All Data', N'11qqHTW-5KpBpnTCp1szFSKHnvMaA7zKqSnYjHXb0hGE', N'EmployeeData', N'A2:AX', 1, N'New Globant India Employee Database', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (8, N'Base Data', N'16_lpxbpEBZNTa8-o8i1-Ude-2pM3_MDPgmqVOitxKmY', N'Glow', N'A:L', 1, N'India - Latest Glow Allocation Data', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (9, N'Base Data', N'15WTURX9GE0JgIGxgTwJdp-IID-dIeLAksaLrihUD2ag', N'BetterMe', N'A:E', 1, N'BetterMe Data', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (10, N'New Vertical Mapping', N'1B-2gEvgjgHTE9miSFCkTeG6XoZzi7553RGcfundMOjY', N'Lookup', N'A:K', 1, N'Lookup data', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (11, N'OP for DB', N'1cuaaB1J4uCGINRBm9LXLR5mAjNmKWXTOXPj8l9mE5Ag', N'Onboarding', N'A:I', 1, N'Personal and Professional Info.Response Looker', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (12, N'Current-All', N'1ZfY6i2XPZycQvX9Oe_OJMaxv9T33_8RSOI15iyhnpIc', N'CareerReport', N'A:J', 1, N'Globant India Career Data Report', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (13, N'Employees Data', N'11qqHTW-5KpBpnTCp1szFSKHnvMaA7zKqSnYjHXb0hGE', N'EmployeeDataLive', N'A2:AX', 1, N'New Globant India Employee Database', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (15, N'APAC System Dump', N'1nWpVG_-95wfsGoUWNXPVNY5_WT6o5TMtlmKcmcr_L-I', N'APACSSF', N'A:AK', 1, N'APAC SSF Dump', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1),
        (17, N'Region Data', N'1nWpVG_-95wfsGoUWNXPVNY5_WT6o5TMtlmKcmcr_L-I', N'SSFRegion', N'A:F', 1, N'APAC SSF Dump', N'ritesh.lokhande@globant.com,vishal.salunkhe@globant.com,k.joshi@globant.com', 1)            

        SET IDENTITY_INSERT dbo.GoogleSheetConfiguration ON;

        -- Use the MERGE statement to merge data from the source table into the target table
        MERGE dbo.GoogleSheetConfiguration AS target
        USING @SourceTable AS source
        ON target.Id = source.Id
        WHEN MATCHED AND target.Name <> source.Name THEN
            UPDATE 
            SET target.Name = source.Name,
                target.SheetId = source.SheetId,
                target.RefType = source.RefType,
                target.Range = source.Range,                
                target.EmailSendFreq = source.EmailSendFreq,
                target.SheetName = source.SheetName,
                target.PointOfContact = source.PointOfContact,
                target.IsActive = source.IsActive
        WHEN NOT MATCHED BY TARGET THEN
            INSERT 
            ([Id], [Name], [SheetId], [RefType], [Range], [EmailSendFreq], [SheetName], [PointOfContact], [IsActive])
            VALUES 
            (source.Id, source.Name, source.SheetId, source.RefType, source.Range, source.EmailSendFreq, source.SheetName, source.PointOfContact, source.IsActive)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        SET IDENTITY_INSERT dbo.GoogleSheetConfiguration OFF;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: GoogleSheetConfiguration'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO