using Microsoft.AspNetCore.Mvc;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffRequestsController : ControllerBase
    {
        private readonly IAISettingsProvider _settingsProvider;
        private readonly IStaffRequestService _staffRequestService;

        public StaffRequestsController(
            IAISettingsProvider settingsProvider,
            IStaffRequestService staffRequestService)
        {
            _settingsProvider = settingsProvider;
            _staffRequestService = staffRequestService;
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="dateField"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="searchText"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string dateField = "StartDate",
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] string? searchText = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var aiConnection = _settingsProvider.GetAIConnection();
            var dbConnection = aiConnection.ClientDbConnection ?? aiConnection.StaffingDbConnection;

            var result = await _staffRequestService.QueryStaffRequestsByDateAsync(
                dbConnection,
                startDate,
                endDate,
                searchText,
                pageNumber,
                pageSize);

            return Ok(result);
        }

        /// <summary>
        /// GetById
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var aiConnection = _settingsProvider.GetAIConnection();
            var dbConnection = aiConnection.ClientDbConnection ?? aiConnection.StaffingDbConnection;

            var item = await _staffRequestService.GetStaffRequestByIdAsync(dbConnection, id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        /// <summary>
        /// UpdateEditableFields
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEditableFields(
            int id,
            [FromBody] StaffRequestUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Empty request body.");

            var aiConnection = _settingsProvider.GetAIConnection();
            var dbConnection = aiConnection.ClientDbConnection ?? aiConnection.StaffingDbConnection;

            var rows = await _staffRequestService
                .UpdateStaffRequestEditableFieldsAsync(dbConnection, id, dto);

            if (rows == 0)
                return NotFound();

            var updated = await _staffRequestService
                .GetStaffRequestByIdAsync(dbConnection, id);

            return Ok(updated);
        }
    }
}