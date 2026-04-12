CREATE TABLE InterviewDetail 
(   -- Table with name Interview already exist
    InterviewId SMALLINT PRIMARY KEY IDENTITY(1,1),
    InterviewType NVARCHAR(50),
    ProfileId SMALLINT, -- Profile_Id Foreign Key should refer Profile Table
    Status NVARCHAR(50),
    SectionStatus NVARCHAR(MAX),
    InterviewTime datetime2,
    Candidate NVARCHAR(100),    -- Is this a key of Candidate_Evaluation Table ?
    InterviewCode NVARCHAR(50),
    CONSTRAINT FK_InterviewDetail_Profile FOREIGN KEY (ProfileId) REFERENCES [dbo].[Profile](ProfileId)
);