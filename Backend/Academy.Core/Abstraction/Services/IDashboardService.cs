using Academy.Core.Services;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Microsoft.AspNetCore.Http;

namespace Academy.Core.Abstraction.Services
{
    public interface IDashboardService
    {
        Task<Result<IPagedList<Dashboard>>> FetchTrackerList(DataRequestOptions dataRequestOptions);
        Task<Result<DashboardResponse>> FetchDashboard(int employeeId);
        Task<Result<int>> ExtendEndDate(ExtendEndDateRequest request);
        Task<Result<int>> PostComment(CommentRequest request);
        Task<Result<List<CommentResponse>>> FetchComments(int employeeId, bool latestOnly = false);
        Task<Result<int>> ChangeStatus(ChangeStatusRequest request);
        Task<Result<string>> ExecuteTrainingAssignmentJob(SpinTrainingRequest request);
        Task<string> ExecuteTrainingAssignmentJob(SpinTrainingRequest request, List<string> emails = null);
        Task<Result<string>> ExecuteReportJob(ExportReportMetadata request);
        Task<Result<Tuple<JobRequest, List<JobRequestDetail>>>> RequestTrackerStatus(string transactionId);
        Task<Result<int>> UpdateDojoGxLeadxer(DojoGxLeadxerRequest request);
        Task<string> ExecuteReportJob(ExportDetailReportMetadata request);
        Task<int> ChangeStatusByEmail(TrainingUpdateRequest trainingUpdate);
        Task<bool> FetchTraining(string trainingName);
        Task<string> UploadEmployeeCV(IFormFile file, int employeeId, string community, int docTypeId, string existingCVFileId = null);
        Task<Result<List<EmployeeDocumentType>>> FetchAllDocumentType();
    }
}
