namespace Academy.Shared.DTO
{
    public class EmployeeActivity
    {
        public int EmployeeActivityId { get; set; }
        public int EmployeeId { get; set; }
        public short ActivityId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte StatusId { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDetail { get; set; }
        public string? Comments { get; set; }
        public string? ActivitySource { get; set; }
        public string? Account { get; set; }

    }
}
