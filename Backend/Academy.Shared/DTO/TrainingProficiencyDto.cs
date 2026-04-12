namespace Academy.Shared.DTO
{
    public class SkillTrainingDto
    {
        public int EcosystemId { get; set; }
        public byte SeniorityId { get; set; }
        public short SkillId { get; set; }
        public string TrainingName { get; set; }
        public string TrainingLink { get; set; }
        public int TrainingId { get; set; }
        public byte ExpectedProficiency { get; set; }
        public byte ExpectedKnowledge { get; set; }
        public bool IsMvP { get; set; } = false;
        public string SkillName { get; set; } = string.Empty;

    }
    public class TrainingProficiencyDto : SkillTrainingDto
    {
        public string Ecosystem { get; set; }
        public bool IsActive { get; set; }
        public string TrainingDescription { get; set; }
        public short TrainingCompletionHours { get; set; }
        public int? TrainingProficiencyMapId { get; set; }
    }
}
