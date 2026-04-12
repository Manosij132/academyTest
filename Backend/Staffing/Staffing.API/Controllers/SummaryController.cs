using Microsoft.AspNetCore.Mvc;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using Staffing.Core.Abstraction.Services;
namespace Staffing.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummaryController : ControllerBase
    {
        private readonly IAISettingsProvider _settingsProvider;
        private readonly IStaffingSummaryService _summaryService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <param name="summaryService"></param>
        public SummaryController(
            IAISettingsProvider settingsProvider,
            IStaffingSummaryService summaryService)
        {
            _settingsProvider = settingsProvider;
            _summaryService = summaryService;
        }

        /// <summary>
        /// Get 
        /// </summary>
        /// <param name="startDateTxt"></param>
        /// <param name="endDateTxt"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(string? startDateTxt, string? endDateTxt)
        {
            var aiConnection = _settingsProvider.GetAIConnection();

            DateTime? startDate = DateTime.TryParse(startDateTxt, out var s) ? s : null;
            DateTime? endDate = DateTime.TryParse(endDateTxt, out var e) ? e : null;

            var result = await _summaryService.GetSummaryAsync(
                aiConnection.StaffingDbConnection,
                startDate,
                endDate);

            return Ok(result);
        }

        /// <summary>
        /// GetFilteredData
        /// </summary>
        /// <param name="summaryFilterRequest"></param>
        /// <returns></returns>
        [HttpPost("GetFilteredData")]
        public async Task<IActionResult> GetFilteredData([FromBody] SummaryFilterRequest summaryFilterRequest)
        {
            var aiConnection = _settingsProvider.GetAIConnection();
            var result = await _summaryService.GetSummaryFilteredDataAsync(aiConnection.StaffingDbConnection, summaryFilterRequest.GroupNames, summaryFilterRequest.Clients, summaryFilterRequest.Statuses, summaryFilterRequest.StartDateFrom, summaryFilterRequest.StartDateTo);
            return Ok(result);
        }

        /// <summary>
        /// GetFilteredTicketData
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetFilteredTicketData")]
        public async Task<IActionResult> GetFilteredTicketData(
            [FromBody] GetFilteredTicketDataRequest request)
        {
            var aiConnection = _settingsProvider.GetAIConnection();

            var (data, totalRecords) =
                await _summaryService.GetTicketFilteredDataAsync(
                    aiConnection.StaffingDbConnection,
                    request);

            return Ok(new { data, totalRecords });
        }

        /// <summary>
        /// GetTicketDropdownData
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTicketDropdownData")]
        public async Task<IActionResult> GetTicketDropdownData()
        {
            var aiConnection = _settingsProvider.GetAIConnection();

            var result =
                await _summaryService.GetTicketDropdownDataAsync(
                    aiConnection.StaffingDbConnection);

            return Ok(result);
        }

        /// <summary>
        /// GetClientAndDetailedStatusByAIGroup
        /// </summary>
        /// <param name="groupNames"></param>
        /// <param name="startDateTxt"></param>
        /// <param name="endDateTxt"></param>
        /// <returns></returns>
        [HttpPost("GetClientAndDetailedStatusByAIGroup")]
        public async Task<IActionResult> GetClientAndDetailedStatusByAIGroup(
        [FromBody] List<string> groupNames,
        string? startDateTxt,
        string? endDateTxt)
        {
            var aiConnection = _settingsProvider.GetAIConnection();

            DateTime? startDate = DateTime.TryParse(startDateTxt, out var s) ? s : null;
            DateTime? endDate = DateTime.TryParse(endDateTxt, out var e) ? e : null;

            var result = await _summaryService
                .GetClientAndDetailedStatusByAIGroupAsync(
                    aiConnection.StaffingDbConnection,
                    groupNames,
                    startDate,
                    endDate);

            return Ok(result);
        }

        /// <summary>
        /// GetDetailedStatusByAIGroupAndClient
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetDetailedStatusByAIGroupAndClient")]
        public async Task<IActionResult> GetDetailedStatusByAIGroupAndClient(
        [FromBody] GroupClientFilterRequest request)
        {
            var aiConnection = _settingsProvider.GetAIConnection();

            var result = await _summaryService
                .GetDetailedStatusByAIGroupAndClientAsync(
                    aiConnection.StaffingDbConnection,
                    request.GroupNames,
                    request.Clients,
                    request.StartDateFrom,
                    request.StartDateTo);

            return Ok(result);
        }
    }
}
