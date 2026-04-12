namespace Staffing.Core.Abstraction.Models
{
    public sealed class GroupCount
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int GroupNameCount { get; set; }
    }
    public sealed class ClientCount
    {
        public int Id { get; set; }
        public string Client { get; set; } = string.Empty;
        public int ClientCountValue { get; set; }
    }
    public sealed class StatusCount
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int StatusNameCount { get; set; }
    }
    public sealed class SummaryResponse
    {
        public List<GroupCount> AIStudioGroups { get; set; } = new List<GroupCount>();
        public List<ClientCount> Clients { get; set; } = new List<ClientCount>();
        public List<StatusCount> DetailedStatuses { get; set; } = new List<StatusCount>();
    }
    public class SummaryData
    {
        public string TicketStatus { get; set; }
        public string Client { get; set; } // Client
        public Dictionary<string, int> MonthCounts { get; set; } = new();
    }
    public class SummaryResponseNew
    {
        public List<SummaryData> SummaryData { get; set; } = new();
    }
    public class TicketFilteredData
    {
        public string DetailedStatus { get; set; }
        public int RequestID { get; set; }
        public string Client { get; set; }
        public string MonthClosure { get; set; }
        public string TicketStatus { get; set; }
        public string Comments { get; set; }
    }
    public class TicketDropdownData
    {
        public List<string> DetailedStatus { get; set; } = new List<string>();
        public List<string> MonthClosure { get; set; } = new List<string>();
        public List<string> TicketStatus { get; set; } = new List<string>();
    }
    public class SummaryFilterRequest
    {
        public List<string> GroupNames { get; set; }
        public List<string> Clients { get; set; }
        public List<string> Statuses { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
    public sealed class GroupClientFilterRequest
    {
        public List<string> GroupNames { get; set; } = new();
        public List<string> Clients { get; set; } = new();
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
    public class GetFilteredTicketDataRequest
    {
        public List<string>? GroupNames { get; set; }
        public List<string>? DetailedStatuses { get; set; }
        public List<string>? Client { get; set; }
        public List<string>? TicketStatus { get; set; }
        public List<string>? MonthClosure { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

}
