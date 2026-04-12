namespace Academy.Shared.DTO
{
    public class UpdateDojoDetailTrainingInfo
    {
        public int DojoDetailId { get; set; }
        public bool AssignedThroughTraining { get; set; }
        public string Comments { get; set; }
        public int TicketNumber { get; set; }
    }
}
