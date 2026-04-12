using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Academy.Shared.Response;
using Microsoft.EntityFrameworkCore;

namespace Academy.Core.Services
{
    public class JobsService : IJobsService
    {
        private readonly IAcademyDbContext _academyDbContext;

        public JobsService(IAcademyDbContext academyDbContext)
        {
            _academyDbContext = academyDbContext;
        }

        public async Task<Result<List<ScheduleJobsDto>>> GetAllJobs()
        {
            var result = await _academyDbContext.ScheduledJobs
                .Select(x => new ScheduleJobsDto
                {
                    ScheduledJobId = x.ScheduledJobId,
                    JobName = x.JobName,
                    JobDescription = x.JobDescription,
                    JobSchedule = x.JobSchedule,
                    JobState = x.JobState,
                    IsActive = x.IsActive
                }).ToListAsync();

            return result;
        }
    }
}
