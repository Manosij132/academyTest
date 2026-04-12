namespace Academy.Shared.DTO
{
    public class Training
    {
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TrainingDescription { get; set; }
        public string TrainingUrl { get; set; }
        public int TrainingCompletionHours { get; set; }
        public bool IsActive { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? IsAssignment { get; set; }
        public bool? IsPriortize { get; set; }
    }
}
