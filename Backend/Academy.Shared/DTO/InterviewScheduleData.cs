namespace Academy.Shared.DTO
{
    public class InterviewScheduleData
    {
        public int PanelId { get; set; }
        public string EmailId { get; set; } = string.Empty;
        public string Panel { get; set; } = string.Empty;
        public string PrimaryPanel { get; set; } = string.Empty;
        public string UpToSeniority { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
        public List<AllPanelSlots> Slots { get; set; }
    }
}
