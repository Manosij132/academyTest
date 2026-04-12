using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class JobsController : ControllerBase
    {
        private readonly IJobsService _jobsService;

        public JobsController(IJobsService jobsService)
        {
            _jobsService = jobsService;
        }

        [HttpGet("Fetch/jobs")]
        public async Task<IActionResult> GetAlljobs()
        {
            var result = await _jobsService.GetAllJobs();
            AcademyResponse<List<ScheduleJobsDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
    }
}