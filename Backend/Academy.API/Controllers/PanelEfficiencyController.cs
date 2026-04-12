using Academy.Core.Abstraction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Academy.API.Controllers
{
    //AGK API Migration

    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class PanelEfficiencyController : ApiBaseController
    {
        private readonly ICandidateProfileService _candidateProfileService;

        public PanelEfficiencyController(ICandidateProfileService candidateProfileService)
        {
            _candidateProfileService = candidateProfileService;
        }

        [HttpGet]
        [Route("GetPanelEfficiency")]
        public ActionResult GetPanelEfficiency([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            try
            {
                // Use startDate from query and today's date as endDate
                var result = _candidateProfileService.Process(pageNumber, pageSize, startDate, endDate);
                var totalCount = _candidateProfileService.GetTotalCount(startDate, endDate);

                var response = new
                {
                    Items = result,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return new JsonResult(response)
                {
                    StatusCode = 200,
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}