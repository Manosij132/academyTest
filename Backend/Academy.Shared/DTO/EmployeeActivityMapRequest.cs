namespace Academy.Shared.DTO
{
    public class EmployeeActivityMapRequest
    {
        public int? EmployeeActivityId { get; set; }
        public int EmployeeId { get; set; }
        public short ActivityId { get; set; }
        public string ActivitySource { get; set; }
        public string ActivityDetail { get; set; }
        public string? Comments { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Status { get; set; }
        public  string Account { get; set; }
        public string Action { get; set; }
    }

}
