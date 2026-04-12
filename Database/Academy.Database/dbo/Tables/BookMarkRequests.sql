CREATE TABLE [dbo].[BookMarkRequests] (
    [BookMarkId] INT NOT NULL,
    [BookMarkName] NVARCHAR (255) NULL,
    [TDC] NVARCHAR (MAX) NULL,
    [Community] NVARCHAR (MAX) NULL,
    [Trainings] NVARCHAR (MAX) NULL,
    [Seniorities] NVARCHAR (MAX) NULL,
    [Projects] NVARCHAR (MAX) NULL,
    [Statuses] NVARCHAR (MAX) NULL,
    [ReportType] NVARCHAR (100) NULL,
    [ConfigureColumns] NVARCHAR (MAX) NULL,
    [GroupByColumns] NVARCHAR (MAX) NULL,	
    PRIMARY KEY CLUSTERED ([BookMarkId] ASC)
);