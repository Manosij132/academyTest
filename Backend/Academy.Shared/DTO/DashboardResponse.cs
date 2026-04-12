namespace Academy.Shared.DTO
{
    public class DashboardResponse
    {
        public EmployeeResponse Employee { get; set; } = new();
        public List<TrainingResponse> Trainings { get; set; } = new();
    }
}
