CREATE PROCEDURE [dbo].[usp_InsertComment]
	@commentText NVARCHAR(255),
	@commentFor INT,
	@createdBy INT,
	@createdOn DATETIME2(0) = NULL
AS
BEGIN
	IF (@createdOn IS NULL)
	BEGIN
		SET @createdOn = (SELECT CAST(SWITCHOFFSET(SYSDATETIMEOFFSET(), '+05:30') AS DATETIME2(0)));
	END

	INSERT INTO dbo.Comment 
	(EmployeeId, CommentText, CreatedBy, CreatedOn)
	SELECT @commentFor, @commentText, @createdBy, @createdOn;
END