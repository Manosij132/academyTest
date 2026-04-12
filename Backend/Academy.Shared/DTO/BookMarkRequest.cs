namespace Academy.Shared.DTO
{
    public class BookMarkRequest
    {
        public int BookMarkId { get; set; }
        public string BookMarkName { get; set; }
        public List<string> TDC { get; set; }
        public List<string> Community { get; set; }
        public List<int> Trainings { get; set; }
        public List<int> Seniorities { get; set; }
        public List<string> Projects { get; set; }
        public List<int> Statuses { get; set; }
        public int ReportType { get; set; }
        public List<int> SelectColumns { get; set; }
        public List<int> GroupByColumns { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public List<int> AreaPaths { get; set; }        
        public List<int> PrimaryActivities { get; set; }
        public List<int> activityOptions { get; set; }
        public List<int> EmployeeId { get; set; }
        public string? DateTypeFilter { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public List<string> Client { get; set; }
    }

    public class ReportEmailRequest
    {
        public int BookMarkId { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public bool IsDataMore { get; set; }
    }

    public static class ReportTypeName
    {
        public static string ReportName { get; set; }
    }
}
