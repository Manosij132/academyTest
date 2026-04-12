using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Net;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class DashboardController : ApiBaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProficiencyService _proficiencyService;

        public DashboardController(IDashboardService dashboardService, IProficiencyService proficiencyService)
        {
            _dashboardService = dashboardService;
            _proficiencyService = proficiencyService;
        }

        #region GET Endpoints
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> FetchDashboard([FromRoute] int employeeId)
        {
            var result = await _dashboardService.FetchDashboard(employeeId);
            AcademyResponse<DashboardResponse> response = new()
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Fetch/Proficiencies/{employeeId}")]
        public async Task<IActionResult> FetchProficienciencies([FromRoute] int employeeId)
        {
            var result = await _proficiencyService.FetchProficienciencies(employeeId);
            AcademyResponse<List<SkillEndorsementResponse>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Fetch/Comments/{employeeId}")]
        public async Task<IActionResult> FetchComments([FromRoute] int employeeId)
        {
            var result = await _dashboardService.FetchComments(employeeId, false);
            AcademyResponse<List<CommentResponse>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Fetch/LatestComments/{employeeId}")]
        public async Task<IActionResult> FetchLatestComment([FromRoute] int employeeId)
        {
            var result = await _dashboardService.FetchComments(employeeId, true);
            AcademyResponse<CommentResponse> response = new()
            {
                Data = result.IsSuccess ? (result.Value.Count > 0 ? result.Value[0] : new()) : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("Fetch/ProficiencyByEcosystemSkill/{ecosystemId}/{skillId}")]
        public async Task<IActionResult> FetchProficiencyByEcosystemSkill([FromRoute] short ecosystemId, short skillId)
        {
            var result = await _proficiencyService.FetchProficiencyByEcosystemSkill(ecosystemId, skillId);
            AcademyResponse<List<ProficiencyDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("Fetch/RequestTrackerStatus/{transactionId}")]
        public async Task<IActionResult> RequestTrackerStatus([FromRoute] string transactionId)
        {
            var result = await _dashboardService.RequestTrackerStatus(transactionId);
            AcademyResponse<Tuple<JobRequest, List<JobRequestDetail>>> response = new()
            {
                Data = result.IsSuccess ? result.Value : null,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("FetchAll/DocumentType")]
        public async Task<IActionResult> FetchAllDocumentType()
        {
            var result = await _dashboardService.FetchAllDocumentType();
            AcademyResponse<List<EmployeeDocumentType>> response = new()
            {
                Data = result.IsSuccess ? result.Value : null,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        #endregion

        #region POST Endpoints
        [HttpPost]
        public async Task<IActionResult> FetchTrackerList([FromBody] DataRequestOptions dataRequestOptions)
        {
            var result = await _dashboardService.FetchTrackerList(dataRequestOptions);
            AcademyResponse<IPagedList<Dashboard>> response = new()
            {
                Data = result.IsSuccess ? result.Value : null,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("InsertOrUpdate/Proficiency")]
        public async Task<IActionResult> InsertOrUpdateProficiency([FromBody] ProficiencyRequest request)
        {
            var result = await _proficiencyService.InsertOrUpdateEmployeeProficiency(request);
            AcademyResponse<int> response = new()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("ExtendEndDate")]
        public async Task<IActionResult> FetchComments([FromBody] ExtendEndDateRequest request)
        {
            var result = await _dashboardService.ExtendEndDate(request);
            AcademyResponse<int> response = new()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
        {
            var result = await _dashboardService.ChangeStatus(request);
            AcademyResponse<int> response = new()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("Post/Comments")]
        public async Task<IActionResult> InsertComment([FromBody] CommentRequest request)
        {
            var result = await _dashboardService.PostComment(request);
            AcademyResponse<int> response = new()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("spin/trainings")]
        public async Task<IActionResult> InitSpinTraining([FromBody] SpinTrainingRequest request)
        {
            var result = await _dashboardService.ExecuteTrainingAssignmentJob(request);
            AcademyResponse<string> response = new()
            {
                Data = result.IsSuccess ? result.Value : string.Empty,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }


        [HttpPost("UpdateDojoGxLeader")]
        public async Task<IActionResult> UpdateDojoGxLeader([FromBody] DojoGxLeadxerRequest request)
        {
            var result = await _dashboardService.UpdateDojoGxLeadxer(request);
            AcademyResponse<int> response = new()
            {
                Data = result.IsSuccess ? result.Value : 0,
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("UploadCV")]
        public async Task<IActionResult> UploadCV([FromForm] IFormFile file,  [FromForm] int employeeId, [FromForm] string community, [FromForm] int docType, [FromForm] string existingWebContentLink = null)
        {
            var token = HttpContext.Request.Headers.Authorization;
            string _existingWebContentLink = string.Empty;

            if(file == null || employeeId <= 0 || string.IsNullOrEmpty(community))
            {
                AcademyResponse<int> errorResponse = new()
                {
                    Data = 0,
                    Status = HttpStatusCode.OK,
                    Success = false,
                    Error = null
                };

                return BadRequest(errorResponse);
            }

            if (!string.IsNullOrEmpty(existingWebContentLink))
            {
                if (Uri.TryCreate(existingWebContentLink, UriKind.Absolute, out var uri))
                {
                    var query = QueryHelpers.ParseQuery(uri.Query);
                    if (query.TryGetValue("id", out var idVal))
                    {
                        _existingWebContentLink = idVal.ToString();
                    }
                }
            }

            string result = await _dashboardService.UploadEmployeeCV(file, employeeId, community, docType, _existingWebContentLink);

            AcademyResponse<int> response = new()
            {
                Data = result.Equals("upload", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
                Status = HttpStatusCode.OK,
                Success = result.Equals("upload", StringComparison.OrdinalIgnoreCase),
                Error = null
            };

            return Ok(response);
        }
        #endregion
    }
}
