CREATE TABLE ModelScoring (
    ModelScoringId SMALLINT PRIMARY KEY IDENTITY(1,1),
    InterviewId SMALLINT,  -- InterviewId should refer InterviewDetail Table for ModelScoring
    ModelId SMALLINT,   -- ModelId should refer ModelMaster Table for ModelScoring
    PromptId SMALLINT,            -- PromptId should refer PromptMaster Table for ModelScoring
    ModelScore SMALLINT,
    ModelComments NVARCHAR(500),
    ManualOverrideScore FLOAT,
    ManualOverrideComments NVARCHAR(500),
    CONSTRAINT FK_ModelScoring_Model
        FOREIGN KEY (ModelId) REFERENCES ModelMaster(ModelId),
    CONSTRAINT FK_ModelScoring_Prompt
        FOREIGN KEY (PromptId) REFERENCES PromptMaster(PromptId),
    CONSTRAINT FK_ModelScoring_InterviewDetail
        FOREIGN KEY (InterviewId) REFERENCES InterviewDetail(InterviewId)
);
