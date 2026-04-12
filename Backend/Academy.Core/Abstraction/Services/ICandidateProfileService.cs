using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Services
{
    public interface ICandidateProfileService
    {
        List<PanelEfficiencyResponseDto> Process(int pageNumber, int pageSize, string? startDate, string? endDate);
        int GetTotalCount(string? startDate, string? endDate);
    }
}
