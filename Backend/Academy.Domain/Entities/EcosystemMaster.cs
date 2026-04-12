namespace Academy.Domain.Entities
{
    public class EcosystemMaster : BaseEntity
    {
        public int EcosystemId { get; set; }
        public string EcosystemName { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int? ParentEcosystemId { get; set; }
        public string DisplayName { get; set; }
    }
}
