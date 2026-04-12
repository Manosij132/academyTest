namespace Academy.Shared.DTO
{
    public class InterviewPanelFilterModelRequest
    {
        public List<string>? TDCs { get; set; }
        public List<int>? Communities { get; set; }
        public List<int>? Seniorities { get; set; }
        public List<string>? PanelTypes { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public bool AvailableSlots { get; set; }
        public bool IsDeficit { get; set; }
    }
}
