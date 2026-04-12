CREATE TABLE InterviewQuestions (
    QuestionId SMALLINT PRIMARY KEY IDENTITY(1,1),
    Status NVARCHAR(50),
    SkillId SMALLINT,          -- SkillMaster Foreign Key should refer SkillMaster Table
    Section NVARCHAR(50),  -- Section is not a table so its a varchar here
    Question NVARCHAR(MAX) NOT NULL,
    ScoreGuideLine NVARCHAR(MAX),
    CoachGuideLine NVARCHAR(MAX),
    CONSTRAINT FK_InterviewQuestions_Skill
        FOREIGN KEY (SkillId) REFERENCES SkillMaster(SkillId)
);