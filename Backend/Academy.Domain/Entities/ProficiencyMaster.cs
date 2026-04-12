namespace Academy.Domain.Entities
{
    public class ProficiencyMaster : BaseEntity
    {
        public short ProficiencyId { get; set; }
        public byte ProficiencyRating { get; set; }
        public string ProficiencyName { get; set; }
    }
}
