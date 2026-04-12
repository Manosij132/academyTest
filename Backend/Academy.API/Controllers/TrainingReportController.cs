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
    public class TrainingReportController : ControllerBase
    {
        private readonly IBookMarkService _bookMarkService;

        public TrainingReportController(IBookMarkService bookMarkService)
        {
            _bookMarkService = bookMarkService;
        }

        [HttpPost("BookMark/AddBookMark")]
        public async Task<IActionResult> AddBookMark([FromBody] BookMarkRequest request)
        {
            if (request == null)
                return BadRequest(CreateResponse("Please provide proper request", HttpStatusCode.BadRequest, false));

            var response = request.BookMarkId == 0
                ? await _bookMarkService.Insert(request)
                : await _bookMarkService.Modify(request);

            return Ok(response);
        }

        [HttpGet("BookMark/GetBookMark")]
        public IActionResult GetBookMark()
        {
            var result = _bookMarkService.Fetch();

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }

        [HttpGet("BookMark/GetBookMarkById/{bookMarkId}")]
        public IActionResult GetBookMarkById([FromRoute] int bookMarkId)
        {
            if (bookMarkId <= 0)
                return BadRequest(CreateResponse("Please provide a valid BookMarkId", HttpStatusCode.BadRequest, false));

            var result = _bookMarkService.Search(bookMarkId);

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }

        [HttpDelete("BookMark/DeleteBookMark/{bookMarkId}")]
        public async Task<IActionResult> DeleteBookMarkAsync([FromRoute] int bookMarkId)
        {
            var result = await _bookMarkService.Deactivate(bookMarkId);

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }

        [HttpPost("BookMark/ViewReport")]
        public async Task<IActionResult> ViewReport([FromBody] BookMarkRequest request)
        {
            if (request == null)
                return BadRequest(CreateResponse("Please provide a proper request", HttpStatusCode.BadRequest, false));

            var result = await _bookMarkService.GetReportData(request);

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }

        [HttpPost("BookMark/SendReportOnEmail")]
        public async Task<IActionResult> SendReportOnEmailAsync([FromBody] ReportEmailRequest reportEmailRequest)
        {
            if (reportEmailRequest == null)
                return BadRequest(CreateResponse("Please provide a proper request", HttpStatusCode.BadRequest, false));

            var result = await _bookMarkService.SendReportData(reportEmailRequest);

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }

        [HttpPost("BookMark/PreviewReport/{bookMarkId}")]
        public async Task<IActionResult> PreviewReport([FromRoute] int bookMarkId)
        {
            if (bookMarkId <= 0)
                return BadRequest(CreateResponse("Please provide a valid BookMarkId", HttpStatusCode.BadRequest, false));

            var emailBody = _bookMarkService.Search(bookMarkId).EmailBody;
            var reportData = await _bookMarkService.GenerateReportData(bookMarkId);
            var result = await _bookMarkService.ReplaceTable(reportData, emailBody, ReportTypeName.ReportName);

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }

        [HttpPost("BookMark/ExportReport")]
        public async Task<IActionResult> ExportReport([FromBody] BookMarkRequest request)
        {
            if (request == null)
                return BadRequest(CreateResponse("Please provide a proper request", HttpStatusCode.BadRequest, false));

            var result = await _bookMarkService.ExportGenerateReportDataBookMarkRequest(request);

            return Ok(CreateResponse(result, HttpStatusCode.OK));
        }


        // 🔧 Reusable method to build responses
        private AcademyResponse<T> CreateResponse<T>(T data, HttpStatusCode status, bool success = true)
        {
            return new AcademyResponse<T>
            {
                Data = data,
                Status = status,
                Success = success
            };
        }
    }
}
