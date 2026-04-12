CREATE PROCEDURE [dbo].[usp_InsertOrUpdateEmployeeDocument]
    @EmployeeId INT,
    @DocumentLink NVARCHAR(1024),
    @DocumentTypeId TINYINT,
    @CurrentUserId INT
AS
BEGIN
	IF EXISTS ( SELECT 1 FROM [dbo].[EmployeeDocument] WITH(NOLOCK) 
                WHERE EmployeeId = @EmployeeId AND DocumentTypeId = @DocumentTypeId)
	BEGIN
		UPDATE [dbo].[EmployeeDocument]
        SET DocumentLink = @DocumentLink,
            ReminderCount = 0,
            LastReminderSentOn = NULL,
            IsUpdateRequired = 0,
            UpdatedBy = @CurrentUserId,
            UpdatedOn = GETUTCDATE()
        WHERE EmployeeId = @EmployeeId 
            AND DocumentTypeId = @DocumentTypeId;
	END
    ELSE
    BEGIN
        INSERT INTO [dbo].[EmployeeDocument]
        ([EmployeeId], [DocumentLink], [DocumentTypeId], [ReminderCount], [LastReminderSentOn], 
         [IsUpdateRequired], [IsActive], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn])
        VALUES
        (@EmployeeId, @DocumentLink, @DocumentTypeId, 0, NULL, 
         0, 1, @CurrentUserId, GETUTCDATE(), NULL, NULL)
    END
END