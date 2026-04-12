namespace Academy.Shared.DTO
{
    public class SkillDto
    {
        public short? SkillId { get; set; }
        public string SkillName { get; set; }
        public string SkillDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public short? CategoryId { get; set; }
        public bool? Mandatory { get; set; }
        public string Grouping { get; set; }
        public string Specification { get; set; }
    }
}
