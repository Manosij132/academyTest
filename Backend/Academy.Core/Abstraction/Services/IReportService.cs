using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IReportService
    {
        Task ExportReport(string reportKey);
        Task<Result<DojoActivityReportResponse>> FetchAllDojoActivitiesForReport(FetchDojoActivityRequest dojoActivity);
        Task<Result<ExportDojoActivitiesReportResponse>> ExportDojoActivitiesReport(ExportDojoActivityRequest dojoActivity);
        Task<Result<AssignedThroughTrainingReportResponse>> FetchAssignThroughTrainingReport(FetchAssignedThroughTrainingRequest request);
        Task<Result<ExportDojoActivitiesReportResponse>> ExportAssignThroughTrainingReport(ExportAssignedThroughTrainingRequest request);
    }
}
