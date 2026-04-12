using Academy.Shared.DTO;
using Academy.Shared.Response;
using static Academy.Core.Services.DojoService;

namespace Academy.Core.Abstraction.Services
{
    public interface IDojoService
    {
        Task<Result<GetDojoDetailsResponse>> GetFilteredPagedDojoDetails(FetchDojoGlobarsRequest request);
        Task<Result<int>> UpdateDojoDetailTrainingInfo(List<UpdateDojoDetailTrainingInfo> request);
        Task<Result<int>> UpdateDojoEndtDate(List<UpdateDojoEndDate> request);
        Task<Result<int>> UpdateGXLeader(UpdateGxLeader request);
        Task<Result<List<DojoActivity>>> FetchDojoActivityByIds(List<string> employeeEmails);
        Task<Result<List<int>>> GetMenteesByEmail(string GXLeaderEmail);
        Task<Result<int>> UpdateMentees(UpdateMentees request);
    }
}
