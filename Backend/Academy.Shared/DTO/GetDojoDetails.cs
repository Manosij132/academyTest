namespace Academy.Shared.DTO
{
    public class GetDojoDetailsResponse
    {
        public List<DojoDetailInfo> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public class DojoDetailInfo
    {
        public int? DojoDetailId { get; set; } 
        public int? EmployeeId { get; set; }   
        public string EmployeeName { get; set; } 
        public string GlobantEmailAddress { get; set; } 
        public DateTime? DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }   
        public string DojoGexLeaderEmail { get; set; } 
        public bool? AssignedThroughTraining { get; set; }
        public string Comments { get; set; } 
        public int? TicketNumber { get; set; }
        public string Community { get; set; }
        public string AiStudio { get; set; }
        public string Account { get; set; }
    }
}
