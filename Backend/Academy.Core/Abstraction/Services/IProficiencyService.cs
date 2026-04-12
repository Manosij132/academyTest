using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IProficiencyService
    {
        Task<Result<int>> InsertOrUpdateEmployeeProficiency(ProficiencyRequest request);
        Task<Result<List<SkillEndorsementResponse>>> FetchProficienciencies(int employeeId);
        Task<Result<List<ProficiencyDto>>> Fetch();
        Task<Result<List<ProficiencyDto>>> FetchProficiencyByEcosystemSkill(short ecosystemId, short skillId);
    }
}
