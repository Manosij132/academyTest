using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface ITrainingService
    {
        Task<Result<FetchTrainingListResponse>> FetchTrainingList(FetchTrainingListRequest request);

        Task<Result<int>> UpdateTraining(UpdateTrainingRequest request);
    }
}
