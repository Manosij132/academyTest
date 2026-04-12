namespace Academy.Shared.DTO
{
    public class ExportDojoActivityRequest
    {
        public List<DojoActivityReport> DetailedReport { get; set; }
        public List<DojoActivityCount> ActivitySummary { get; set; }
        public DojoActivityReportFilter Filter { get; set; }
        public List<DojoEngagementCount> EngagementCounts { get; set; }
        public List<DojoActivityReport> NonAssignableItems { get; set; }
    }
}
