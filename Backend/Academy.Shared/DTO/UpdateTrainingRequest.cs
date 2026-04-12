namespace Academy.Shared.DTO
{
    public class UpdateTrainingRequest
    {
        public int TrainingId { get; set; }
        public bool IsPriortize { get; set; }
        public string TrainingName { get; set; }
    }
}
