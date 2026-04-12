CREATE PROCEDURE [dbo].[usp_CreateEcosystemIfNotExists]
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN;

		INSERT INTO EcosystemMaster
		(EcosystemName, IsPrimary, ParentEcosystemId, DisplayName, IsActive, CreatedBy, CreatedOn)
		SELECT DISTINCT e.WorkingEcosystem, 1, NULL, e.WorkingEcosystem, 1, 0, GETUTCDATE()
		FROM Employee e
		LEFT JOIN EcosystemMaster em
			ON e.WorkingEcosystem = em.EcosystemName
		WHERE em.EcosystemId IS NULL AND e.WorkingEcosystem IS NOT NULL AND e.WorkingEcosystem != ''; 

		UPDATE e
		SET e.EcosystemId = em.EcosystemId
		FROM Employee e
		INNER JOIN EcosystemMaster em
			ON e.WorkingEcosystem = em.EcosystemName
		WHERE em.IsPrimary = 1
			AND em.IsActive = 1
			AND e.EcosystemId IS NULL

		COMMIT TRAN;
		
		SELECT '';
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN;
		SELECT CONCAT('ERROR: ', ERROR_MESSAGE());
	END CATCH
END