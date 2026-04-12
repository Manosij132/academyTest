namespace Academy.Domain.Entities
{
    public class TrainingProficiencyMap: BaseEntity
    {
        public int TrainingProficiencyId { get; set; }
        public int EcosystemId { get; set; }
        public byte SeniorityId { get; set; }
        public short SkillId { get; set; }
        public int TrainingId { get; set; }
        public byte ExpectedProficiency { get; set; }
        public byte ExpectedKnowledge { get; set; }
        public bool IsMVP { get; set; }
    }
}
