namespace Academy.Shared.DTO
{
    public class TrainingsGroupedBySkill
    {
        public int EcosystemId { get; set; }
        public short SkillId { get; set; }
        public byte ExpectedProficiency { get; set; }
        public byte ExpectedKnowledge { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public List<TrainingMasterResponse> Trainings { get; set; } = new();
    }

    public class TrainingMasterResponse
    {
        public string TrainingName { get; set; }
        public string TrainingLink { get; set; }
        public byte SeniorityId { get; set; }
        public string Seniority { get; set; }
        public int TrainingId { get; set; }
        public string TrainingDescription { get; set; }
        public short TrainingCompletionHours { get; set; }
        public bool IsMvP { get; set; } = false;
    }
}
