namespace Academy.Shared.DTO
{
    public class AssignedThroughTrainingReportResponse
    {
        public List<AssignedThroughTrainingInfo> Items { get; set; } = [];
        public List<AssignedThroughTrainingInfo> ExportItems { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int AssignedThroughTrainingCount { get; set; } = 0;
        public int NotAssignedThroughTrainingCount { get; set; } = 0;
    }

    public class AssignedThroughTrainingInfo
    {
        public int? DojoDetailId { get; set; } 
        public int? EmployeeId { get; set; }   
        public string EmployeeName { get; set; } 
        public string GlobantEmailAddress { get; set; } 
        public DateTime? DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }   
       // public string DojoGexLeaderEmail { get; set; } 
        public bool? AssignedThroughTraining { get; set; }
        public string Comments { get; set; } 
        public int? TicketNumber { get; set; }
        public string Community { get; set; }
        public string AiStudio { get; set; }
        public string Account { get; set; }
    }

    public class AssignedThroughTrainingCount
    {
        public string Name { get; set; }
        public int Count { get; set; } = 0;
    }
}
