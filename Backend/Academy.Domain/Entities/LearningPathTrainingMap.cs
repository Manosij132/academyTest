
namespace Academy.Domain.Entities
{
    public class LearningPathTrainingMap : BaseEntity
    {
        public int SeniorityId {  get; set; }
        public int TrainingId {  get; set; }
        public int LearningPathId {  get; set; }
        public bool IsActive {  get; set; }


    }
}
