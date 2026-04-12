using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IJobsService
    {
        Task<Result<List<ScheduleJobsDto>>> GetAllJobs();
    }
}
