CREATE PROCEDURE [dbo].[usp_GetActiveDojoGlobers]
    @TDC VARCHAR(255),
    @Interval INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT emp.GlobantEmailAddress
    FROM [dbo].[Employee] emp
    INNER JOIN [dbo].[DojoDetail] dd 
        ON dd.EmployeeId = emp.Id
    INNER JOIN [dbo].[DojoProjectsConfiguration] dpc
        ON dd.DojoProjectsConfigurationId = dpc.DojoProjectsConfigurationId
    WHERE dd.IsActive = 1
      AND emp.IsActive = 1
      AND (@TDC IS NULL OR emp.TDC IN(@TDC))
      AND dd.DojoStartDate >= DATEADD(DAY, -@Interval, GETDATE())
      AND dpc.IsAssignable = 1;
END