using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IEmployeeService
    {
        Task<Result<IList<Employee>>> FetchByOptions(DataRequestOptions dataRequestOptions);
        Task<Result<Employee>> FetchById(int globerId);
        Task<Result<IList<Employee>>> FetchAll();
        Task<Result<List<EmployeeResponse>>> FetchByEcosystemAndEmailStartsWith(string startsWith, int ecosystemId,string account);
        Task<Result<List<EmployeeResponse>>> FetchByGexLeaderNameStartsWith(string startsWith);
        Task<Result<List<EmployeeRoleDto>>> Search(string keyword);
        Task<Result<List<string>>> FetchAllTdc();
        Task<Result<DojoCommunityCountryListResponse>> FetchAllTdcCommunityDojo();
        Task<Result<List<string>>> FetchAllCommunity();
        Task<Result<List<string>>> FetchAllAccount();
        Task<Result<List<ActivityMasterDto>>> FetchAllActivities();
        Task<List<string>> FetchAllProject(CancellationToken cancellationToken = default);
        Task<Result<List<LearningPathDto>>> FetchAllAreaPaths();
        Task<Employee> FetchByEmail(string email);
        IEnumerable<Employee> FetchByName(string name);
        Task<Result<List<string>>> FetchAllClients();
        Task<List<string>> FetchAllProjectBasedonClient(string[] Client, CancellationToken cancellationToken = default);
        Task<Result<List<string>>> FetchAllAiStudio();
        Task<Result<List<AiStudioAccount>>> FetchAllAiStudioAccount();
        Task<List<Employee>> FetchByEmails(List<string> emails);
    }
}