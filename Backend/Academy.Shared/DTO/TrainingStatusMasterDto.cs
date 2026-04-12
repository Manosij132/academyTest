namespace Academy.Shared.DTO
{
    public class TrainingStatusMasterDto
    {
        public byte TrainingStatusId { get; set; }
        public string TrainingStatusName { get; set; }
        public bool IsActive { get; set; }
    }
    public class TrainingStatusListDto
    {
        public int TrainingStatusId { get; set; }
        public string TrainingStatusName { get; set; }
    }
}
