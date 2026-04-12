using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Academy.Workers.ReportWorker.ReportsCore
{
    internal class FullReport : IReport
    {
        private readonly IServiceScope scope;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSetting _appSetting;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Dashboard> _repositoryDashboard;
        private readonly IAcademyDbContext _dbContext;
        private readonly IGoogleApiManager _googleClient;
        private JobRequest job = new();
        private bool disposedValue;
        private const string SHEET_NAME_DASHBOARD = "Dashboard";
        private const string SHEET_NAME_TRAININGS = "Trainings";
        private string WorksheetId = string.Empty;
        private Employee requestor = new();
        List<EmployeeRoleMap> userRoles = new();
        Expression<Func<Dashboard, bool>>? predicate;
        private const int PaginationSize = 1000;
        List<int> EmployeeIds = [];
        public FullReport(IServiceProvider serviceProvider, AppSetting appSetting)
        {
            _serviceProvider = serviceProvider;
            _appSetting = appSetting;
            scope = _serviceProvider.CreateScope();
            _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            _repositoryDashboard = _unitOfWork.GetRepository<Dashboard>();
            _dbContext = scope.ServiceProvider.GetRequiredService<IAcademyDbContext>();
            _googleClient = scope.ServiceProvider.GetRequiredService<IGoogleApiManager>();
        }


        public async Task StartProcess(JobRequest jobRequest)
        {
            string status = string.Empty;
            job = jobRequest;
            EmailDump emailDump = new()
            {
                Cc = _appSetting.ReportsCc,
                To = requestor.GlobantEmailAddress,
                CreatedBy = _appSetting.SystemUser,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                Subject = "Export Report Task has been completed",
                Template = string.Empty
            };
            requestor = _dbContext.Employees.FirstOrDefault(e => e.Id == job.CreatedBy) ?? new();
            try
            {
                await ChangeStatusToOngoing(job);
                await Initialize();
                await ChangeStatusToCompleted(job, string.Empty);
                status = $"The report task with transaction Id {job.TransactionId}, has been completed. " +
                    $"Please see the report at: https://docs.google.com/spreadsheets/d/{WorksheetId}";

                emailDump.PlainText = status;
            }
            catch (Exception ex)
            {
                await ChangeStatusToCompleted(job, ex.ToString());
                status = $"The report task with transaction Id {job.TransactionId}, has been completed with errors -- {ex.Message}";
                emailDump.ErrorText = status;
            }
            finally
            {
                _dbContext.EmailDumps.Add(emailDump);
                await _dbContext.SaveChangesAsync();
            }
        }
        private async Task Initialize()
        {
            //Console.WriteLine($"[ReportService >> FullReport >> Initialize] Started...");
            KeyValuePair<string, string> sheet = await _googleClient.CreateNewWorksheet($"ExportFullReport_{DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat)}");
            WorksheetId = sheet.Key;

            await _googleClient.AddNewEmptySheetAsync(WorksheetId, SHEET_NAME_DASHBOARD);
            await _googleClient.AddNewEmptySheetAsync(WorksheetId, SHEET_NAME_TRAININGS);
            await _googleClient.RemoveSheetFromWorksheet(WorksheetId, "Sheet1");
            await _googleClient.MoveFileToAnotherFolder(WorksheetId, _appSetting.ExportReportDriveId);
            await _googleClient.GrantPermissionTo(WorksheetId, requestor.GlobantEmailAddress);

            userRoles = [.. _dbContext.EmployeeRoleMaps.Where(x => x.IsActive && x.EmployeeId == requestor.Id)];
            predicate = Utilities.BuildRoleGuard(userRoles, requestor);
            //Console.WriteLine($"[ReportService >> FullReport >> Initialize] Completed...");
            await ExportDashboard(0);
        }
        private async Task ExportDashboard(int pageIndex)
        {
            string range = string.Empty;
            //Console.WriteLine($"[ReportService >> FullReport >> ExportDashboard >> {pageIndex}] Started...");
            var result = await _repositoryDashboard.GetPagedListAsync(predicate: predicate, pageIndex: pageIndex, pageSize: PaginationSize);
            List<Dashboard> list = [.. result.Items];
            EmployeeIds.AddRange(list.Select(x => x.EmployeeId));
            IList<IList<object>> values = [];
            List<object> empty_row = [];
            PropertyInfo[] properties = typeof(Dashboard).GetProperties();
            if (pageIndex == 0)
            {
                List<object> headers = [];
                foreach (PropertyInfo property in properties)
                {
                    headers.Add(property.Name);
                }
                values.Add(headers);
                range = $"A{(1000 * pageIndex) + 1}:Z";
            }
            else
            {
                range = $"A{(1000 * pageIndex) + 2}:Z";
            }
            foreach (var item in list)
            {
                List<object> row = [];
                foreach (PropertyInfo property in properties)
                {
                    row.Add(property.GetValue(item, null));
                }
                values.Add(row);
            }
            // Adding empty row at the last of list, to avoid No Extra Rows Google Exception
            foreach (PropertyInfo property in properties)
            {
                empty_row.Add(string.Empty);
            }
            values.Add(empty_row);
            await _googleClient.InsertFormulaByRange(WorksheetId, SHEET_NAME_DASHBOARD, values, range);
            //Console.WriteLine($"[ReportService >> FullReport >> ExportDashboard >> {pageIndex}] Completed...");
            if (result.HasNextPage)
            {
                await ExportDashboard(pageIndex + 1);
            }
            await ExportTrainings(0);
        }

        private async Task ExportTrainings(int pageIndex)
        {
            string range = string.Empty;
            //Console.WriteLine($"[ReportService >> FullReport >> ExportTrainings >> {pageIndex}] Started...");
            var trainings = (from ET in _dbContext.EmployeeTrainingMaps
                             join S in _dbContext.SkillMasters on ET.SkillId equals S.SkillId
                             join T in _dbContext.TrainingMasters on ET.TrainingId equals T.TrainingId
                             join E in _dbContext.Employees on ET.EmployeeId equals E.Id
                             where EmployeeIds.Contains(ET.EmployeeId)
                             select new EmployeeTrainingRecord()
                             {
                                 EmployeeId = ET.EmployeeId,
                                 GlobantEmailAddress = E.GlobantEmailAddress,
                                 Seniority = E.Seniority,
                                 SkillName = S.SkillName,
                                 TrainingName = T.TrainingName,
                                 TrainingUrl = T.TrainingUrl,
                                 TrainingStatusId = ET.TrainingStatusId,
                                 TrainingStatus = string.Empty,
                                 StartDate = ET.StartDate,
                                 ActualEndDate = ET.ActualEndDate,
                                 ExpectedEndDate = ET.ExpectedEndDate
                             }).Skip(PaginationSize * pageIndex).Take(PaginationSize + 1).ToList();
            IList<IList<object>> trainingsList = [];
            //Console.Clear();
            //Console.WriteLine(WorksheetId);
            List<object> empty_row = [];

            if (pageIndex == 0)
            {
                trainingsList.Add(new List<object>
                {
                   nameof(EmployeeTrainingRecord.EmployeeId),
                   nameof(EmployeeTrainingRecord.GlobantEmailAddress),
                   nameof(EmployeeTrainingRecord.Seniority),
                   nameof(EmployeeTrainingRecord.SkillName),
                   nameof(EmployeeTrainingRecord.TrainingName),
                   nameof(EmployeeTrainingRecord.TrainingUrl),
                   nameof(EmployeeTrainingRecord.TrainingStatusId),
                   nameof(EmployeeTrainingRecord.TrainingStatus),
                   nameof(EmployeeTrainingRecord.StartDate),
                   nameof(EmployeeTrainingRecord.ActualEndDate),
                   nameof(EmployeeTrainingRecord.ExpectedEndDate)
                });
            }

            foreach (var training in trainings)
            {
                var dataRow = new List<object>
                {
                    training.EmployeeId,
                    training.GlobantEmailAddress,
                    training.Seniority,
                    training.SkillName,
                    training.TrainingName,
                    training.TrainingUrl,
                    training.TrainingStatusId,
                    Enum.GetName(typeof(TrainingStatus), training.TrainingStatusId) ,
                    training.StartDate,
                    training.ActualEndDate,
                    training.ExpectedEndDate
                };
                trainingsList.Add(dataRow);
            }
            trainingsList.Remove(trainingsList.Last());
            // Adding empty row at the last of list, to avoid No Extra Rows Google Exception
            PropertyInfo[] properties = typeof(EmployeeTrainingRecord).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                empty_row.Add(string.Empty);
            }
            trainingsList.Add(empty_row);
            List<Tuple<int?, string, int>> lastrowdata = await _googleClient.GetLastRowIndex(WorksheetId);
            Tuple<int?, string, int>? SHEET_NAME_TRAININGS_lastrow = lastrowdata.FirstOrDefault(x => x.Item2 == SHEET_NAME_TRAININGS);
            range = (SHEET_NAME_TRAININGS_lastrow?.Item3 == 0) ? $"A{1}:Z" : $"A{SHEET_NAME_TRAININGS_lastrow?.Item3 + 1}:Z";
            await _googleClient.InsertFormulaByRange(WorksheetId, SHEET_NAME_TRAININGS, trainingsList, range);
            //Console.WriteLine($"[ReportService >> FullReport >> ExportTrainings >> {pageIndex}] Completed...");
            if (trainings.Count > PaginationSize)
            {
                await ExportTrainings(pageIndex + 1);
            }
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects) here.
                    scope?.Dispose();
                }

                // Dispose unmanaged resources (if any) here.

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #region Private Methods
        private async Task ChangeStatusToCompleted(JobRequest job, string message = "")
        {
            if (_dbContext is not null)
            {
                job.UpdatedOn = DateTime.UtcNow;
                job.UpdatedBy = _appSetting.SystemUser;
                job.ErrorDetail = message;
                job.RetryCount += 1;
                job.Status = TrainingStatus.Completed.ToString();
                _dbContext.JobRequests.Update(job);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task ChangeStatusToOngoing(JobRequest job)
        {
            if (_dbContext is not null)
            {
                job.UpdatedOn = DateTime.UtcNow;
                job.UpdatedBy = _appSetting.SystemUser;
                job.Status = TrainingStatus.Ongoing.ToString();
                _dbContext.JobRequests.Update(job);
                await _dbContext.SaveChangesAsync();
            }
        }
        #endregion
    }
}
