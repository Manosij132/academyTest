using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IGXLeaderService
    {
        Task<Result<List<LeaderModel>>> GetGXAllLeader(string community);

        Task<Result<int>> DeleteGXLeader(UpdateGxLeader request);
    }
}