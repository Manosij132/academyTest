namespace Academy.Shared.DTO
{
    public class EmployeeTrainingRecord
    {
        public int EmployeeId { get; set; }
        public string GlobantEmailAddress { get; set; }
        public string Seniority { get; set; }
        public string SkillName { get; set; }
        public string TrainingName { get; set; }
        public string TrainingUrl { get; set; }
        public int TrainingStatusId { get; set; }
        public string TrainingStatus { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? ActualEndDate { get; set; } // Nullable if the training may not have ended
        public DateTime? ExpectedEndDate { get; set; } // Nullable if the expected date is not set
    }
}
