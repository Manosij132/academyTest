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
    public class GXLeaderController : ApiBaseController
    {
        private readonly IGXLeaderService _gxleaderService;

        private readonly IConfiguration _configuration;
        private readonly ILogger<GXLeaderController> _logger;
        public GXLeaderController(IGXLeaderService gxleaderService, IConfiguration configuration, ILogger<GXLeaderController> logger)
        {
            _gxleaderService = gxleaderService;
            _configuration = configuration;
            _logger = logger;
        }
        
        [HttpGet]
        [Route("GetAllGXLeader")]
        public async Task<IActionResult> GetAllGXLeader(string community)
        {
            _logger.LogInformation("GetAllGXLeader Called");
            var result = await _gxleaderService.GetGXAllLeader(community);

            var response = new AcademyResponse<List<LeaderModel>>
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("DeleteGXLeader")]
        public async Task<IActionResult> DeleteGXLeader(UpdateGxLeader request)
        {
            var result = await _gxleaderService.DeleteGXLeader(request);

            var response = new AcademyResponse<int>
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }


    }
}