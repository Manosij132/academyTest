using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IActivityService
    {
        Task<Result<List<EmployeeActivity>>> FetchActivityById(int employeeId);
        Task<Result<int>> InsertOrUpdateEmployeeActivities(EmployeeActivityMapRequest request);
        Task<Result<int>> BulkInsertActivities(List<EmployeeActivityMapRequest> employeeActivities);
        Task<Result<List<DojoActivity>>> FetchAllActivities(string employeeEmails);
    }
}