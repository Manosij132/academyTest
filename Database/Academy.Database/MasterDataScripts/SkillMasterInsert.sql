BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.SkillMaster WHERE SkillName = 'Not Available'
        )
        BEGIN
            INSERT INTO dbo.SkillMaster (SkillId, SkillName, IsActive, CreatedBy, CreatedOn)
            VALUES (
                (SELECT ISNULL(MAX(SkillId), 0) + 1 FROM dbo.SkillMaster),
                'Not Available',
                1,
                0,
                GETUTCDATE()
            );
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT 'Error occurred while running script: SkillMasterInsert'
        PRINT ERROR_MESSAGE();
        
        IF ( @@TRANCOUNT > 0 )
            ROLLBACK TRANSACTION;

        THROW
    END CATCH
END
GO