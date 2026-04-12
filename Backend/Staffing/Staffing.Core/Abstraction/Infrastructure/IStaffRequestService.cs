using Staffing.Core.Abstraction.Models;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface IStaffRequestService
    {
        Task<PagedResult<StaffRequestDto>> QueryStaffRequestsByDateAsync(
            DataConnection dbConn,
            string? startDate,
            string? endDate,
            string? searchText,
            int pageNumber,
            int pageSize);

        Task<StaffRequestDto?> GetStaffRequestByIdAsync(
            DataConnection dbConn,
            int id);

        Task<int> UpdateStaffRequestEditableFieldsAsync(
            DataConnection dbConn,
            int id,
            StaffRequestUpdateDto dto);
    }
}
