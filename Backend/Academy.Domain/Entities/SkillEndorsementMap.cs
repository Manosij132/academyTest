namespace Academy.Domain.Entities
{
    public class SkillEndorsementMap : BaseEntity
    {
        public int SkillEndorsementId { get; set; }
        public int EmployeeId { get; set; }
        public short SkillId { get; set; }
        public byte CurrentProficiency { get; set; }
        public byte CurrentKnowledge { get; set; }
    }
}
