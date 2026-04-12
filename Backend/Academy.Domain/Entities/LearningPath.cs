

namespace Academy.Domain.Entities
{
    public class LearningPath : BaseEntity
    {
        public int LearningPathId {  get; set; }
        public string LearningPathName { get; set; }
        public string LearningPathDescription {  get; set; }
        public string LearningPathUrl {  get; set; }

        public string SeniorityLevel {  get; set; }

    }
}
