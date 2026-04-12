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
    public class SkillTrainingController : ApiBaseController
    {
        public readonly ISkillAndTrainingService _skillAndTrainingService;
        public SkillTrainingController(ISkillAndTrainingService skillAndTrainingService)
        {
            _skillAndTrainingService = skillAndTrainingService;
        }

        [HttpGet("Fetch/skills")]
        public async Task<IActionResult> FetchSkills()
        {
            var result = await _skillAndTrainingService.FetchSkills();
            AcademyResponse<List<SkillDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpPost("Insert/skills")]
        public async Task<IActionResult> InsertOrUpdateSkills([FromBody] SkillDto request)
        {
            var result = await _skillAndTrainingService.InsertOrUpdateSkill(request);
            AcademyResponse<int> response = new()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Fetch/SkillTrainingsMetaData/{ecosystemId}")]
        public async Task<IActionResult> FetchSkillTrainingsMetaData([FromRoute] short ecosystemId)
        {
            var result = await _skillAndTrainingService.FetchSkillTrainingsMetaData(ecosystemId);
            AcademyResponse<List<TrainingsGroupedBySkill>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Fetch/FetchSkillEndorsement/{ecosystemId}/{account?}/{commaSeperatedEmployeeIds?}")]
        public async Task<IActionResult> FetchFetchSkillEndorsement([FromRoute] short ecosystemId, string account, string commaSeperatedEmployeeIds)
        {
            var result = await _skillAndTrainingService.FetchSkillEndorsement(ecosystemId, account, commaSeperatedEmployeeIds);
            AcademyResponse<List<BaseSkillEndorsementResponse>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }


        [HttpPost("Insert/Trainings")]
        public async Task<IActionResult> InsertTrainingsAndMapping([FromBody] ManageTrainingDto request)
        {
            var result = await _skillAndTrainingService.CreateTrainings(request);
            AcademyResponse<string> response = new()
            {
                Data = result.IsSuccess ? result.Value : string.Empty,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Fetch/Categories")]
        public async Task<IActionResult> FetchCategories()
        {
            var result = await _skillAndTrainingService.FetchCategory();
            AcademyResponse<List<CategoryDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpPost("Insert/CategoryOrSubCategory")]
        public async Task<IActionResult> CreateCategoryOrSubCategory([FromBody] SubCategoryDto request)
        {
            var result = await _skillAndTrainingService.CreateCategoryOrSubCategory(request);
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
