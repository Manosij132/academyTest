using Academy.Domain.Entities;
using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Services
{
    //AGK API Migration
    public interface IInterviewPanelService
    {
        Task<(List<InterviewPanelModel>, int, int)> GetAllInterviewPanelsData(InterviewPanelFilterModelRequest panelFilter, int pageNumber, int pageSize);
        Task<List<PanelModel>> GetAllPanelData();
        Task<List<TDCModel>> GetAllTDCData();
        Task<List<CommunityModel>> GetAllCommunityData();
        Task<List<SeniorityDto>> GetAllSeniorityData();
        Task<List<PanelSlotModel>> GetPanelSlotsDetail(int panelId);
        Task<DashboardDataModel> GetDashboardData(DashboardFilterModel panelFilter);
        Task<DashboardDataModel> GetInterviewPanelDetails(DashboardFilterModel panelFilter);
        Task<string> SavePanelSlotCalenderEvent(PanelSlotsCalenderEvent panelSlotsCalenderEvent);
        Task<PanelSlotDetailModel> GetPanelSlotDataById(int slotId);

        Task<bool> SendEmail(PanelSendEmailModel panelSendEmailModel);
        Task<List<AIEvaluationDataModel>> GetAIEvaluation(string panelEmail);
    }
}
