CREATE PROCEDURE [dbo].[usp_InsertOrUpdateEmployeeActivityMap]  
    @EmployeeId INT,    
    @ActivityId SMALLINT,    
    @ActivitySource VARCHAR(255) = NULL,  
    @ActivityDetail VARCHAR(255) = NULL,    
    @Comments VARCHAR(2048) = NULL,    
    @StartDate DATETIME2(0),    
    @EndDate DATETIME2(0) = NULL,    
    @StatusId TINYINT = 1,    
    @IsActive BIT = 1,    
    @recordInsertOrUpdateBy INT,    
    @recordInsertOrUpdateDate DATETIME2(0) = NULL  ,  
    @EmployeeActivityId INT=NULL,
    @Account VARCHAR(255) = NULL
AS    
BEGIN  
    SET NOCOUNT ON;    
    
    -- Default to current IST time    
    IF @recordInsertOrUpdateDate IS NULL    
        SET @recordInsertOrUpdateDate = CAST(SWITCHOFFSET(SYSDATETIMEOFFSET(), '+05:30') AS DATETIME2(0));    
    
    -- DECLARE @EmployeeActivityId INT;    
    BEGIN TRY    
        IF @StatusId IS NULL    
        BEGIN    
            RAISERROR('StatusId is required.', 16, 1);    
            RETURN;    
        END
    
        -- INSERT if not exists    
        IF @EmployeeActivityId IS NULL    
        BEGIN    
            INSERT INTO [dbo].[EmployeeActivityMap]   
            ([EmployeeId], [ActivityId], [ActivitySource],[ActivityDetail], [Comments], [StartDate], [EndDate],   
             [StatusId], [IsActive], [CreatedBy], [CreatedOn], [Account])    
            VALUES   
            (@EmployeeId, @ActivityId, @ActivitySource, @ActivityDetail, @Comments, ISNULL(@StartDate, @recordInsertOrUpdateDate), @EndDate,   
             @StatusId, @IsActive, @recordInsertOrUpdateBy, @recordInsertOrUpdateDate, @Account);    
    
            -- Get newly inserted ID    
            SET @EmployeeActivityId = SCOPE_IDENTITY();    
    
            SELECT 'Inserted' AS Result, @EmployeeActivityId AS EmployeeActivityId, @ActivityId AS ActivityId;
        END    
        -- UPDATE if exists    
        ELSE    
        BEGIN    
            UPDATE [dbo].[EmployeeActivityMap]    
            SET ActivityId = @ActivityId,
                ActivitySource = @ActivitySource,
                ActivityDetail = @ActivityDetail,
                Comments = @Comments,
                StartDate = ISNULL(@StartDate, StartDate),
                EndDate = @EndDate,
                StatusId = @StatusId,
                Account = @Account,
                IsActive = @IsActive,
                UpdatedBy = @recordInsertOrUpdateBy,   
                UpdatedOn = @recordInsertOrUpdateDate
            WHERE EmployeeActivityId = @EmployeeActivityId;    
    
            SELECT 'Updated' AS Result, @EmployeeActivityId AS EmployeeActivityId, @ActivityId AS ActivityId;
        END    
    END TRY    
    BEGIN CATCH    
        SELECT 'Error' AS Result, ERROR_MESSAGE() AS ErrorMessage;    
    END CATCH    
END;