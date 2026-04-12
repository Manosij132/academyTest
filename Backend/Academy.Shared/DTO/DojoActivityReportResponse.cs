namespace Academy.Shared.DTO
{
    public class DojoActivityReportResponse
    {
        public List<DojoActivityReport> Items { get; set; } = [];
        public List<DojoActivityReport> ExportItems { get; set; } = [];
        public List<DojoActivityCount> ActivityCounts { get; set; } = [];
        public List<DojoActivityReport> NonAssignableItems { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int DojoEngagedCount { get; set; } = 0;
        public int CurrentDojoCount { get; set; } = 0;
        public int DojoNotEngagedCount { get; set; } = 0;
        public int NonAssignable { get; set; } = 0;
    }
    public class DojoActivityReport
    {
        public string GlobantEmailAddress { get; set; }
        public string EmployeeName { get; set; }
        public string BaseLocation { get; set; }
        public string Community { get; set; }
        public string Seniority { get; set; }
        public DateTime? DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }
        public string? ActivityName { get; set; }
        public string? ActivityDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public decimal Priority { get; set; }
        public bool IsActive { get; set; }
        public string IsEmployeeActive { get; set; }
        public string ActivityComment { get; set; }
        public string DojoProjectName { get; set; }
        public string AiStudio { get; set; }
        public string Account { get; set; }
    }

    public class DojoActivityCount
    {
        public string? ActivityName { get; set; }
        public int ActivityCount { get; set; }
    }

    public class DojoEngagementCount
    {
        public string Name { get; set; }
        public int Count { get; set; } = 0;
    }
}
