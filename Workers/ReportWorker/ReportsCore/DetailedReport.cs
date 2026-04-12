using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Extensions;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Academy.Core.Abstraction.Services;
using Academy.Core.Mapper;

namespace Academy.Workers.ReportWorker.ReportsCore
{
    internal class DetailReport : IReport
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
        private const string SHEET_NAME_DETAILREPORT = "DetailReport";
        private const string TemplateName = "ACADEMY_TRAINING_REPORT_GENERATED";
        private string WorksheetId = string.Empty;
        private Employee requestor = new();
        List<EmployeeRoleMap> userRoles = new();
        Expression<Func<Dashboard, bool>>? predicate;
        private const int PaginationSize = 1000;
        List<int> EmployeeIds = [];
        private readonly IReportDataService _getReportDataService;
        public DetailReport(IServiceProvider serviceProvider, AppSetting appSetting)
        {
            _serviceProvider = serviceProvider;
            _appSetting = appSetting;
            scope = _serviceProvider.CreateScope();
            _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            _repositoryDashboard = _unitOfWork.GetRepository<Dashboard>();
            _dbContext = scope.ServiceProvider.GetRequiredService<IAcademyDbContext>();
            _googleClient = scope.ServiceProvider.GetRequiredService<IGoogleApiManager>();
            _getReportDataService = scope.ServiceProvider.GetRequiredService<IReportDataService>();
        }
        public async Task StartProcess(JobRequest jobRequest)
        {
            List<string> emailList = new List<string>();
            string status = string.Empty;
            job = jobRequest;

            ExportDetailReportMetadata metadata = JsonConvert.DeserializeObject<ExportDetailReportMetadata>(jobRequest.RequestMetadata) ?? new();
            int bookmarkId = metadata.BookMarkId;

            BookMarkTemplates bookMarkTemplate = _dbContext.BookMarkTemplates.Where(x => x.BookMarkId == bookmarkId).SingleOrDefault();

            EmailDump emailDump = new()
            {
                Cc = bookMarkTemplate.CC == null ? String.Empty : bookMarkTemplate.CC,
                To = bookMarkTemplate.To == null ? String.Empty : bookMarkTemplate.To,
                CreatedBy = _appSetting.SystemUser,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                Subject = bookMarkTemplate.Subject==null?String.Empty: bookMarkTemplate.Subject,
                Template = TemplateName
            };

            if (bookMarkTemplate.To != null)
            {
                emailList.AddRange(bookMarkTemplate.To.Split(',').ToList());
            }

            if (bookMarkTemplate.CC != null)
            {
                emailList.AddRange(bookMarkTemplate.CC.Split(',').ToList());
            }

            requestor = _dbContext.Employees.FirstOrDefault(e => e.Id == job.CreatedBy) ?? new();
            try
            {
                await ChangeStatusToOngoing(job);
                await Initialize(jobRequest, emailList);
                await ChangeStatusToCompleted(job, string.Empty);
                status = $"https://docs.google.com/spreadsheets/d/{WorksheetId}";
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
        private async Task Initialize(JobRequest jobRequest, List<string> emailList)
        {
            //Console.WriteLine($"[ReportService >> FullReport >> Initialize] Started...");
            KeyValuePair<string, string> sheet = await _googleClient.CreateNewWorksheet($"ExportDetailReport_{DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat)}");
            WorksheetId = sheet.Key;

            await _googleClient.AddNewEmptySheetAsync(WorksheetId, SHEET_NAME_DETAILREPORT);
            await _googleClient.RemoveSheetFromWorksheet(WorksheetId, "Sheet1");
            await _googleClient.MoveFileToAnotherFolder(WorksheetId, _appSetting.ExportReportDriveId);
            await _googleClient.GrantPermissionTo(WorksheetId, emailList.ToArray());
            userRoles = [.. _dbContext.EmployeeRoleMaps.Where(x => x.IsActive && x.EmployeeId == requestor.Id)];
            predicate = Utilities.BuildRoleGuard(userRoles, requestor);
            //Console.WriteLine($"[ReportService >> FullReport >> Initialize] Completed...");
            await ExportDetailReport(0, jobRequest);
        }
        private async Task ExportDetailReport(int pageIndex, JobRequest jobRequest)
        {
            string range = string.Empty;
            //Console.WriteLine($"[ReportService >> FullReport >> ExportTrainings >> {pageIndex}] Started...");


            var trainings = await GetDetailReportData(jobRequest);
            IList<IList<object>> trainingsList = new List<IList<object>>();
            //Console.Clear();
            //Console.WriteLine(WorksheetId);
            List<object> empty_row = [];

            var headerRow = new List<object>();
            foreach (DataColumn column in trainings.Columns)
            {
                string formatedColumnName = FormatColumnName(column.ColumnName);
                headerRow.Add(formatedColumnName);
            }
            trainingsList.Add(headerRow);


            foreach (DataRow row in trainings.Rows)
            {
                var rowData = new List<object>();

                foreach (var item in row.ItemArray)
                {
                    rowData.Add(item);
                }

                trainingsList.Add(rowData);
            }

            //trainingsList.Remove(trainingsList.Last());
            // Adding empty row at the last of list, to avoid No Extra Rows Google Exception
            PropertyInfo[] prop = typeof(EmployeeTrainingRecord).GetProperties();
            foreach (PropertyInfo property in prop)
            {
                empty_row.Add(string.Empty);
            }
            trainingsList.Add(empty_row);
            List<Tuple<int?, string, int>> lastrowdata = await _googleClient.GetLastRowIndex(WorksheetId);
            Tuple<int?, string, int>? SHEET_NAME_TRAININGS_lastrow = lastrowdata.FirstOrDefault(x => x.Item2 == SHEET_NAME_DETAILREPORT);
            range = (SHEET_NAME_TRAININGS_lastrow?.Item3 == 0) ? $"A{1}:Z" : $"A{SHEET_NAME_TRAININGS_lastrow?.Item3 + 1}:Z";
            await _googleClient.InsertFormulaByRange(WorksheetId, SHEET_NAME_DETAILREPORT, trainingsList, range);
            //Console.WriteLine($"[ReportService >> FullReport >> ExportTrainings >> {pageIndex}] Completed...");
            await ChangeStatusToCompleted(job, string.Empty);
        }
        public string FormatColumnName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return columnName;
            }

            var formattedName = new System.Text.StringBuilder();
            for (int i = 0; i < columnName.Length; i++)
            {
                char currentChar = columnName[i];
                if (char.IsUpper(currentChar) && i > 0)
                {
                    formattedName.Append(' ');
                }
                formattedName.Append(currentChar);
            }
            return formattedName.ToString();
        }
        private async Task<DataTable> GetDetailReportData(JobRequest jobRequest)
        {
            var result = new DataTable();

            ExportDetailReportMetadata metadata = JsonConvert.DeserializeObject<ExportDetailReportMetadata>(jobRequest.RequestMetadata) ?? new();
            int bookmarkId = metadata.BookMarkId;
            BookMarkTemplates bookMarkTemplate = _dbContext.BookMarkTemplates.Where(x => x.BookMarkId == bookmarkId).SingleOrDefault();

            var req = BookMarkRequestMapper.ToBookMarkRequest(bookMarkTemplate);

            var response = await _getReportDataService.GetReportData(req);

            DataTable dte = new DataTable();

            var ress = ConvertJsonToDataTable(response);

            return ress;
        }

        public static DataTable ConvertJsonToDataTable(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var records = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonData, options);
            DataTable dataTable = new DataTable();
            if (records != null && records.Count > 0)
            {
                // Create columns
                foreach (var key in records[0].Keys)
                {
                    dataTable.Columns.Add(key);
                }
                // Add rows
                foreach (var record in records)
                {
                    var row = dataTable.NewRow();
                    foreach (var key in record.Keys)
                    {
                        row[key] = record[key];
                    }
                    dataTable.Rows.Add(row);
                }
            }
            return dataTable;
        }
        private static List<int> ToIntList(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<int>()
                : input.Split(',')
                       .Select(s => s.Trim())
                       .Where(s => int.TryParse(s, out _))
                       .Select(int.Parse)
                       .ToList();
        }

        private static List<string> ToStringList(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<string>()
                : input.Split(',')
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
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
