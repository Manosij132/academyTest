CREATE TABLE InterviewSkillSet (
    id INT IDENTITY(1,1) PRIMARY KEY,
    interview_id UNIQUEIDENTIFIER NOT NULL,
    skill_id smallint NOT NULL,
    CONSTRAINT fk_interview FOREIGN KEY (interview_id)
        REFERENCES interview (interview_id),
    CONSTRAINT fk_skill FOREIGN KEY (skill_id)
        REFERENCES skillmaster (skillid)
);