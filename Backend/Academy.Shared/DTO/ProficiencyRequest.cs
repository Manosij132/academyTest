namespace Academy.Shared.DTO
{
    public class ProficiencyRequest
    {
        public int EmployeeId { get; set; }
        public int LoggedInUserId { get; set; }
        public short SkillId { get; set; }
        public byte CurrentProficiency { get; set; }
        public byte CurrentKnowledge { get; set; }
        public byte NewProficiency { get; set; }
        public byte NewKnowledge { get; set; }
    }
}
