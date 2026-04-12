CREATE PROCEDURE [dbo].[usp_UpdateGloberTrainingStatus]
	@globerTrainingStatus dbo.udt_GloberTrainingStatus READONLY
AS
BEGIN
	DECLARE @completedTopicStatusId INT = 2

	;WITH cte_FilteredGloberTrainingStatus AS 
	(
        SELECT	GloberEmail, TrainingLink, TopicStatusId, UpdatedOn, UpdatedByEmail,
				ROW_NUMBER() OVER (	PARTITION BY GloberEmail, TrainingLink ORDER BY 
									CASE WHEN TopicStatusId = @completedTopicStatusId THEN 0 ELSE 1 END, UpdatedOn) AS rn
        FROM @GloberTrainingStatus
    )

	UPDATE etm
	SET TrainingStatusId = gts.TopicStatusId,
		UpdatedOn = gts.UpdatedOn,
		UpdatedBy = upd.Id
	FROM EmployeeTrainingMap etm
	INNER JOIN Employee e
		ON etm.EmployeeId = e.Id
	INNER JOIN TrainingMaster tm
		ON etm.TrainingId = tm.TrainingId
	INNER JOIN cte_FilteredGloberTrainingStatus gts
		ON e.GlobantEmailAddress = gts.GloberEmail
			AND tm.TrainingUrl = gts.TrainingLink
			AND etm.TrainingStatusId <> gts.TopicStatusId
	INNER JOIN Employee upd
		ON gts.UpdatedByEmail = upd.GlobantEmailAddress
END