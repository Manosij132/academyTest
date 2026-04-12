namespace Academy.Domain.Entities
{
    public class EmployeeTrainingMap: BaseEntity
    {
        public int EmployeeTrainingId { get; set; }
        public int EmployeeId { get; set; }
        public short SkillId { get; set; }
        public int TrainingId { get; set; }
        public byte TrainingStatusId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public short? TrainingTimeSeniorityId { get; set; }
        public string TrainingTimeAccount { get; set; }
        public string TraingAssignmentSrc { get; set; } = "GLOBANT";
        public bool EmailSent { get; set; } = false;
    }
}
