namespace Academy.Domain.Entities
{
    public class BookMarkTemplates : BaseEntity
    {
        public int BookMarkId { get; set; }
        public string BookMarkName { get; set; }
        public string TDC { get; set; }
        public string Communities { get; set; }
        public string Trainings { get; set; }
        public string Projects { get; set; }
        public string Statuses { get; set; }
        public int ReportType { get; set; }
        public string Seniorities { get; set; }
        public string ConfigureColumns { get; set; }
        public string GroupByColumns { get; set; }
        public string To {  get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AreaPaths { get; set; }
        public string PrimaryActivities { get; set; }
        public string ActivitieOptions { get; set; }
        public string EmployeeId { get; set; }
        public string? DateTypeFilter { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? Client {  get; set; }

    }
}
