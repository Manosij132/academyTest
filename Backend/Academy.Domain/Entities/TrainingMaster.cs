namespace Academy.Domain.Entities
{
    public class TrainingMaster : BaseEntity
    {
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TrainingDescription { get; set; }
        public string TrainingUrl { get; set; }
        public short TrainingCompletionHours { get; set; }
        public bool? IsPriortize { get; set; }
    }
}
