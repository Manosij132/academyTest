CREATE TYPE [dbo].[EmployeeActivityMapType] AS TABLE (
    EmployeeId INT NOT NULL,
    ActivityId SMALLINT NOT NULL,
    ActivitySource VARCHAR(255) NULL,
    ActivityDetail VARCHAR(255) NULL, -- Matches string, allow NULL if it can be empty
    StartDate DATETIME NULL,
    EndDate DATETIME NULL,          -- Matches DateTime?, allow NULL
	Account VARCHAR(500) NULL
);
GO