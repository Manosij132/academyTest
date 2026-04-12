using System.Net;
using Academy.Core.Abstraction.Services;
using Academy.Core.Services;
using Academy.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class ReportController : ApiBaseController
    {
        public IDashboardService _dashboardService { get; set; }
        private readonly IReportService _reportService;
        public ReportController(IDashboardService dashboardService, IReportService reportService)
        {
            _dashboardService = dashboardService;
            _reportService = reportService;
        }

        [HttpPost("Execute/ReportJob")]
        public async Task<IActionResult> ExecuteReportJob([FromBody] ExportReportMetadata request)
        {
            var result = await _dashboardService.ExecuteReportJob(request);
            return ToActionResult(result);
        }
        [HttpPost("Execute/DetailedReportJob")]
        public async Task<IActionResult> ExecuteReportJob([FromBody] ExportDetailReportMetadata requests)
        {
            var result = await _dashboardService.ExecuteReportJob(requests);
            return ToActionResult(result);
        }

        [HttpPost("GetDojoActivityReport")]
        public async Task<IActionResult> FetchDojoActivity([FromBody] FetchDojoActivityRequest request)
        {
            var result = await _reportService.FetchAllDojoActivitiesForReport(request);

            var response = new AcademyResponse<DojoActivityReportResponse>
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }

        [HttpPost("ExportDojoActivityReport")]
        public async Task<IActionResult> FetchDojoActivity([FromBody] ExportDojoActivityRequest request)
        {
            var result = await _reportService.ExportDojoActivitiesReport(request);

            var response = new AcademyResponse<ExportDojoActivitiesReportResponse>
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }
        
        [HttpPost("AssignedThroughTraining")]
        public async Task<IActionResult> FetchAssignThroughTraining([FromBody] FetchAssignedThroughTrainingRequest request)
        {
            var result = await _reportService.FetchAssignThroughTrainingReport(request);

            var response = new AcademyResponse<AssignedThroughTrainingReportResponse>
            {
                Data = result.IsSuccess ? result.Value : new(),
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };

            return Ok(response);
        }
        
        [HttpPost("ExportAssignThroughTrainingReport")]
        public async Task<IActionResult> ExportAssignThroughTrainingReport([FromBody] ExportAssignedThroughTrainingRequest request)
        {
            var result = await _reportService.ExportAssignThroughTrainingReport(request);

            var response = new AcademyResponse<ExportDojoActivitiesReportResponse>
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
