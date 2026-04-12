namespace Academy.Shared.DTO
{
    public class DojoGxLeadxerRequest
    {
        public int? DojoDetailId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }
        public string DojoGexLeaderEmail { get; set; }
        public string DojoGexGlobarEmail { get; set; }
    }
}
