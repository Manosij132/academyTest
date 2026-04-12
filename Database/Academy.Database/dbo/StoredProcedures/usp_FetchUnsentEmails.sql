CREATE PROCEDURE [dbo].[usp_FetchUnsentEmails]
    @TopCount INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ed
    SET IsActive = 0,
        ErrorText = 'Email sending skipped as Employee has MailException.'
    FROM dbo.EmailDump ed
    INNER JOIN dbo.Employee e
        ON TRIM(ed.[To]) = TRIM(e.GlobantEmailAddress)
    INNER JOIN dbo.EmployeeMetadata em
        ON e.Id = em.EmployeeId
    WHERE ed.IsActive = 1
        AND em.MetaKey = 'MailException'
        AND em.MetaValue = '1';

    IF (@TopCount IS NOT NULL AND @TopCount > 0)
    BEGIN
        SELECT TOP (@TopCount) 
               EmailDumpId, 
               [To], 
               Cc, 
               Bcc, 
               [Subject], 
               Template, 
               PlainText
        FROM dbo.EmailDump
        WHERE IsActive = 1 
          AND ISNULL(ErrorText, '') = '';
    END
END