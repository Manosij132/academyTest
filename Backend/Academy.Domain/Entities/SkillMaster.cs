namespace Academy.Domain.Entities
{
    public class SkillMaster : BaseEntity
    {
        public short SkillId { get; set; }
        public string SkillName { get; set; }
        public string DisplayName { get; set; }
        public string SkillDescription { get; set; }
        public short? CategoryId { get; set; }
        public bool? Mandatory { get; set; }
        public string Grouping { get; set; }
        public string Specification { get; set; }
        public bool IsDefaultInGroup { get; set; } = false;
        public bool IsSkillRequiredInReport { get; set; }
    }
}
