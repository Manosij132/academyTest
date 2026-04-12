namespace Academy.Shared.DTO
{
    public class DojoActivity
    {
        public int DojoDetailId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string GlobantEmailAddress { get; set; }
        public DateTime DojoStartDate { get; set; } 
        public List<string> ActivityDetail { get; set; }
        public string Comments { get; set; }
        public int? TicketNumber { get; set; }
        public string PositionTitle { get; set; }
        public string Client { get; set; }
        public string ProjectName { get; set; }
        public string Skills { get; set; }
    }
}
