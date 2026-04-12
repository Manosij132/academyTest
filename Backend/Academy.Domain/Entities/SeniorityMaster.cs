namespace Academy.Domain.Entities
{
    public class SeniorityMaster: BaseEntity
    {
        public short SeniorityId { get; set; }
        public short SeniorityLevel { get; set; }
        public string SeniorityName { get; set; }
    }
}
