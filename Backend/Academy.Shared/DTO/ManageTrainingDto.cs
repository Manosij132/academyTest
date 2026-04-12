namespace Academy.Shared.DTO
{
    public class ManageTrainingDto
    {
        public int ecosystemId { get; set; }
        public int skillId { get; set; }
        public int trainingId { get; set; }
        public string trainingName { get; set; }
        public string trainingDescription { get; set; }
        public string trainingUrl { get; set; }
        public int trainingCompletionHours { get; set; }
        public List<ExpectedProficiency> expectedProficiency { get; set; }
        public bool IsMvP { get; set; }
        public bool? IsPriortize { get; set; }
    }

    public class ExpectedProficiency
    {
        public byte seniorityId { get; set; }
        public byte proficiencyValue { get; set; }
        public byte knowledgeValue { get; set; }
    }

}
