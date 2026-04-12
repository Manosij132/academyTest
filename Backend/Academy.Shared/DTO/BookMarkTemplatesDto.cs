namespace Academy.Shared.DTO
{
    public class BookMarkTemplatesDto
    {
        public int BookMarkId { get; set; }
        public string BookMarkName { get; set; }
        public List<string> TDC { get; set; }
        public List<string> Communities { get; set; }
        public List<int> Trainings { get; set; }
        public List<int> Seniorities { get; set; }
        public List<string> Projects { get; set; }
        public List<int> Statuses { get; set; }
        public int ReportType { get; set; }
        public List<int> ConfigureColumns { get; set; }
        public List<int> GroupByColumns { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public List<int> AreaPaths { get; set; }
        public List<int> PrimaryActivities { get; set; }
        public List<int> ActivityOptions { get; set; }
        public List<string> EmployeeId{ get; set; }
        public List<EmployeeRoleDto> Employees { get; set; }
        public string? DateTypeFilter { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public List<string> Client {  get; set; }
    }

    public class BookMarkTemplateListDto
    {
        public int BookMarkId { get; set; }
        public string BookMarkName { get; set; }
        public int ReportType { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
    }
}
