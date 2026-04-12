using Academy.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Core.Abstraction.Services
{

    //AGK API Migration
    public interface ISlotRequirementService
    {
        Task<List<SlotRequirementModel>> GetAllSlotManagement(string? TDC = null, int communityID = 0, DateTime? startDate = null, DateTime? endDate = null);
        Task<CommunitySelectionRatioModel> GetCommunitySelectionRatio(string? TDC = null, int communityID = 0, DateTime? startDate = null, DateTime? endDate = null);
        Task<CommunitySelectionRatioModel> GetPredicatedRatio(string? TDC = null, int communityId = 0, DateTime? startDate = null, DateTime? endDate = null);
        Task<CommunitySelectionRatioModel> UpdateCommunitySelectionRatio(CommunitySelectionRatioModel communitySelectionRatioModel);
        Task<bool> UpdateSlotManagement(List<SlotRequirementModel> slotRequirementModel);
        Task<bool> CreateSlotManagement(List<SlotRequirementModel> slotRequirementModel);
        Task DeleteSlotManagement(int id);
    }
}
