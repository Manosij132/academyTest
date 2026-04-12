using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Academy.Shared.Extensions;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class MasterController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IRoleService _roleService;
        private readonly IEcosystemService _ecosystemService;
        private readonly ISeniorityService _seniorityService;
        private readonly IProficiencyService _proficiencyService;
        private readonly ISkillAndTrainingService _trainingService;
        public MasterController(IEmployeeService employeeService, IRoleService roleService, IEcosystemService ecosystemService, ISeniorityService seniorityService,
            IProficiencyService proficiencyService, ISkillAndTrainingService trainingService)
        {
            _employeeService = employeeService;
            _roleService = roleService;
            _ecosystemService = ecosystemService;
            _seniorityService = seniorityService;
            _proficiencyService = proficiencyService;
            _trainingService = trainingService;
        }

        #region Endpoints_Ecosystem

        [HttpPost("Employees/StartsWith")]
        public async Task<IActionResult> FetchEmployees(FetchEmployeesRequest request)
        {
            var result = await _employeeService.FetchByEcosystemAndEmailStartsWith(request.Startswith, request.EcosystemId, request.Account);
            AcademyResponse<List<EmployeeResponse>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpPost("Employees/GexLeaderStartsWith")]
        public async Task<IActionResult> FetchDojoGexLeader(FetchDojoGexRequest request)
        {
            var result = await _employeeService.FetchByGexLeaderNameStartsWith(request.Startswith);
            AcademyResponse<List<EmployeeResponse>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Ecosystems/Fetch/all")]
        public async Task<IActionResult> FetchAllEcosystems()
        {
            var result = await _ecosystemService.FetchAllEcosystem(true);
            AcademyResponse<List<EcosystemDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("Ecosystems/Fetch/all/formenu")]
        public async Task<IActionResult> FetchAllPrimaryEcosystems()
        {
            var result = await _ecosystemService.FetchAllPrimaryEcosystems();
            AcademyResponse<List<string>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = HttpStatusCode.OK,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("Ecosystems/Fetch/secondary")]
        public async Task<IActionResult> FetchSecondaryEcosystems()
        {
            var result = await _ecosystemService.FetchAllEcosystem(false);
            AcademyResponse<List<EcosystemDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpPost("Ecosystems/Insert/secondary")]
        public async Task<IActionResult> InsertSecondaryEcosystems([FromBody] EcosystemDto request)
        {
            var result = await _ecosystemService.InsertEcosystem(request);
            AcademyResponse<string> response = new()
            {
                Data = result.IsSuccess ? result.Value : string.Empty,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        #endregion Endpoints_Ecosystem

        #region Endpoints_Role
        [HttpGet("Role/Fetch")]
        public IActionResult GetRoleMaster()
        {
            return Ok(_roleService.GetRoleMaster());
        }

        [HttpPost("Role/Insert")]
        public async Task<IActionResult> AddRoleMaster([FromBody] Role role)
        {
            await _roleService.AddRoleMaster(role);
            return Created();
        }

        [HttpPatch("Role/Modify")]
        public async Task<IActionResult> UpdateRoleMaster(byte roleId, [FromBody] JsonPatchDocument<Role> patchRoleDoc)
        {
            var result = await _roleService.UpdateRoleMaster(roleId, patchRoleDoc);

            AcademyResponse<int> response = new AcademyResponse<int>()
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost("EmployeeRole/InsertOrUpdate")]
        public async Task<IActionResult> AddEmployeeRole(EmployeeRoleRequest request)
        {
            var result = await _roleService.AddEmployeeRole(request);
            AcademyResponse<bool> response = new()
            {
                Data = result.IsSuccess && result.Value,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpPost("Employee/Search")]
        public async Task<IActionResult> Search(SearchUserDto request)
        {
            var result = await _employeeService.Search(request.Searchkeywords);
            AcademyResponse<List<EmployeeRoleDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        #endregion Endpoints_Role

        #region Endpoints_Seniority
        [HttpGet("Seniority/Fetch")]
        public async Task<IActionResult> FetchSeniority()
        {
            var result = await _seniorityService.Fetch();
            AcademyResponse<List<SeniorityDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpPost("Seniority/Insert")]
        public async Task<IActionResult> InsertSeniority([FromBody] SeniorityDto request)
        {
            var result = await _seniorityService.Insert(request);
            AcademyResponse<string> response = new()
            {
                Data = result.IsSuccess ? result.Value : string.Empty,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpPost("Seniority/Modify")]
        public async Task<IActionResult> ModifySeniority([FromBody] SeniorityDto request)
        {
            var result = await _seniorityService.Modify(request);
            AcademyResponse<string> response = new()
            {
                Data = result.IsSuccess ? result.Value : string.Empty,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpDelete("Seniority/Deactivate/{seniorityId}")]
        public async Task<IActionResult> DeactivateSeniority([FromRoute] short seniorityId)
        {
            var result = await _seniorityService.Deactivate(seniorityId);
            AcademyResponse<string> response = new()
            {
                Data = result.IsSuccess ? result.Value : string.Empty,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        #endregion Endpoints_Seniority

        #region Endpoints_Proficiency
        [HttpGet("ProficiencyMaster/Fetch")]
        public async Task<IActionResult> FetchProficiencyMaster()
        {
            var result = await _proficiencyService.Fetch();
            AcademyResponse<List<ProficiencyDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        #endregion Endpoints_Proficiency

        [HttpGet("Tdc/FetchAll")]
        public async Task<IActionResult> FetchAllTdc()
        {
            var result = await _employeeService.FetchAllTdc();
            AcademyResponse<List<string>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("TdcCommunityForDojo/FetchAll")]
        public async Task<IActionResult> FetchAllTdcCommunityForDojo()
        {
            var result = await _employeeService.FetchAllTdcCommunityDojo();
            AcademyResponse<DojoCommunityCountryListResponse> response = new()
            {
                Data = result.IsSuccess ? result.Value : null,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("Account/FetchAll")]
        public async Task<IActionResult> FetchAllAccount()
        {
            var result = await _employeeService.FetchAllAccount();
            AcademyResponse<List<string>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        [HttpGet("Community/FetchAll")]
        public async Task<IActionResult> FetchAllCommunity()
        {
            var result = await _employeeService.FetchAllCommunity();
            AcademyResponse<List<string>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("ad/decrypt/{text}")]
        public IActionResult Decrypt([FromRoute] string text)
        {
            string decryptedText = text.Decrypt();
            return Ok(decryptedText);
        }
        [AllowAnonymous]
        [HttpGet("ad/encrypt/{text}")]
        public IActionResult Encrypt([FromRoute] string text)
        {
            string encryptedText = text.Encrypt();
            return Ok(encryptedText);
        }

        [HttpGet("Training/FetchAll")]
        public async Task<IActionResult> FetchTraining([FromQuery] string[] Communities, [FromQuery] string[] Areapaths, [FromQuery]int? PageIndex, [FromQuery]int? PageSize, [FromQuery]string? FilterByName)
        {

            var result = new List<TrainingDto>();
            if (Communities?.Length > 0 && Areapaths?.Length > 0)
            {
                result = await _trainingService.FetchByAreaPathAndCommunity(Communities, Areapaths);
            }
            else if (Communities?.Length > 0 && Areapaths?.Length == 0)
            {
                result = await _trainingService.FetchTrainingByCommunity(Communities);
            }
            else if (Communities?.Length == 0 && Areapaths?.Length > 0)
            {
                result = await _trainingService.FetchByAreaPath(Areapaths);
            }
            else if (PageIndex != null && PageSize != null)
            {
                AcademyResponse<IPagedList<TrainingDto>> academyResponse = new()
                {
                    Data = await _trainingService.FetchPagedTrainingList(FilterByName, PageIndex, PageSize),
                    Status = HttpStatusCode.OK,
                    Success = true
                };
                return Ok(academyResponse);
            }
            else
            {
                result = await _trainingService.FetchTraining();
            }
            AcademyResponse<List<TrainingDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }

        [HttpGet("Project/FetchAll")]
        public async Task<IActionResult> FetchAllProject([FromQuery] string[] Client)
        {
            var result = new List<string>();
            if (Client?.Length > 0)
            {
                result = await _employeeService.FetchAllProjectBasedonClient(Client);
            }
            else
            {
                 result = await _employeeService.FetchAllProject();
            }
            AcademyResponse<List<string>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }
        [HttpGet("Training/FetchTrainingStatus")]
        public async Task<IActionResult> FetchTrainingStatus()
        {
            var result = await _trainingService.FetchTrainingStatus();
            AcademyResponse<List<TrainingStatusListDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }
        [HttpGet("Training/FetchReportSelectColumns/{ActivityType}")]
        public async Task<IActionResult> FetchReportSelectColumns(string ActivityType)
        {
            var result = await _trainingService.FetchReportSelectColumns(ActivityType);
            AcademyResponse<List<ReportColumnConfigurationDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }

        [HttpGet("Training/FetchReportGroupByColumns/{ActivityType}")]
        public async Task<IActionResult> FetchReportGroupByColumns(string ActivityType)
        {
            var result = await _trainingService.FetchReportGroupByColumns(ActivityType);
            AcademyResponse<List<ReportColumnConfigurationDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }
        [HttpGet("ReportType/FetchAll")]
        public async Task<IActionResult> FetchAllReportType()
        {
            var result = await _trainingService.FetchReportType();
            AcademyResponse<List<ReportTypeDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }

        [HttpGet("Activities/FetchAll")]
        public async Task<IActionResult> FetchAllActivities()
        {
            var result = await _employeeService.FetchAllActivities();
            AcademyResponse<List<ActivityMasterDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        
        [HttpGet("AreaPath/FetchAll")]
        public async Task<IActionResult> FetchAllAreaPath()
        {
            var result = await _employeeService.FetchAllAreaPaths();
            AcademyResponse<List<LearningPathDto>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("Training/FetchByCommunity")]
        public async Task<IActionResult> FetchByCommunity([FromQuery] string[] Communities)
        {
            var result = new List<TrainingDto>();
            if (Communities?.Length > 0)
            {
                 result = await _trainingService.FetchTrainingByCommunity(Communities);
            }
            else
            {
                result = await _trainingService.FetchTraining();
            }
            AcademyResponse<List<TrainingDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }

        [HttpGet("PrimaryActivity/FetchPrimaryActivityByCommunity")]
        public async Task<IActionResult> FetchPrimaryActivityByCommunity([FromQuery] string[] Communities)
        {
            var result = new List<ActivityMasterDto>();
            if (Communities?.Length > 0)
            {
               result = await _trainingService.FetchPrimaryActivityByCommunity(Communities);
            }
            else
            {
                result = await _trainingService.FetchAllPrimaryActivity();
            }
            AcademyResponse<List<ActivityMasterDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }

        [HttpGet("PrimaryActivity/FetchAll")]
        public async Task<IActionResult> FetchAllPrimaryActivity()
        {
            var result = await _trainingService.FetchPrimaryActivity();
            AcademyResponse<List<PrimaryActivityTypeDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }
        [HttpGet("Training/FetchByAreaPathAndCommunity")]
        public async Task<IActionResult> FetchByAreaPathAndCommunity([FromQuery] string[] Communities, [FromQuery] string[] Areapaths)
        {
            var result = await _trainingService.FetchByAreaPathAndCommunity(Communities, Areapaths);
            AcademyResponse<List<TrainingDto>> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);

        }
        [HttpGet("Client/FetchAll")]
        public async Task<IActionResult> FetchAllClients()
        {
            var result = await _employeeService.FetchAllClients();
            AcademyResponse<List<string>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("AiStudio/FetchAll")]
        public async Task<IActionResult> FetchAllAiStudio()
        {
            var result = await _employeeService.FetchAllAiStudio();
            AcademyResponse<List<string>> response = new()
            {
                Data = result.IsSuccess ? result.Value : [],
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }

        [HttpGet("AiStudioAccount/FetchAll")]
        public async Task<IActionResult> FetchAllAiStudioAccount()
        {
            var result = await _employeeService.FetchAllAiStudioAccount();
            AcademyResponse<List<AiStudioAccount>> response = new()
            {
                Data = result.IsSuccess ? result.Value : new List<AiStudioAccount>(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
            return Ok(response);
        }
        
    }
}
