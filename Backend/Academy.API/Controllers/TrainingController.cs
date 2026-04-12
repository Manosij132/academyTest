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
    public class TrainingController : ApiBaseController
    {
        public readonly ITrainingService _trainingService; 
        public TrainingController(ITrainingService trainingService)
        {
            _trainingService = trainingService; 
        }

        [HttpPost("FetchTrainingList")]
        public async Task<IActionResult> FetchTrainingList([FromBody] FetchTrainingListRequest request)
        {
            var result = await _trainingService.FetchTrainingList(request);
            AcademyResponse<FetchTrainingListResponse> response = new()
            {
                Data = result.IsSuccess ? result.Value : null,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("UpdateTraining")]
        public async Task<IActionResult> UpdateTraining([FromBody] UpdateTrainingRequest request)
        {
            var result = await _trainingService.UpdateTraining(request);
            AcademyResponse<int> response = new()
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