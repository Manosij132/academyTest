namespace Staffing.Core.Abstraction.Models
{ 
    public class StaffRequestDto
    {
        public int RequestID { get; set; }
        public string Client { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PositionID { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? SubmitDate { get; set; }
        public string Handler { get; set; } = string.Empty;
        public string PositionNotes { get; set; } = string.Empty;
        // New editable fields
        public string DetailedStatus { get; set; } = string.Empty;
        public string MonthClosure { get; set; } = string.Empty;
        public string TicketStatus { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public sealed class StaffRequestUpdateDto
    {
        // null => do not change; non-null => update
        public string? DetailedStatus { get; set; }
        public string? MonthClosure { get; set; }
        public string? TicketStatus { get; set; }
        public string? Comments { get; set; }
    }
}
