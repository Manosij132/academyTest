using Academy.Shared.DTO;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;

namespace Academy.Core.Abstraction.Services
{
    public interface ISkillAndTrainingService
    {
        Task<Result<int>> InsertOrUpdateSkill(SkillDto request);
        Task<Result<int>> InsertOrUpdateTraining(TrainingProficiencyDto request);
        Task<Result<int>> InsertTrainingProficiencyMapping(TrainingProficiencyDto request);
        Task<Result<int>> UpdateTrainingProficiency(TrainingProficiencyDto request);
        Task<Result<List<TrainingsGroupedBySkill>>> FetchSkillTrainingsMetaData(int ecosystemId);
        Task<Result<List<BaseSkillEndorsementResponse>>> FetchSkillEndorsement(short ecosystemId, string account, string commaSeperatedUserIds);
        Task<Result<List<SkillDto>>> FetchSkills();
        Task<Result<string>> CreateTrainings(ManageTrainingDto request);
        Task<Result<List<CategoryDto>>> FetchCategory();
        Task<Result<List<SubCategoryDto>>> FetchSubCategory();
        Task<Result<int>> CreateCategoryOrSubCategory(SubCategoryDto request);

        Task<List<TrainingDto>> FetchTraining();
        Task<IPagedList<TrainingDto>> FetchPagedTrainingList(string FilterByName, int? pageIndex, int? pageSize);
        Task<List<TrainingStatusListDto>> FetchTrainingStatus();
        Task<List<ReportColumnConfigurationDto>> FetchReportSelectColumns(string ActivityType);
        Task<List<ReportColumnConfigurationDto>> FetchReportGroupByColumns(string ActivityType);
        Task<List<ReportTypeDto>> FetchReportType();
        Task<List<TrainingDto>> FetchTrainingByCommunity(string[] Communities);
        Task<List<ActivityMasterDto>> FetchPrimaryActivityByCommunity(string[] Communities);
        Task<List<ActivityMasterDto>> FetchAllPrimaryActivity();

        Task<List<PrimaryActivityTypeDto>> FetchPrimaryActivity();
        Task<List<TrainingDto>> FetchByAreaPathAndCommunity(string[] Communities, string[] Areapaths);

        Task<List<TrainingDto>> FetchByAreaPath(string[] Areapaths);
        Task<List<TrainingsGroupedBySkill>> FetchAISkillTrainingsMetaData();
    }
}
