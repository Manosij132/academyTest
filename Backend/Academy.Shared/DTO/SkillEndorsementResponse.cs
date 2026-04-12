namespace Academy.Shared.DTO
{
    public class BaseSkillEndorsementResponse
    {
        public int EmployeeId { get; set; }
        public short SeniorityId { get; set; }
        public short SkillId { get; set; }
        public byte CurrentProficiency { get; set; }
        public byte CurrentKnowledge { get; set; }
    }
    public class SkillEndorsementResponse : BaseSkillEndorsementResponse
    {
        public int SkillEndorsementId { get; set; }
        public int EcosystemId { get; set; }
        public byte ExpectedProficiency { get; set; }
        public byte ExpectedKnowledge { get; set; }
        public string SkillName { get; set; }
        public bool IsMVP { get; set; }
    }
}
