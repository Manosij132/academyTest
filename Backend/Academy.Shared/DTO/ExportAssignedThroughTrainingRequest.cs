namespace Academy.Shared.DTO
{
    public class ExportAssignedThroughTrainingRequest
    {
        public List<AssignedThroughTrainingInfo> DetailedReport { get; set; }
        public AssignedThroughTrainingReportFilter Filter { get; set; }
        public List<AssignedThroughTrainingCount> ReportCounts { get; set; }
    }
}
