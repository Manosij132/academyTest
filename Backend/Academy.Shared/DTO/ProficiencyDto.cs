namespace Academy.Shared.DTO
{
    public class ProficiencyDto
    {
        public short ProficiencyId { get; set; }
        public byte ProficiencyLevel { get; set; }
        public byte KnowledgeLevel { get; set; }
        public string ProficiencyName { get; set; }
        public byte SeniorityId { get; set; }
        public string SeniorityName { get; set; }
        public bool IsActive { get; set; }
        public bool IsMVP { get; set; }
    }
}
