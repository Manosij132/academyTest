namespace Academy.Shared.DTO
{
    public class DashboardFilterModel
    {
        public List<string>? TDCs { get; set; }
        public List<int>? Communities { get; set; }
        public List<int>? Seniorities { get; set; }
        public List<string>? PanelTypes { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? SearchTerm { get; set; }
    }
}
