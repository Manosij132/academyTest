using Academy.API.Helpers;
using Academy.Core.Abstraction.Services;
using Academy.Core.Services;
using Academy.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class ActivityController : ApiBaseController
    {
        private readonly IActivityService _activityService;
        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("{employeeId:int}")]
        public async Task<IActionResult> FetchAllActivity(int employeeId)
        {
            var result = await _activityService.FetchActivityById(employeeId);

            var response = new AcademyResponse<List<EmployeeActivity>>
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }


        [HttpPost("InsertOrUpdate/EmployeeActivities")]
        public async Task<IActionResult> InsertOrUpdateEmployeeActivities([FromBody] EmployeeActivityMapRequest request)
        {
            var result = await _activityService.InsertOrUpdateEmployeeActivities(request);

            var response = new AcademyResponse<int>()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost("BulkActivities")]
        public async Task<IActionResult> AssignBulkActivities([FromBody] List<EmployeeActivityMapRequest> request)
        {
            var result = await _activityService.BulkInsertActivities(request);

            var response = new AcademyResponse<int>()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("FetchActivityDetail")]
        public async Task<IActionResult> FetchActivityDetail([FromQuery] string employeeEmails)
        {
            var result = await _activityService.FetchAllActivities(employeeEmails);
            var response = ApiResponseHelper.ToAcademyResponse(result);
            return Ok(response);
        }
    }
}
