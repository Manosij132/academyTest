CREATE TABLE Profile (
    ProfileId SMALLINT PRIMARY KEY IDENTITY(1,1),
    ProfileName NVARCHAR(100) NOT NULL,
    SeniorityId SMALLINT,  -- Seniority_Id should refer Seniority_Master Table  
    SkillId NVARCHAR(100) NOT NULL,      -- Skill_Id will be multiple skills so varchar
    Section NVARCHAR(50),
    CONSTRAINT FK_Profile_Seniority
        FOREIGN KEY (SeniorityId) REFERENCES SeniorityMaster(SeniorityId)
);