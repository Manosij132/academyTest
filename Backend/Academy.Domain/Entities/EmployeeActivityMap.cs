namespace Academy.Domain.Entities
{
    public class EmployeeActivityMap : BaseEntity
    {
        public int EmployeeActivityId { get; set; }
        public int EmployeeId { get; set; }
        public short ActivityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte StatusId { get; set; }
        public string ActivityDetail { get; set; }
        public string ActivitySource { get; set; }
    }
}
