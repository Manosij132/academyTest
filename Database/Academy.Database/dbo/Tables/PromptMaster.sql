CREATE TABLE PromptMaster (
    PromptId SMALLINT PRIMARY KEY IDENTITY(1,1),
    Prompt NVARCHAR(MAX) NOT NULL,
    Version NVARCHAR(50),
    Usage NVARCHAR(100),
    ModelId SMALLINT,  -- ModelId should refer ModelMaster Table for PromptMaster Table
    ReasonForChange NVARCHAR(500)
);