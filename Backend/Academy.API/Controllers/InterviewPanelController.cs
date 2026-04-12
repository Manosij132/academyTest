using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Google.Apis.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Academy.API.Controllers
{
    //AGK API Migration

    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class InterviewPanelController : ApiBaseController
    {

        private readonly IInterviewPanelService _interviewPanelService;
        public InterviewPanelController(IInterviewPanelService interviewPanelService)
        {
            _interviewPanelService = interviewPanelService;
        }

        [Route("GetAllInterviewPanelsByFilterAsync")]
        [HttpPost]
        public async Task<IActionResult> GetAllInterviewPanelsByFilterAsync([FromQuery] PaginationFilter filter, InterviewPanelFilterModelRequest interviewPanelModel)
        {
            try
            {
                var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

                var data = await _interviewPanelService.GetAllInterviewPanelsData(interviewPanelModel, filter.PageNumber, filter.PageSize);
                PagedResponse<List<InterviewPanelModel>> pagedResponse = new(data.Item1, validFilter.PageNumber, validFilter.PageSize);
                pagedResponse.TotalFilteredRecords = data.Item2;

                int totalPages = pagedResponse.TotalFilteredRecords / pagedResponse.PageSize;

                if ((pagedResponse.TotalFilteredRecords % pagedResponse.PageSize) != 0)
                {
                    totalPages++;
                }
                pagedResponse.TotalRecords = data.Item2;
                pagedResponse.TotalPages = totalPages;

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Route("GetDashboardData")]
        [HttpPost]
        public async Task<IActionResult> GetDashboardData(DashboardFilterModel interviewPanelModel)
        {
            try
            {
                var dashboardDataModel = await _interviewPanelService.GetDashboardData(interviewPanelModel);
                return Ok(dashboardDataModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("GetInterviewPanelDetails")]
        [HttpPost]
        public async Task<IActionResult> GetInterviewPanelDetails(DashboardFilterModel interviewPanelModel)
        {
            try
            {
                var dashboardDataModel = await _interviewPanelService.GetInterviewPanelDetails(interviewPanelModel);
                return Ok(dashboardDataModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet]
        [Route("GetAllTDCData")]
        [ResponseCache(Duration = 25200, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetAllTDCData()
        {
            var result = await _interviewPanelService.GetAllTDCData();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllCommunityData")]
        [ResponseCache(Duration = 25200, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetAllCommunityData()
        {
            var result = await _interviewPanelService.GetAllCommunityData();
            return Ok(result);
        }
        [HttpGet]
        [Route("GetAllSeniorityData")]
        [ResponseCache(Duration = 25200, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetAllSeniorityData()
        {
            var result = await _interviewPanelService.GetAllSeniorityData();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllPanelData")]
        [ResponseCache(Duration = 25200, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetAllPanelData()
        {
            var result = await _interviewPanelService.GetAllPanelData();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetPanelSlotsDetail")]
        public async Task<IActionResult> GetPanelSlotsDetail(int panelId)
        {
            var result = await _interviewPanelService.GetPanelSlotsDetail(panelId);
            return Ok(result);
        }

        [HttpPost]
        [Route("SendEmail")]
        public async Task<IActionResult> SendEmail(PanelSendEmailModel panelSendEmailModel)
        {
            var result = await _interviewPanelService.SendEmail(panelSendEmailModel);
            return Ok(result);
        }

        /// <summary>
        /// Save panel slot with google caleder event id
        /// 
        /// </summary>
        /// <param name="panelSlotsCalenderEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SavePanelSlotCalenderEvent")]
        public async Task<IActionResult> SavePanelSlotCalenderEvent(PanelSlotsCalenderEvent panelSlotsCalenderEvent)
        {
            var result = string.Empty;
            panelSlotsCalenderEvent.TargetIanaTimeZoneId = "Asia/Kolkata";

            try
            {
                result = await _interviewPanelService.SavePanelSlotCalenderEvent(panelSlotsCalenderEvent);
            }
            catch (Exception ex)
            {
                throw ex;
                result = ex.ToString();
            }

            return Ok(result);
        }

        /// <summary>
        /// Fetch the panel slot detail based on id
        /// 
        /// </summary>
        /// <param name="slotId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPanelSlotDataById")]
        public async Task<IActionResult> GetPanelSlotDataById(int slotId)
        {
            var result = await _interviewPanelService.GetPanelSlotDataById(slotId);
            return Ok(result);
        }

        [HttpGet]
        [Route("PanelAIEvaluation")]
        public async Task<IActionResult> GetAIEvaluation(string panelEmail)
        {
            try
            {
                var result = await _interviewPanelService.GetAIEvaluation(panelEmail);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
