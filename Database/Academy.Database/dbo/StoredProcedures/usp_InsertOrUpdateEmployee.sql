CREATE PROCEDURE [dbo].[usp_InsertOrUpdateEmployee]
	@EmployeeName NVARCHAR(255) ,
	@GlobantEmailAddress NVARCHAR(255),
	@BetterMeLeaderEmail NVARCHAR(255) ,
	@Seniority NVARCHAR(255),
	@SeniorityId SMALLINT ,
	@Community NVARCHAR(255) ,
	@Client NVARCHAR(100) ,
	@Project NVARCHAR(100) ,
	@BaseLocation NVARCHAR(255) ,
	@Designation NVARCHAR(255) ,
	@Position NVARCHAR(255) ,
	@JoiningDate DATETIME2(7) = NULL,
	@MobileNo NVARCHAR(100) = NULL,
	@TotalExperience DECIMAL(5, 2) ,
	@Aging DECIMAL(5, 2) = NULL,
	@Gender NVARCHAR(50) = NULL,
	@NoOfDays SMALLINT  = NULL,
	@NotificationSendCount INT  = NULL,
	@ProjectManagerEmail NVARCHAR(255)  = NULL,
	@ProjectTL NVARCHAR(255)  = NULL,
	@ProjectTLEmailsCsv NVARCHAR(100) ,
	@ProposedLeaderEmail NVARCHAR(100) ,
	@GlobalId NVARCHAR(100) ,
	@Status NVARCHAR(100) ,
	@Image NVARCHAR(200) = '',
	@OnHoldBy INT = NULL,
	@OnHoldForProject BIT = '',
	@OtherInfo NVARCHAR(max) = '',
	@ProfileLink NVARCHAR(255) = '',
	@ResumeLink NVARCHAR(255) = '',
	@IsNewJoiner BIT ,
	@Comments NVARCHAR(1000) = '',
	@MyGrowthReminderCount SMALLINT = 0,
	@IsActive BIT = 1,
    @recordInsertOrUpdateBy INT,
    @recordInsertOrUpdateDate DATETIME2(0)
AS
BEGIN
	IF (@recordInsertOrUpdateDate IS NULL)
		SET @recordInsertOrUpdateDate = (SELECT CAST(SWITCHOFFSET(SYSDATETIMEOFFSET(), '+05:30') AS DATETIME2(0)));
	DECLARE @userExists TINYINT = (SELECT COUNT(1) FROM dbo.Employee WHERE TRIM(LOWER(GlobantEmailAddress)) = TRIM(LOWER(@GlobantEmailAddress)));
	BEGIN TRY
		IF @userExists = 0
		BEGIN -- INSERT employee
			INSERT INTO dbo.Employee 
			([EmployeeName],[GlobantEmailAddress]
			 ,[BetterMeLeaderEmail],[Seniority],[Community],[Client],[Project],[BaseLocation]
			 ,[Designation],[Position],[JoiningDate],[MobileNo],[TotalExperience],[Aging],[Gender],[NoOfDays]
			 ,[NotificationSendCount],[ProjectManagerEmail],[ProjectTL],[ProjectTLEmailsCsv],[ProposedLeaderEmail]
			 ,[GlobalId],[Status],[Image],[OnHoldBy],[OnHoldForProject],[OtherInfo],[ProfileLink],[ResumeLink]
			 ,[IsNewJoiner],[Comments],[MyGrowthReminderCount],[IsActive],[CreatedBy],[CreatedOn])
			SELECT	@EmployeeName,TRIM(LOWER(@GlobantEmailAddress))
					,@BetterMeLeaderEmail,@Seniority,@Community,@Client,@Project,@BaseLocation
					,@Designation,@Position,@JoiningDate,@MobileNo,@TotalExperience,@Aging,@Gender,@NoOfDays
					,@NotificationSendCount,@ProjectManagerEmail,@ProjectTL,@ProjectTLEmailsCsv,@ProposedLeaderEmail
					,@GlobalId,@Status,@Image,@OnHoldBy,@OnHoldForProject,@OtherInfo,@ProfileLink,@ResumeLink
					,@IsNewJoiner,@Comments,@MyGrowthReminderCount,@IsActive,@recordInsertOrUpdateBy,@recordInsertOrUpdateDate
		END
		ELSE 
		BEGIN -- UPDATE employee
			UPDATE dbo.Employee 
			SET [EmployeeName]=@EmployeeName,[BetterMeLeaderEmail]=@BetterMeLeaderEmail,[Seniority] = @Seniority 
				,[Community]=@Community,[Client]=@Client,[Project]=@Project,[BaseLocation]=@BaseLocation
				,[Designation]=@Designation,[Position]=@Position,[MobileNo]=@MobileNo,[TotalExperience]=@TotalExperience
				,[Aging]=@Aging,[Gender]=@Gender,[NoOfDays]=@NoOfDays,[NotificationSendCount]=@NotificationSendCount
				,[ProjectManagerEmail]=@ProjectManagerEmail,[ProjectTL]=@ProjectTL,[ProjectTLEmailsCsv]=@ProjectTLEmailsCsv
				,[ProposedLeaderEmail]=@ProposedLeaderEmail,[Status]=@Status,[Image]=@Image,[OnHoldBy]=@OnHoldBy
				,[OnHoldForProject]=@OnHoldForProject,[OtherInfo]=@OtherInfo,[ProfileLink]=@ProfileLink,[ResumeLink]=@ResumeLink
				,[Comments]=@Comments,[MyGrowthReminderCount]=@MyGrowthReminderCount,[IsActive]=@IsActive
				,[UpdatedBy]=@recordInsertOrUpdateBy,[UpdatedOn]=@recordInsertOrUpdateDate
			WHERE TRIM(LOWER(GlobantEmailAddress)) = TRIM(LOWER(@GlobantEmailAddress));
		END
		
		SELECT 'Success' AS Result, Id AS EmployeeId 
		FROM dbo.Employee 
		WHERE TRIM(LOWER(GlobantEmailAddress)) = TRIM(LOWER(@GlobantEmailAddress));
	END TRY
	BEGIN CATCH
		SELECT 'Error' AS Result, ERROR_MESSAGE() AS ErrorMessage;
	END CATCH
END
