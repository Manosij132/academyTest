namespace Academy.Shared.DTO
{
    public class ChangeStatusRequest
    {
        public int EmployeeTrainingId { get; set; }
        public byte TrainingStatusId { get; set; }
        public int EmployeeId { get; set; }
    }
}
