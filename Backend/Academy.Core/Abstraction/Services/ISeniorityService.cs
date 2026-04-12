using Academy.Shared.DTO;
using Academy.Shared.Response;
using System.Threading.Tasks;

namespace Academy.Core.Abstraction.Services
{
    public interface ISeniorityService
    {
        Task<Result<List<SeniorityDto>>> Fetch();
        Task<Result<string>> Insert(SeniorityDto request);
        Task<Result<string>> Modify(SeniorityDto request);
        Task<Result<string>> Deactivate(short seniorityId);
    }
}
