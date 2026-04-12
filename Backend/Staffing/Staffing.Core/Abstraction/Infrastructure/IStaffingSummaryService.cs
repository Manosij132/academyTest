using Staffing.Core.Abstraction.Models;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface IStaffingSummaryService
    {
        Task<SummaryResponse> GetSummaryAsync(
            DataConnection dbConnection,
            DateTime? startDate,
            DateTime? endDate);

        Task<SummaryResponseNew> GetSummaryFilteredDataAsync(
            DataConnection dbConnection,
            List<string> groupNames,
            List<string> clients,
            List<string> statuses,
            DateTime? startDateFrom,
            DateTime? startDateTo);

        Task<(List<TicketFilteredData>, long)> GetTicketFilteredDataAsync(
            DataConnection dbConnection,
            GetFilteredTicketDataRequest requestParams);

        Task<TicketDropdownData> GetTicketDropdownDataAsync(
            DataConnection dbConnection);

        Task<SummaryResponse> GetClientAndDetailedStatusByAIGroupAsync(
            DataConnection dbConnection,
            List<string> groupNames,
            DateTime? startDate,
            DateTime? endDate);

        Task<SummaryResponse> GetDetailedStatusByAIGroupAndClientAsync(
            DataConnection dbConnection,
            List<string> groupNames,
            List<string> clients,
            DateTime? startDateFrom,
            DateTime? startDateTo);
    }
}

