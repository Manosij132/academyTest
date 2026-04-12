using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Academy.API.Controllers
{
    //AGK API Migration

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class SlotRequirementController : ControllerBase
    {
        private readonly ISlotRequirementService _slotRequirementService;
        public SlotRequirementController(ISlotRequirementService slotRequirementService)
        {
            _slotRequirementService = slotRequirementService; ;
        }

        [HttpGet]
        [Route("GetAllSlotManagementData")]
        public async Task<IActionResult> GetAllSlotManagementData(string TDC, int communityID, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = await _slotRequirementService.GetAllSlotManagement(TDC, communityID, startDate, endDate);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCommunitySelectionRatio")]
        public async Task<CommunitySelectionRatioModel> GetCommunitySelectionRatio(string? TDC = null, int communityID = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            return _slotRequirementService.GetCommunitySelectionRatio(TDC, communityID, startDate, endDate).Result;
        }

        [Route("GetPredicatedRatio")]
        [HttpGet]
        public async Task<IActionResult> GetPredicatedRatio(string? TDC = null, int communityId = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = await _slotRequirementService.GetPredicatedRatio(TDC, communityId, startDate, endDate);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateCommunitySelectionRatio")]
        public async Task<IActionResult> UpdateCommunitySelectionRatio(CommunitySelectionRatioModel communitySelectionRatioModel)
        {
            if (communitySelectionRatioModel.StartDate.HasValue && communitySelectionRatioModel.EndDate.HasValue)
            {
                communitySelectionRatioModel.StartDate = TimeZoneInfo.ConvertTimeFromUtc(communitySelectionRatioModel.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                communitySelectionRatioModel.EndDate = TimeZoneInfo.ConvertTimeFromUtc(communitySelectionRatioModel.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
            var result = _slotRequirementService.UpdateCommunitySelectionRatio(communitySelectionRatioModel);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdatePanelSlotRequirement")]
        public async Task<IActionResult> UpdatePanelSlotRequirement(List<SlotRequirementModel> panelSlotsRequirementModeList)
        {
            try
            {
                var result = ValidateField(panelSlotsRequirementModeList);
                if (!result.IsNullOrEmpty())
                {
                    return BadRequest(result);
                }
                bool update = await _slotRequirementService.UpdateSlotManagement(panelSlotsRequirementModeList);

                if (update)
                    return Ok(update);
                else
                    return BadRequest("Error occurred while inserting or updating Panel Slot Requirement.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        private string ValidateField(List<SlotRequirementModel> panelSlotsRequirementMode)
        {
            string str = string.Empty;
            if (panelSlotsRequirementMode == null || panelSlotsRequirementMode.Count == 0)
                str = "Please provide valid input";
            return str;
        }

        [HttpPost]
        [Route("CreatePanelSlotRequired")]
        public async Task<IActionResult> CreatePanelSlotRequired(List<SlotRequirementModel> slotRequirementModel)
        {
            try
            {
                var result = ValidateField(slotRequirementModel);
                if (!result.IsNullOrEmpty())
                {
                    return BadRequest(result);
                }

                bool isAddOrUpdatePnlSlots = await _slotRequirementService.CreateSlotManagement(slotRequirementModel);
                if (isAddOrUpdatePnlSlots)
                    return Ok(isAddOrUpdatePnlSlots);
                else
                    return BadRequest("Error occurred while inserting or updating Panel Slot Requirement.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [Route("DeleteSlotsRequirement")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSlotsRequirement(int id)
        {
            await _slotRequirementService.DeleteSlotManagement(id);
            return Ok("Data Is Deleted Successfully");
        }
    }
}
