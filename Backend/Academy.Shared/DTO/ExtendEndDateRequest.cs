namespace Academy.Shared.DTO
{
    public class ExtendEndDateRequest
    {
        public int EmployeeTrainingId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime NewExpectedDate { get; set; }
    }
}
