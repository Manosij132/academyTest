using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Mapper;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Academy.Core.Services
{
    public class BookMarkService : IBookMarkService
    {
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ISendEmailService _sendEmailService;
        public readonly IDashboardService _dashboardService;
        public readonly IReportDataService _getReportDataService;
        private readonly IGoogleApiManager _googleApiManager;
        private readonly AppSetting _appSetting;

        public BookMarkService(IAcademyDbContext academyDbContext, IAuthenticatedUserService authenticatedUserService, ISendEmailService sendEmailService, IDashboardService dashboardService, IReportDataService getReportDataService, IGoogleApiManager googleApiManager, IOptions<AppSetting> appSetting)
        {
            _academyDbContext = academyDbContext;
            _authenticatedUserService = authenticatedUserService;
            _sendEmailService = sendEmailService;
            _dashboardService = dashboardService;
            _getReportDataService = getReportDataService;
            _googleApiManager = googleApiManager;
            _appSetting = appSetting.Value;
        }

        public async Task<string> Deactivate(int bookMarkId)
        {
            var bookMarkTemplate = await _academyDbContext.BookMarkTemplates
                .FirstOrDefaultAsync(x => x.BookMarkId == bookMarkId && x.IsActive);

            if (bookMarkTemplate == null)
                return Messages.ERROR_BookMarkTemplateNotFound;

            bookMarkTemplate.IsActive = false;

            _academyDbContext.BookMarkTemplates.Update(bookMarkTemplate);
            int response = await _academyDbContext.SaveChangesAsync();

            return response == 1 ? Messages.SUCCESS_GENERIC : Messages.ERROR_Generic;
        }

        public List<BookMarkTemplateListDto> Fetch()
        {
            return _academyDbContext.BookMarkTemplates
                .Where(x => x.IsActive)
                .Select(x => new BookMarkTemplateListDto
                {
                    BookMarkId = x.BookMarkId,
                    BookMarkName = x.BookMarkName,
                    ReportType = x.ReportType,
                    EmailCC = x.CC,
                    EmailSubject = x.Subject,
                    EmailTo = x.To,
                    EmailBody = x.Body,
                })
                .ToList();
        }


        public async Task<dynamic> GetReportData(BookMarkRequest request, bool fromExport = false)
        {
            return await _getReportDataService.GetReportData(request , fromExport);
        }

        public async Task<AcademyResponse<BookMarkTemplateListDto>> Insert(BookMarkRequest request)
        {
            try
            {
                var bookMarkTemplates = new BookMarkTemplates
                {
                    BookMarkName = request.BookMarkName,
                    Communities = ToCsv(request.Community),
                    Client = ToCsv(request.Client),         
                    GroupByColumns = ToCsv(request.GroupByColumns),
                    Projects = ToCsv(request.Projects),
                    Statuses = ToCsv(request.Statuses),
                    TDC = ToCsv(request.TDC),
                    ReportType = request.ReportType,
                    Trainings = ToCsv(request.Trainings),
                    Seniorities = ToCsv(request.Seniorities),
                    ConfigureColumns = ToCsv(request.SelectColumns),
                    To = request.EmailTo,
                    CC = request.EmailCC == string.Empty ? null : request.EmailCC,
                    Subject = request.EmailSubject,
                    Body = request.EmailBody,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    AreaPaths = ToCsv(request.AreaPaths),
                    PrimaryActivities = ToCsv(request.PrimaryActivities),
                    ActivitieOptions = ToCsv(request.activityOptions),
                    EmployeeId = ToCsv(request.EmployeeId),
                    DateTypeFilter = request.DateTypeFilter,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                };

                var result = await _academyDbContext.BookMarkTemplates.AddAsync(bookMarkTemplates);

                // Save changes to the BookMarkTemplates asynchronously and get the response count.
                var response = await _academyDbContext.SaveChangesAsync();
                BookMarkTemplateListDto result1 = new BookMarkTemplateListDto();
                result1.BookMarkId = 0;

                bool status = false;
                if (response == 1)
                {
                    BookMarkTemplates bookMarkTemplate = _academyDbContext.BookMarkTemplates.Where(x => x.BookMarkId == result.Entity.BookMarkId).FirstOrDefault();
                    result1 = new BookMarkTemplateListDto()
                    {
                        BookMarkId = bookMarkTemplate.BookMarkId,
                        BookMarkName = bookMarkTemplate.BookMarkName,
                        ReportType = bookMarkTemplate.ReportType,
                        EmailCC = bookMarkTemplate.CC,
                        EmailSubject = bookMarkTemplate.Subject,
                        EmailTo = bookMarkTemplate.To,
                        EmailBody = bookMarkTemplate.Body,
                    };
                    status = true;
                }


                AcademyResponse<BookMarkTemplateListDto> response2 = new()
                {
                    Data = result1,
                    Status = HttpStatusCode.OK,
                    Success = status,
                    Message = Messages.SUCCESS_GENERIC
                };

                return response2;
            }
            catch (Exception)
            {
                AcademyResponse<BookMarkTemplateListDto> response2 = new()
                {
                    Data = null,
                    Status = HttpStatusCode.OK,
                    Success = false,
                    Message = Messages.ERROR_Generic
                };
                return response2;
            }
        }

        public async Task<AcademyResponse<BookMarkTemplateListDto>> Modify(BookMarkRequest request)
        {
            BookMarkTemplates bookMarkTemplates = _academyDbContext.BookMarkTemplates.Where(x => x.BookMarkId == request.BookMarkId).FirstOrDefault();

            if (bookMarkTemplates != null)
            {
                bookMarkTemplates.BookMarkName = request.BookMarkName;
                bookMarkTemplates.Trainings = ToCsv(request.Trainings);
                bookMarkTemplates.Communities = ToCsv(request.Community);
                bookMarkTemplates.Client = ToCsv(request.Client);
                bookMarkTemplates.GroupByColumns = ToCsv(request.GroupByColumns);
                bookMarkTemplates.Projects = ToCsv(request.Projects);
                bookMarkTemplates.Statuses = ToCsv(request.Statuses);
                bookMarkTemplates.TDC = ToCsv(request.TDC);
                bookMarkTemplates.ReportType = request.ReportType;
                bookMarkTemplates.Seniorities = ToCsv(request.Seniorities);
                bookMarkTemplates.ConfigureColumns = ToCsv(request.SelectColumns);
                bookMarkTemplates.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                bookMarkTemplates.UpdatedOn = DateTime.UtcNow;
                bookMarkTemplates.AreaPaths = ToCsv(request.AreaPaths);
                bookMarkTemplates.PrimaryActivities = ToCsv(request.PrimaryActivities);
                bookMarkTemplates.ActivitieOptions = ToCsv(request.activityOptions);
                bookMarkTemplates.EmployeeId = ToCsv(request.EmployeeId);
                bookMarkTemplates.DateTypeFilter = request.DateTypeFilter;
                bookMarkTemplates.FromDate = request.FromDate;
                bookMarkTemplates.ToDate = request.ToDate;

                var result = _academyDbContext.BookMarkTemplates.Update(bookMarkTemplates);

                var response = await _academyDbContext.SaveChangesAsync();

                BookMarkTemplateListDto result1 = new BookMarkTemplateListDto();
                result1.BookMarkId = 0;

                bool status = false;
                if (response == 1)
                {
                    BookMarkTemplates bookMarkTemplate = _academyDbContext.BookMarkTemplates.Where(x => x.BookMarkId == request.BookMarkId).SingleOrDefault();
                    result1 = new BookMarkTemplateListDto()
                    {
                        BookMarkId = bookMarkTemplate.BookMarkId,
                        BookMarkName = bookMarkTemplate.BookMarkName,
                        ReportType = bookMarkTemplate.ReportType,
                        EmailCC = bookMarkTemplate.CC,
                        EmailSubject = bookMarkTemplate.Subject,
                        EmailTo = bookMarkTemplate.To,
                        EmailBody = bookMarkTemplate.Body,
                    };
                    status = true;
                }

                AcademyResponse<BookMarkTemplateListDto> response2 = new()
                {
                    Data = result1,
                    Status = HttpStatusCode.OK,
                    Success = status,
                    Message = Messages.SUCCESS_GENERIC
                };

                return response2;
            }
            else
            {
                AcademyResponse<BookMarkTemplateListDto> response2 = new()
                {
                    Data = null,
                    Status = HttpStatusCode.OK,
                    Success = false,
                    Message = Messages.ERROR_BookMarkTemplateNotFound
                };
                return response2;
            }
        }

        public BookMarkTemplatesDto Search(int bookMarkId)
        {
            BookMarkTemplates bookMarkTemplate = _academyDbContext.BookMarkTemplates.Where(x => x.BookMarkId == bookMarkId).SingleOrDefault();
            BookMarkTemplatesDto result = new BookMarkTemplatesDto
            {
                BookMarkId = bookMarkTemplate.BookMarkId,
                BookMarkName = bookMarkTemplate.BookMarkName,
                ReportType = bookMarkTemplate.ReportType,
                Communities = ToStringList(bookMarkTemplate.Communities),
                Client = ToStringList(bookMarkTemplate.Client),
                ConfigureColumns = ToIntList(bookMarkTemplate.ConfigureColumns),
                GroupByColumns = ToIntList(bookMarkTemplate.GroupByColumns),
                Projects = ToStringList(bookMarkTemplate.Projects),
                Seniorities = ToIntList(bookMarkTemplate.Seniorities),
                Statuses = ToIntList(bookMarkTemplate.Statuses),
                TDC = ToStringList(bookMarkTemplate.TDC),
                Trainings = ToIntList(bookMarkTemplate.Trainings),
                EmailTo = bookMarkTemplate.To,
                EmailCC = bookMarkTemplate.CC,
                EmailSubject = bookMarkTemplate.Subject,
                EmailBody = bookMarkTemplate.Body,
                AreaPaths = ToIntList(bookMarkTemplate.AreaPaths),
                PrimaryActivities = ToIntList(bookMarkTemplate.PrimaryActivities),
                ActivityOptions = ToIntList(bookMarkTemplate.ActivitieOptions),
                EmployeeId = ToStringList(bookMarkTemplate.EmployeeId),
                DateTypeFilter = bookMarkTemplate.DateTypeFilter,
                FromDate = bookMarkTemplate.FromDate,
                ToDate = bookMarkTemplate.ToDate,

            };

            if (result.EmployeeId.Any() == true)
                result.Employees = _academyDbContext.Employees.Where(x => x.IsActive && result.EmployeeId.Select(int.Parse).Contains(x.Id))
                                    .Select(x => new EmployeeRoleDto { EmployeeId = x.Id, EmployeeName = x.EmployeeName, GlobantEmailAddress = x.GlobantEmailAddress, Seniority = x.Seniority }).ToList();

            return result;
        }

        public async Task<string> SendReportData(ReportEmailRequest reportEmailRequest)
        {
            try
            {
                var bookMarkTemplate = await _academyDbContext.BookMarkTemplates
                    .FirstOrDefaultAsync(x => x.BookMarkId == reportEmailRequest.BookMarkId);
                var reportTypeName = await _academyDbContext.ReportTypes
                                 .Where(x => x.ReportId == bookMarkTemplate.ReportType)
                                 .Select(x => x.ReportName)
                                 .FirstOrDefaultAsync();
                if (bookMarkTemplate == null)
                    return Messages.ERROR_BookMarkTemplateNotFound;

                // Update template email fields
                bookMarkTemplate.To = reportEmailRequest.EmailTo;
                bookMarkTemplate.CC = reportEmailRequest.EmailCC == string.Empty ? null : reportEmailRequest.EmailCC;
                bookMarkTemplate.Subject = reportEmailRequest.EmailSubject;
                bookMarkTemplate.Body = reportEmailRequest.EmailBody;

                _academyDbContext.BookMarkTemplates.Update(bookMarkTemplate);
                await _academyDbContext.SaveChangesAsync();

                if (!reportEmailRequest.IsDataMore)
                {
                    var reportData = await GenerateReportData(reportEmailRequest.BookMarkId);
                    var emailBody = await ReplaceTable(reportData, bookMarkTemplate.Body, reportTypeName);

                    var emailDto = new SendEmailDto
                    {
                        To = bookMarkTemplate.To,
                        CC = bookMarkTemplate.CC,
                        Subject = bookMarkTemplate.Subject,
                        Body = emailBody
                    };

                    await _sendEmailService.SendEmail(emailDto);
                    return Messages.SUCCESS_GENERIC;
                }
                else
                {
                    var exportRequest = new ExportDetailReportMetadata
                    {
                        Type = ((ExportReportType)bookMarkTemplate.ReportType).ToString(),
                        BookMarkId = bookMarkTemplate.BookMarkId
                    };

                    return await _dashboardService.ExecuteReportJob(exportRequest);
                }
            }
            catch (Exception)
            {
                return Messages.ERROR_Generic;
            }
        }

        public async Task<string> GenerateReportData(int bookMarkId)
        {
            try
            {
                var bookMarkTemplate = await _academyDbContext.BookMarkTemplates
                    .FirstOrDefaultAsync(x => x.BookMarkId == bookMarkId);
                var reportTypeName = await _academyDbContext.ReportTypes
                                 .Where(x => x.ReportId == bookMarkTemplate.ReportType)
                                 .Select(x => x.ReportName)
                                 .FirstOrDefaultAsync();
                ReportTypeName.ReportName = reportTypeName;
                if (bookMarkTemplate == null)
                    return Messages.ERROR_BookMarkTemplateNotFound;

                var bookMarkRequest = BookMarkRequestMapper.ToBookMarkRequest(bookMarkTemplate);

                var reportDataJson = await GetReportData(bookMarkRequest);
                var dataTable = ConvertJsonToDataTable(reportDataJson);

                if (dataTable.Rows.Count == 0)
                    return "No records found.";

                if (dataTable.Rows.Count <= 20)
                    return ConvertDataTableToHTML1(dataTable);

                return "Due to the large number of rows, previewing the data here isn't feasible. To access the full data, please use send an email to receive a Google Drive link.";
            }
            catch (Exception)
            {
                return Messages.ERROR_Generic;
            }
        }

        public async Task<string> ExportGenerateReportDataBookMarkRequest(BookMarkRequest bookMarkRequest)
        {
            try
            {
                DataTable dataTable = new DataTable();
                DataTable dataTableDetails = new DataTable();
                DataTable dataTableSummary = new DataTable();

                DataTable dataTableFilters = CreateExportDataTable(bookMarkRequest);


                if (bookMarkRequest.ReportType == 1 || bookMarkRequest.ReportType == 2)
                {
                    var bookMarkRequestDet = bookMarkRequest;
                    bookMarkRequestDet.ReportType = 1;
                    dataTableDetails = await GetReportData(bookMarkRequestDet,true);
                   

                    var bookMarkRequestSummary = bookMarkRequest;
                    bookMarkRequestSummary.ReportType = 2;
                    dataTableSummary = await GetReportData(bookMarkRequestSummary, true);
                    
                }
                else if (bookMarkRequest.ReportType == 3 || bookMarkRequest.ReportType == 4)
                {
                    var bookMarkRequestDet = bookMarkRequest;
                    bookMarkRequestDet.ReportType = 3;
                    dataTableDetails = await GetReportData(bookMarkRequestDet, true);
                    

                    var bookMarkRequestSummary = bookMarkRequest;
                    bookMarkRequestSummary.ReportType = 4;
                    dataTableSummary = await GetReportData(bookMarkRequestSummary, true);
                
                }
                else
                {
                    dataTable = await GetReportData(bookMarkRequest, true);
                }
                // Create a new Google Sheet (this will be the main sheet)
                var WorkSheetName = $"{bookMarkRequest.BookMarkName}_{_authenticatedUserService.AuthUser.Name}_{DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat)}";
                KeyValuePair<string, string> mainSheet = await _googleApiManager.CreateNewWorksheet(WorkSheetName);
                string mainSheetId = mainSheet.Key;


                // Insert the data into the worksheet
                await _googleApiManager.WriteSheetDirectly(mainSheetId, dataTableFilters, "Configuration");


                await _googleApiManager.MoveFileToAnotherFolder(mainSheetId, _appSetting.ExportReportDriveId);

                if (bookMarkRequest.ReportType == 1 || bookMarkRequest.ReportType == 2 || bookMarkRequest.ReportType == 3 || bookMarkRequest.ReportType == 4)
                {
                    // Insert the data into the first worksheet (Sheet1)
                    await _googleApiManager.WriteSheetDirectly(mainSheetId, dataTableDetails, "DetailReport");


                    // Insert the data into the second worksheet (Sheet2)
                    await _googleApiManager.WriteSheetDirectly(mainSheetId, dataTableSummary, "SummaryReport");

                    // Grant permissions for the  worksheet
                    await _googleApiManager.GrantPermissionTo(mainSheetId, new[] { _authenticatedUserService.AuthUser.GloberEmail });
                }
                else {
                    await _googleApiManager.WriteSheetDirectly(mainSheetId, dataTable, "Report");

                    // Grant permissions for the first worksheet
                    await _googleApiManager.GrantPermissionTo(mainSheetId, new[] { _authenticatedUserService.AuthUser.GloberEmail });
                }
                await _googleApiManager.RemoveSheetFromWorksheet(mainSheetId, "Sheet1");
                // Step 7: Return the URL of the created sheet
                return mainSheet.Value;
            }
            catch (Exception)
            {
                return Messages.ERROR_Generic;
            }
        }

        private DataTable CreateExportDataTable(BookMarkRequest bookMarkRequest)
        {
            if (bookMarkRequest == null)
            {
                throw new ArgumentNullException(nameof(bookMarkRequest), "The BookMarkRequest object is null.");
            }

            DataTable dataTable = new DataTable();

            // Define columns based on the properties of BookMarkRequest only if data is present
            if (!string.IsNullOrWhiteSpace(bookMarkRequest.BookMarkName))
            {
                dataTable.Columns.Add("BookMarkName", typeof(string));
            }
            if (bookMarkRequest.TDC != null && bookMarkRequest.TDC.Count > 0)
            {
                dataTable.Columns.Add("TDC", typeof(string));
            }
            if (bookMarkRequest.Community != null && bookMarkRequest.Community.Count > 0)
            {
                dataTable.Columns.Add("Community", typeof(string));
            }
            if (bookMarkRequest.Trainings != null && bookMarkRequest.Trainings.Count > 0)
            {
                dataTable.Columns.Add("Trainings", typeof(string));
            }
            if (bookMarkRequest.Seniorities != null && bookMarkRequest.Seniorities.Count > 0)
            {
                dataTable.Columns.Add("Seniorities", typeof(string));
            }
            if (bookMarkRequest.Projects != null && bookMarkRequest.Projects.Count > 0)
            {
                dataTable.Columns.Add("Projects", typeof(string));
            }
            if (bookMarkRequest.Statuses != null && bookMarkRequest.Statuses.Count > 0)
            {
                dataTable.Columns.Add("Statuses", typeof(string));
            }
            dataTable.Columns.Add("ReportType", typeof(string)); // Assuming ReportType is always present
            if (bookMarkRequest.AreaPaths != null && bookMarkRequest.AreaPaths.Count > 0)
            {
                dataTable.Columns.Add("AreaPaths", typeof(string));
            }
            if (bookMarkRequest.PrimaryActivities != null && bookMarkRequest.PrimaryActivities.Count > 0)
            {
                dataTable.Columns.Add("PrimaryActivities", typeof(string));
            }
            if (bookMarkRequest.activityOptions != null && bookMarkRequest.activityOptions.Count > 0 && (bookMarkRequest.ReportType==1|| bookMarkRequest.ReportType == 2))
            {
                dataTable.Columns.Add("ActivityOptions", typeof(string));
            }
            if (bookMarkRequest.EmployeeId != null && bookMarkRequest.EmployeeId.Count > 0)
            {
                dataTable.Columns.Add("Employee", typeof(string));
            }
            if (!string.IsNullOrWhiteSpace(bookMarkRequest.DateTypeFilter))
            {
                dataTable.Columns.Add("DateTypeFilter", typeof(string));
                dataTable.Columns.Add("FromDate", typeof(DateOnly));
                dataTable.Columns.Add("ToDate", typeof(DateOnly));
            }
            if (bookMarkRequest.Client != null && bookMarkRequest.Client.Count > 0)
            {
                dataTable.Columns.Add("Client", typeof(string));
            }

            // Create a new row
            DataRow row = dataTable.NewRow();

            // Populate the row with values or blank if no data
            if (dataTable.Columns.Contains("BookMarkName"))
            {
                row["BookMarkName"] = bookMarkRequest.BookMarkName;
            }
            if (dataTable.Columns.Contains("TDC"))
            {
                row["TDC"] = string.Join(", ", bookMarkRequest.TDC);
            }
            if (dataTable.Columns.Contains("Community"))
            {
                row["Community"] = string.Join(", ", bookMarkRequest.Community);
            }
            if (dataTable.Columns.Contains("Trainings"))
            {
                var TrainingName = _academyDbContext.TrainingMasters.Where(x => bookMarkRequest.Trainings.Contains(x.TrainingId)).Select(x => x.TrainingName).ToList();
                row["Trainings"] = string.Join(", ", bookMarkRequest.Trainings);
            }
            if (dataTable.Columns.Contains("Seniorities"))
            {
                var SeniorityName = _academyDbContext.SeniorityMasters.Where(x => bookMarkRequest.Seniorities.Contains(x.SeniorityId)).Select(x => x.SeniorityName).ToList();
                row["Seniorities"] = string.Join(", ", SeniorityName);
            }
            if (dataTable.Columns.Contains("Projects"))
            {
                row["Projects"] = string.Join(", ", bookMarkRequest.Projects);
            }
            if (dataTable.Columns.Contains("Statuses"))
            {
                row["Statuses"] = string.Join(", ", bookMarkRequest.Statuses);
            }
            var Report = _academyDbContext.ReportTypes.Where(x => x.ReportId == bookMarkRequest.ReportType).FirstOrDefault();

            row["ReportType"] = Report.ReportName; // Assuming ReportType is always present
            if (dataTable.Columns.Contains("AreaPaths"))
            {
               var AreaPathName= _academyDbContext.LearningPaths.Where(x => bookMarkRequest.AreaPaths.Contains(x.LearningPathId)).Select(x=>x.LearningPathName).ToList();
                    
                row["AreaPaths"] = string.Join(", ", AreaPathName);
            }
            if (dataTable.Columns.Contains("PrimaryActivities"))
            {
                row["PrimaryActivities"] = string.Join(", ", bookMarkRequest.PrimaryActivities);
            }
            if (dataTable.Columns.Contains("ActivityOptions"))
            {
                var ActivityOption = bookMarkRequest.activityOptions.FirstOrDefault() == 1 ? "Training" : "Activity";
                row["ActivityOptions"] = ActivityOption;
            }
            if (dataTable.Columns.Contains("Employee"))
            {
                var employee = _academyDbContext.Employees.Where(x => x.Id == bookMarkRequest.EmployeeId.FirstOrDefault()).FirstOrDefault();
                row["Employee"] = employee != null ? employee.EmployeeName : string.Empty;
            }
            if (dataTable.Columns.Contains("DateTypeFilter"))
            {
                row["DateTypeFilter"] = bookMarkRequest.DateTypeFilter;
                row["FromDate"] = bookMarkRequest.FromDate ?? default(DateOnly);
                row["ToDate"] = bookMarkRequest.ToDate ?? default(DateOnly);
            }
            if (dataTable.Columns.Contains("Client"))
            {
                row["Client"] = string.Join(", ", bookMarkRequest.Client);
            }

            // Add the row to the DataTable
            dataTable.Rows.Add(row);

          
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

        private static string ToCsv<T>(IEnumerable<T> items)
        {
            return items != null && items.Any()
                ? string.Join(",", items)
                : string.Empty;
        }

        public DataTable ConvertJsonToDataTable(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var records = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonData, options);

            var dataTable = new DataTable();

            if (records == null || records.Count == 0)
                return dataTable;

            // Get all unique keys across all records to avoid schema mismatch
            var allKeys = records.SelectMany(r => r.Keys).Distinct();

            foreach (var key in allKeys)
            {
                dataTable.Columns.Add(key);
            }

            foreach (var record in records)
            {
                var row = dataTable.NewRow();
                foreach (var key in allKeys)
                {
                    row[key] = record.ContainsKey(key) && record[key] != null
                        ? record[key]
                        : DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }


        public string ConvertDataTableToHTML1(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<p>No data available</p>";

            var html = new StringBuilder();

            // Start the table container
            html.AppendLine("<div style=\"width: 100%; margin-top: 20px; overflow-x: auto;\">");
            html.AppendLine("<table style=\"background-color:#ffffff;border-collapse:collapse;width:100%;text-align:left;border-radius:8px;overflow:hidden;\">");

            // Title row 
            html.AppendLine($"<tr><td colspan=\"{dt.Columns.Count}\" style=\"background-color:#004b8d;color:white;font-weight:bold;text-align:center;font-size:16px;padding:10px;border:1px solid #ddd\">AI Expert Training Dashboard</td></tr>");

            // Header row
            html.AppendLine("<tr style=\"background-color:#b7daf8;border:1.5px solid #ddd;height:22px;color:#003366\">");
            foreach (DataColumn column in dt.Columns)
            {
                string header = column.ColumnName.Equals("tdc", StringComparison.OrdinalIgnoreCase) ? "Country" : column.ColumnName;
                html.AppendLine($"<th style=\"padding:5px;width:5%;border:1px solid #ddd;text-align:center;font-size:14px;font-weight:bold;\">{header}</th>");
            }
            html.AppendLine("</tr>");

            // Data rows 
            foreach (DataRow row in dt.Rows)
            {
                int index = 0;
                html.AppendLine("<tr style=\"background-color:#ffffff;font-size:14px\">");
                foreach (var item in row.ItemArray)
                {
                    string textAlign = index == 1 ? "left" : "center";
                    string textWrap = index == 1 ? "normal" : "nowrap";
                    html.AppendLine($"<td style=\"padding:5px;border:0.5px solid #ddd;text-align:{textAlign};font-weight:bold;white-space:{textWrap};\">{item}</td>");
                    index++;
                }
                html.AppendLine("</tr>");
            }

            // End table
            html.AppendLine("</table></div>");
            return html.ToString();
        }



        public async Task<string> ReplaceTable(string generatedHtmlTable, string headerMessage = "", string reportName = "")
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "template2.html");

            if (!File.Exists(templatePath))
                return "Email template not found.";

            string emailBody = await File.ReadAllTextAsync(templatePath);

            if (string.IsNullOrWhiteSpace(emailBody))
                return "Email template is empty.";

            const string bodyPlaceholder = "<EmailBody></EmailBody>";
            const string tablePlaceholder = "<Mytable></Mytable>";
            const string reportNamePlaceholder = "<ReportName></ReportName>";
            if (!emailBody.Contains(bodyPlaceholder))
                return "Body placeholder not found.";

            if (!emailBody.Contains(tablePlaceholder))
                return "Table placeholder not found.";

            // Optional: Convert newlines to <br/> in headerMessage
            if (!string.IsNullOrWhiteSpace(headerMessage))
            {
                headerMessage = headerMessage.Replace("\r\n", "<br/>")
                                             .Replace("\n", "<br/>")
                                             .Replace("\r", "<br/>");
            }

            emailBody = emailBody.Replace(bodyPlaceholder, headerMessage);
            emailBody = emailBody.Replace(tablePlaceholder, generatedHtmlTable);
            emailBody = emailBody.Replace(reportNamePlaceholder, reportName);
            return emailBody;
        }
    }
}