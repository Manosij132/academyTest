namespace Academy.Shared.DTO
{
    public class TrainingResponse
    {
        public int EmployeeTrainingMapId { get; set; }
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TrainingUrl { get; set; }
        public int TrainingStatusId { get; set; }
        public string TrainingStatus { get; set; }
        public double TrainingScore { get; set; }
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public DateTime? StartDate { get; set; }       
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public bool IsMvp { get; set; }
    }
}
