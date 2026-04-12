CREATE PROCEDURE [dbo].[usp_InsertDojoWfoEmailEntries]
    @DojoEmployeeList dbo.udt_DojoEmployeeList READONLY,
    @TemplateName VARCHAR(30),                 
    @IsCareerMentorIncluded VARCHAR(3),        
    @DefaultCcList VARCHAR(255),                
    @Subject VARCHAR(255)                          
AS
BEGIN
    SET NOCOUNT ON;

    -- Split template list into table
    ;WITH EmployeeWithMentor AS (
        SELECT EL.EmployeeEmailId, E.BetterMeLeaderEmail
        FROM @DojoEmployeeList EL
        LEFT JOIN Employee E ON E.GlobantEmailAddress = EL.EmployeeEmailId
    )
    INSERT INTO EmailDump 
    ([Subject], [Template], [To], [Cc], [Bcc], [IsActive], [CreatedBy], [CreatedOn])
    SELECT
        @Subject,
        @TemplateName,
        E.EmployeeEmailId,
        CASE WHEN @IsCareerMentorIncluded = 'Yes' AND E.BetterMeLeaderEmail IS NOT NULL AND E.BetterMeLeaderEmail LIKE '%@globant.com%'
                THEN TRIM(ISNULL(@DefaultCcList, '') + ',' + E.BetterMeLeaderEmail)
             ELSE @DefaultCcList
        END AS Cc,
        '',
        1,                
	0,
	GETUTCDATE()      
    FROM EmployeeWithMentor E
END
