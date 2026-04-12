namespace Academy.Domain.Entities
{
    public class DojoDetail : BaseEntity
    {
        public int DojoDetailId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }
        public string DojoGexLeaderEmail { get; set; }
        public bool? AssignedThroughTraining { get; set; }
        public string Comments { get; set; }
        public int? TicketNumber { get; set; }
    }
}
