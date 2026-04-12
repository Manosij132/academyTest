CREATE PROCEDURE [dbo].[usp_ProcessTrainingAssignment]
	@force BIT, 
	@transactionId VARCHAR(20),
	@trainingAssignmentSrc VARCHAR(50),
	@crossStudioId SMALLINT = NULL
AS
BEGIN
	DECLARE @systemUserId INT = 0;
	DECLARE @pending TINYINT = (SELECT TOP 1 TrainingStatusId FROM TrainingStatusMaster WHERE TrainingStatusName = 'Pending');
	DECLARE @training_data TABLE 
	(
		JobRequestDetailId INT
		,EcosystemId INT
		,EmployeeId INT
		,SeniorityId INT
		,SkillId INT
		,TrainingId INT
		,ExpectedProficiency INT
		,CurrentProficiency INT
		,[Status] VARCHAR(10)
		,StartDate DATETIME2 DEFAULT GETUTCDATE()
		,ExpectedEndDate DATETIME2 DEFAULT DATEADD(DAY,1,GETUTCDATE())
		,Account VARCHAR(255)
		,IsActive BIT DEFAULT 1
		,CreatedBy INT
		,CreatedDate DATETIME2 DEFAULT GETUTCDATE()
		,[Action] VARCHAR(MAX)
		,TrainingAssignmentIndex INT
	);

	WITH cte1 AS
	(
		SELECT	A.JobRequestDetailId
				,A.[Key],A.[Value],A.GlobantEmailAddress,B.Client AS Account
				,A.CreatedBy
				,CAST(JSON_VALUE(A.[Key], '$.SkillId') AS SMALLINT) AS SkillId
				,CAST(JSON_VALUE(A.[Key], '$.TrainingId') AS INT) AS TrainingId
				,B.Id AS EmployeeId, B.SeniorityId, B.EcosystemId
		FROM JobRequestDetail AS A
		JOIN Employee AS B 
			ON A.GlobantEmailAddress = B.GlobantEmailAddress
		WHERE A.[Status] =  'Pending' 
			AND A.TransactionId = @transactionId
	)
	,cte2 AS 
	(
		SELECT ET.EmployeeId, ET.TrainingId, E.GlobantEmailAddress, ET.SkillId
		FROM EmployeeTrainingMap AS ET
		JOIN Employee AS E 
			ON ET.EmployeeId = E.Id
		INNER JOIN JobRequestDetail JRD
			ON E.GlobantEmailAddress = JRD.GlobantEmailAddress
		WHERE JRD.TransactionId = @transactionId AND JRD.[Status] =  'Pending'
	)

	INSERT INTO @training_data 
	SELECT	cte1.JobRequestDetailId
			,cte1.EcosystemId
			,cte1.EmployeeId
			,cte1.SeniorityId
			,cte1.SkillId
			,cte1.TrainingId
			,TP.ExpectedProficiency
			,ISNULL(SE.CurrentProficiency, 1) AS CurrentProficiency
			,@pending
			,GETUTCDATE()
			,DATEADD(DAY, 21, GETUTCDATE())
			,cte1.Account
			,1 AS IsActive
			,cte1.CreatedBy
			,GETUTCDATE()
			,(
				CASE WHEN ET.TrainingId IS NULL THEN NULL 
					 ELSE 'Training has already been assigned to the user.' 
				END
			 ) AS [Action]
			,ROW_NUMBER() OVER (PARTITION BY cte1.EmployeeId, cte1.SkillId, cte1.TrainingId 
								ORDER BY cte1.EmployeeId, cte1.SkillId, cte1.TrainingId) AS TrainingAssignmentIndex
	FROM cte1 
	JOIN TrainingProficiencyMap AS TP 
		ON TP.EcosystemId = ISNULL(@crossStudioId, cte1.EcosystemId)
			AND cte1.SeniorityId = TP.SeniorityId 
			AND cte1.SkillId = TP.SkillId 
			AND cte1.TrainingId = TP.TrainingId 
			AND TP.IsActive = 1
	LEFT JOIN SkillEndorsementMap AS SE 
		ON cte1.EmployeeId = SE.EmployeeId 
			AND cte1.SkillId = SE.SkillId 
			AND SE.IsActive = 1
	LEFT JOIN EmployeeTrainingMap AS ET 
		ON cte1.EmployeeId = ET.EmployeeId 
			AND cte1.SkillId = ET.SkillId 
			AND cte1.TrainingId = ET.TrainingId

	-- DELETING DUPLICATE RECORDS
	DELETE FROM @training_data WHERE TrainingAssignmentIndex > 1;
			
	BEGIN TRY
		BEGIN TRAN

		IF (@force = 0)
		BEGIN
			UPDATE @training_data 
			SET [Action] = 'User is proficient, training not required.' 
			WHERE CurrentProficiency >= ExpectedProficiency;
					
			UPDATE JRD 
			SET JRD.Comment = TR.[Action], 
				JRD.[Status] = 'Completed',
				JRD.UpdatedBy = @systemUserId, 
				JRD.UpdatedOn = GETUTCDATE()
			FROM JobRequestDetail AS JRD 
			JOIN @training_data AS TR 
				ON JRD.JobRequestDetailId = TR.JobRequestDetailId
			WHERE TR.[Action] IS NOT NULL;

			INSERT INTO EmployeeTrainingMap 
			(ExpectedEndDate, CreatedBy, CreatedOn, EmployeeId, IsActive, SkillId, StartDate,
			 TrainingId, TrainingStatusId, TrainingTimeAccount, TrainingTimeSeniorityId,
			 TraingAssignmentSrc)
			SELECT	ExpectedEndDate, CreatedBy, CreatedDate, EmployeeId, IsActive, SkillId, StartDate,
					TrainingId, @pending, Account, SeniorityId, @trainingAssignmentSrc
			FROM @training_data 
			WHERE [Action] IS NULL;

			INSERT INTO dbo.Comment
			(EmployeeId, CommentText, IsActive, CreatedBy, CreatedOn)                        
			SELECT DISTINCT EmployeeId, 'New Training has been assigned to you', 1, 0, GETUTCDATE()
			FROM @training_data 
			WHERE [Action] IS NULL;

			INSERT INTO EmailDump 
			([To], [Cc], [Bcc], [Subject], Template, IsActive, CreatedBy, CreatedOn)                                         
			SELECT DISTINCT	e.GlobantEmailAddress, e.BetterMeLeaderEmail, NULL, 'Action Required : Mandatory Training Assigned To You',
					'GU_USER_ADDED', 1, 0, GETUTCDATE()
			FROM @training_data td
			INNER JOIN dbo.Employee e
				ON td.EmployeeId = e.Id
			WHERE [Action] IS NULL;

			UPDATE JRD 
			SET JRD.[Status] = 'Completed',
				JRD.UpdatedBy = @systemUserId, 
				JRD.UpdatedOn = GETUTCDATE()
			FROM JobRequestDetail AS JRD 
			JOIN @training_data AS TR 
				ON JRD.JobRequestDetailId = TR.JobRequestDetailId
			WHERE TR.[Action] IS NULL;
		END
		ELSE
		BEGIN
			INSERT INTO EmployeeTrainingMap 
			(ExpectedEndDate, CreatedBy, CreatedOn, EmployeeId, IsActive, SkillId, StartDate,
			 TrainingId, TrainingStatusId, TrainingTimeAccount, TrainingTimeSeniorityId,
			 TraingAssignmentSrc)
			SELECT	ExpectedEndDate, CreatedBy, CreatedDate, EmployeeId, IsActive, SkillId, StartDate,
					TrainingId, @pending, Account, SeniorityId, @trainingAssignmentSrc
			FROM @training_data

			INSERT INTO dbo.Comment
			(EmployeeId, CommentText, IsActive, CreatedBy, CreatedOn)                        
			SELECT DISTINCT EmployeeId, 'New Training has been assigned to you', 1, 0, GETUTCDATE()
			FROM @training_data

			---Send email-------                       
			INSERT INTO EmailDump 
			([To], [Cc], [Bcc], [Subject], Template, IsActive, CreatedBy, CreatedOn)                                         
			SELECT	DISTINCT e.GlobantEmailAddress, e.BetterMeLeaderEmail, NULL, 'Action Required : Mandatory Training Assigned To You',
					'GU_USER_ADDED', 1, 0, GETUTCDATE()
			FROM @training_data td
			INNER JOIN dbo.Employee e
				ON td.EmployeeId = e.Id

			UPDATE JRD 
			SET JRD.[Status] = 'Completed',
				JRD.UpdatedBy = @systemUserId, 
				JRD.UpdatedOn = GETUTCDATE()
			FROM JobRequestDetail AS JRD 
			JOIN @training_data AS TR 
				ON JRD.JobRequestDetailId = TR.JobRequestDetailId;
		END

		UPDATE JobRequest 
		SET [Status] = 'Completed',
			UpdatedBy = @systemUserId, 
			UpdatedOn = GETUTCDATE()
		WHERE TransactionId = @transactionId;
		
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN;
		UPDATE JobRequest 
		SET [Status] = 'Completed', 
			[ErrorDetail] = ERROR_MESSAGE(),
			UpdatedBy = @systemUserId, 
			UpdatedOn = GETUTCDATE()
		WHERE TransactionId = @transactionId;
	END CATCH
END