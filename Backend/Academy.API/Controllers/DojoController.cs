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
    public class DojoController : ApiBaseController
    {
        private readonly IDojoService _dojoService;

        public DojoController(IDojoService dojoService)
        {
            _dojoService = dojoService;
        }

        [HttpPost]
        public async Task<IActionResult> FetchDojoGlobars([FromBody]FetchDojoGlobarsRequest request)
        {
            var result = await _dojoService.GetFilteredPagedDojoDetails(request);

            var response = new AcademyResponse<GetDojoDetailsResponse>
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost("UpdateDojoGlobarTrainingInfo")]
        public async Task<IActionResult> UpdateDojoGlobarTrainingInfo(List<UpdateDojoDetailTrainingInfo> request)
        {
            var result = await _dojoService.UpdateDojoDetailTrainingInfo(request);

            var response = new AcademyResponse<int>
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost("UpdateGlobarDojoEndDates")]
        public async Task<IActionResult> UpdateGlobarDojoStartDates(List<UpdateDojoEndDate> request)
        {
            var result = await _dojoService.UpdateDojoEndtDate(request);

            var response = new AcademyResponse<int>
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("UpdateGXLeader")]
        public async Task<IActionResult> UpdateGXLeader(UpdateGxLeader request)
        {
            var result = await _dojoService.UpdateGXLeader(request);

            var response = new AcademyResponse<int>
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("UpdateMentees")]
        public async Task<IActionResult> UpdateMentees(UpdateMentees request)
        {
            var result = await _dojoService.UpdateMentees(request);

            var response = new AcademyResponse<int>
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }


        [HttpGet]
        [Route("GetMenteesByEmail")]
        public async Task<IActionResult> GetMenteesByEmail(string GXLeaderEmail)
        {
            var result = await _dojoService.GetMenteesByEmail(GXLeaderEmail);

            var response = new AcademyResponse<List<int>>
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }      

    }
}
