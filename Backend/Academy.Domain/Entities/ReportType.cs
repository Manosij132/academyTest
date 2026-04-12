namespace Academy.Domain.Entities
{
     
    public class ReportType : BaseEntity
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public string StoredProcName { get; set; } = string.Empty;
    }
}
