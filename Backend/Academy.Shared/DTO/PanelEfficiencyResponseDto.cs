namespace Academy.Shared.DTO
{
    public class PanelEfficiencyResponseDto
    {
        public string? PanelName { get; set; }
        public string? PanelType { get; set; }
        public int L1Conducted { get; set; }
        public int L1Selected { get; set; }
        public int GKConducted { get; set; }
        public int GKSelected { get; set; }
        public double Efficiency { get; set; }
        public double CountwiseEfficiency { get; set; }
        public string? TDC { get; set; }
        public string? Community { get; set; }
        public string? Seniority { get; set; }
    }
}
