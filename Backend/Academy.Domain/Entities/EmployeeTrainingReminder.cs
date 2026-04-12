namespace Academy.Domain.Entities
{
    public class EmployeeTrainingReminder : BaseEntity
    {
        public int EmployeeTrainingReminderId { get; set; }
        public int EmployeeId { get; set; }
        public int EmployeeTrainingId {  get; set; }
        public short ReminderCount { get; set; }
    }
}
