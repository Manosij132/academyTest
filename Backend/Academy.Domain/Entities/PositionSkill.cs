
namespace Academy.Domain.Entities
{
    public class PositionSkill
    {
        public int Id { get; set; }
        public decimal OpenPositionId { get; set; }
        public int? ExternalSkillId { get; set; }
        public string SkillName { get; set; }
        public decimal? SkillValue { get; set; } 
        public string Importance { get; set; }
    }
}
