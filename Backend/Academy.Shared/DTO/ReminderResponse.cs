namespace Academy.Shared.DTO
{
    public class ReminderResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeEmail { get; set; }
        public int ReminderCount { get; set; } = 0;
        public DateTime? LastReminder { get; set; }
    }
}
