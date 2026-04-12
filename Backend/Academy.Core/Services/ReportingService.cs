using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Shared.DTO;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Microsoft.Extensions.Options;
using System.Data;
using static Academy.Shared.Exceptions.DomainErrors;

namespace Academy.Core.Services
{
    public class ReportingService : IReportService
    {
        private readonly AppSetting _appSetting;
        private readonly IGoogleApiManager _googleApiManager;
        private readonly IAdoClient<AcademyDbSetting> _adoClient;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPredicateFactory _predicateFactory;

        public ReportingService(IOptions<AppSetting> appSetting, IGoogleApiManager googleApiManager, IAdoClient<AcademyDbSetting> adoClient, IAuthenticatedUserService authenticatedUserService, IPredicateFactory predicateFactory)
        {
            _appSetting = appSetting.Value;
            _googleApiManager = googleApiManager;
            _adoClient = adoClient;
            _authenticatedUserService = authenticatedUserService;
            _predicateFactory = predicateFactory;
        }

        public async Task ExportReport(string reportKey)
        {
            IList<IList<object>> values = [];
            var reportMetadata = _appSetting.ExportReports.FirstOrDefault(x => x.Key.Equals(reportKey, StringComparison.OrdinalIgnoreCase));
            if (reportMetadata != null)
            {
                var data = await _adoClient.ExecuteReaderAsync(reportMetadata.Command, null);
                if (data != null && data.Rows.Count > 0)
                {
                    var cols = new List<object>();
                    foreach (DataColumn column in data.Rows)
                    {
                        cols.Add(column.ColumnName);
                    }
                    values.Add(cols);
                    foreach (DataRow row in data.Rows)
                    {
                        var rows = new List<object>();
                        for (int i = 0; i < cols.Count; i++)
                        {
                            rows.Add(row[i]);
                        }
                        values.Add(rows);
                    }
                    await _googleApiManager.ClearData(reportMetadata.SpreadsheetId, reportMetadata.SheetId);
                    await _googleApiManager.UpdateRow(values, reportMetadata.SpreadsheetId, $"{reportMetadata.SheetName}!{reportMetadata.Range}");
                }
            }
        }

        public async Task<Result<DojoActivityReportResponse>> FetchAllDojoActivitiesForReport(FetchDojoActivityRequest dojoActivityRequest)
        {
            DojoActivityReportResponse dojoActivityReportResponse = new();
            List<DojoActivityReport> dojoActivityReports = new List<DojoActivityReport>();

            var predicateBuilder = _predicateFactory
               .PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            bool isDateSelected = false;

            bool isPermitted = predicateBuilder.CanGetFilteredPagedDojoDetails();
            if (!isPermitted)
            {
                return Result.Failure<DojoActivityReportResponse>(Authorization.UnauthorizedAccess);
            }

            Dictionary<string, object> iParams = new();

            if (dojoActivityRequest.Country != null && dojoActivityRequest.Country.Any())
            {
                var countries = string.Join(",", dojoActivityRequest.Country);
                iParams.Add("@Country", countries);
            }
            if (dojoActivityRequest.Community != null && dojoActivityRequest.Community.Any())
            {
                var Communities = string.Join(",", dojoActivityRequest.Community);
                iParams.Add("@Community", Communities);
            }
            if (dojoActivityRequest.Account != null && dojoActivityRequest.Account.Any())
            {
                var accounts = string.Join(",", dojoActivityRequest.Account);
                iParams.Add("@Account", accounts);
            }
            if (dojoActivityRequest.AiStudio != null && dojoActivityRequest.AiStudio.Any())
            {
                var aiStudios = string.Join(",", dojoActivityRequest.AiStudio);
                iParams.Add("@AiStudio", aiStudios);
            }
            if (!string.IsNullOrEmpty(dojoActivityRequest.DojoStartDate))
            {
                iParams.Add("@DojoStartDate", dojoActivityRequest.DojoStartDate.Substring(0, 10));
                isDateSelected = true;

            }
            if (!string.IsNullOrEmpty(dojoActivityRequest.DojoEndDate))
            {
                iParams.Add("@DojoEndDate", string.Concat(dojoActivityRequest.DojoEndDate.Substring(0, 10), " 23:59:59.0"));
            }

            iParams.Add("@IsPrimary", dojoActivityRequest.IsPrimaryRecord);

            if (!string.IsNullOrWhiteSpace(dojoActivityRequest.SearchText))
            {
                iParams.Add("@SearchText", dojoActivityRequest.SearchText);
            }

            var dataset = await _adoClient.XecuteReaderDataSetAsync("usp_GetDojoActivityReport", iParams);

            if (dataset.Tables.Count > 0)
            {
                var reader = dataset.Tables[0];

                foreach (DataRow row in reader.Rows)
                {
                    dojoActivityReportResponse.Items.Add(new DojoActivityReport
                    {
                        GlobantEmailAddress = row.Field<string>("GlobantEmailAddress"),
                        EmployeeName = row.Field<string>("EmployeeName"),
                        AiStudio = row.Field<string>("AiStudio"),
                        Account = row.Field<string>("Account"),
                        DojoStartDate = row.Field<DateTime?>("DojoStartDate"),
                        DojoEndDate = row.Field<DateTime?>("DojoEndDate"),
                        ActivityName = row.Field<string>("ActivityName"),
                        ActivityDescription = row.Field<string>("ActivityDescription"),
                        StartDate = row.Field<DateTime?>("StartDate"),
                        EndDate = row.Field<DateTime?>("EndDate"),
                        Type = row.Field<string>("Type"),
                        Country = row.Field<string>("Country"),
                        BaseLocation = row.Field<string>("BaseLocation"),
                        Seniority = row.Field<string>("Seniority"),
                        Community = row.Field<string>("Community"),
                        Priority = row.Field<decimal?>("ActivityPriority") == null ? 2.0m : row.Field<decimal>("ActivityPriority"),
                        IsActive = row.Field<int>("DojoActiveStatus") == 1 ? true : false,
                        IsEmployeeActive = row.Field<string>("EmployeeActiveStatus"),
                        ActivityComment = row.Field<string>("ActivityComment"),
                        DojoProjectName = row.Field<string>("DojoProjectName")
                        //StatusId = row.Field<byte>("StatusId")
                    });
                }
            }

            if (dataset.Tables.Count > 1 && dataset.Tables[1].Rows.Count > 0)
            {
                DataRow row = dataset.Tables[1].Rows[0];

                dojoActivityReportResponse.DojoEngagedCount = row.Field<int>("Engaged");
                dojoActivityReportResponse.DojoNotEngagedCount = row.Field<int>("NotEngaged");
                dojoActivityReportResponse.CurrentDojoCount = row.Field<int>("TotalDojo");
                dojoActivityReportResponse.NonAssignable = row.Field<int>("NonAssignable");
            }

            if (dataset.Tables.Count > 2 && dataset.Tables[2].Rows.Count > 0)
            {
                var reader = dataset.Tables[2];

                foreach (DataRow row in reader.Rows)
                {
                    dojoActivityReportResponse.ActivityCounts.Add(new DojoActivityCount
                    {
                        ActivityCount = row.Field<int>("ActiveEmployeesInActivity"),
                        ActivityName = row.Field<string>("ActivityName")
                    });
                }
            }

            if (dataset.Tables.Count > 3 && dataset.Tables[3].Rows.Count > 0)
            {
                var reader = dataset.Tables[3];

                foreach (DataRow row in reader.Rows)
                {
                    dojoActivityReportResponse.NonAssignableItems.Add(new DojoActivityReport
                    {
                        GlobantEmailAddress = row.Field<string>("GlobantEmailAddress"),
                        EmployeeName = row.Field<string>("EmployeeName"),
                        DojoStartDate = row.Field<DateTime?>("DojoStartDate"),
                        Country = row.Field<string>("Country"),
                        BaseLocation = row.Field<string>("BaseLocation"),
                        Seniority = row.Field<string>("Seniority"),
                        Community = row.Field<string>("Community"),
                        IsEmployeeActive = row.Field<string>("EmployeeActiveStatus"),
                        DojoProjectName = row.Field<string>("DojoProjectName")
                    });
                }
            }

            dojoActivityReportResponse.TotalCount = dojoActivityReportResponse.Items.Count;
            
            //paging
            var pagedList = dojoActivityReportResponse.Items.Skip((dojoActivityRequest.PageIndex - 1) * dojoActivityRequest.PageSize).Take(dojoActivityRequest.PageSize).ToList();
            dojoActivityReportResponse.ExportItems.AddRange(dojoActivityReportResponse.Items);
            dojoActivityReportResponse.Items.Clear();
            dojoActivityReportResponse.Items = pagedList;

            dojoActivityReportResponse.PageSize = dojoActivityRequest.PageSize;
            dojoActivityReportResponse.PageIndex = dojoActivityRequest.PageIndex;
            dojoActivityReportResponse.TotalPages = (int)Math.Ceiling((double)dojoActivityReportResponse.TotalCount / dojoActivityRequest.PageSize);

            return Result.Success(dojoActivityReportResponse);
        }

        public async Task<Result<ExportDojoActivitiesReportResponse>> ExportDojoActivitiesReport(ExportDojoActivityRequest dojoActivityRequest)
        {
            ExportDojoActivitiesReportResponse exportDojoActivitiesReportResponse = new();
            IList<IList<object>> exportDetailedtLists = new List<IList<object>>();
            IList<IList<object>> exportSummaryLists = new List<IList<object>>();
            IList<IList<object>> exportFilters = new List<IList<object>>();
            IList<IList<object>> exportEngagementCount = new List<IList<object>>();
            IList<IList<object>> exportProjectWiseActiveEmployeeCount = new List<IList<object>>();
            IList<IList<object>> exportNonAssignableDetails = new List<IList<object>>();
            IList<IList<object>> exportAiStudioSummary = new List<IList<object>>();
            exportDetailedtLists.Add(new List<object>
            {
                "Glober",
                "Email",
                "AI Studio",
                "Account",
                "Community",
                "Seniority",
                "Activity Name",
                "Activity Description",
                "Comment",
                "Activity Type",
                "Activity Priority",
                "Dojo Start",
                "Dojo End",
                "Dojo Status",
                "Employee Status",
                "Project",
                "Activity Start",
                "Activty End",
                "Country",
                "Base Location"
            });
            exportSummaryLists.Add(new List<object>
            {
                "Activity Name",
                "Engaged Globers"
            });
            exportFilters.Add(new List<object>
            {
                "Community",
                "Country",
                "AiStudio",
                "Account",
                "Report Type",
                "Dojo Start",
                "Dojo End"
            });
            exportEngagementCount.Add(new List<object>
            {
                "DOJO Engagement Details"
            });
            exportProjectWiseActiveEmployeeCount.Add(new List<object>
            {
                "Project",
                "Activite",
                "Resigned",
                "Total"
            });
            exportNonAssignableDetails.Add(new List<object>
            {
                "Glober",
                "Email",
                "Community",
                "Seniority",
                "Dojo Start",
                "Employee Status",
                "Project",
                "Country",
                "Base Location"
            });
            exportAiStudioSummary.Add(new List<object>
            {
                "AI Studio",
                "Count Of Glober"
            });
            foreach (var activity in dojoActivityRequest.DetailedReport)
            {
                exportDetailedtLists.Add(new List<object> {
                    activity.EmployeeName,
                    activity.GlobantEmailAddress,
                    activity.AiStudio,
                    activity.Account,
                    activity.Community,
                    activity.Seniority,
                    activity.ActivityName,
                    activity.ActivityDescription,
                    activity.ActivityComment,
                    activity.Type,
                    activity.Priority,
                    activity.DojoStartDate.HasValue ? activity.DojoStartDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    activity.DojoEndDate.HasValue ? activity.DojoEndDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    activity.IsActive ? "Active":string.Empty,
                    activity.IsEmployeeActive,
                    activity.DojoProjectName,
                    activity.StartDate.HasValue ? activity.StartDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    activity.EndDate.HasValue ? activity.EndDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    activity.Country,
                    activity.BaseLocation
                });
            }
            if (dojoActivityRequest.NonAssignableItems != null)
            {
                foreach (var activity in dojoActivityRequest.NonAssignableItems)
                {
                    exportNonAssignableDetails.Add(new List<object> {
                    activity.EmployeeName,
                    activity.GlobantEmailAddress,
                    activity.Community,
                    activity.Seniority,
                    activity.DojoStartDate.HasValue ? activity.DojoStartDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    activity.IsEmployeeActive,
                    activity.DojoProjectName,
                    activity.Country,
                    activity.BaseLocation
                });
                }
            }
            foreach (var activity in dojoActivityRequest.ActivitySummary)
            {
                exportSummaryLists.Add(new List<object> {
                    activity.ActivityName,
                    activity.ActivityCount
                });
            }
            exportFilters.Add(new List<object>
            {
                (dojoActivityRequest.Filter.Community != null && dojoActivityRequest.Filter.Community.Any())
                    ? string.Join(",", dojoActivityRequest.Filter.Community)
                    : "All",
                (dojoActivityRequest.Filter.Country != null && dojoActivityRequest.Filter.Country.Any())
                    ? string.Join(",", dojoActivityRequest.Filter.Country)
                    : "All",
                (dojoActivityRequest.Filter.AiStudio != null && dojoActivityRequest.Filter.AiStudio.Any())
                    ? string.Join(",", dojoActivityRequest.Filter.AiStudio)
                    : "All",
                (dojoActivityRequest.Filter.Account != null && dojoActivityRequest.Filter.Account.Any())
                    ? string.Join(",", dojoActivityRequest.Filter.Account)
                    : "All",
                dojoActivityRequest.Filter.IsPrimaryRecord ? "Primary Report" :"Detailed Report",
                dojoActivityRequest.Filter.DojoStartDate,
                dojoActivityRequest.Filter.DojoEndDate,
            });
            foreach (var engagement in dojoActivityRequest.EngagementCounts)
            {
                exportEngagementCount.Add(new List<object> {
                    engagement.Name,
                    engagement.Count
                });
            }

            var dojoProjectCounts = dojoActivityRequest.DetailedReport.GroupBy(dr => dr.DojoProjectName).Select(g =>
            {
                return new
                {
                    ProjectName = g.Key,
                    TotalCount = g.Count(),
                    IsActiveCount = g.Count(g => g.IsEmployeeActive == "Active"),
                    IsResignedCount = g.Count(g => g.IsEmployeeActive == "Resigned")
                };
            });
            foreach (var dojoProject in dojoProjectCounts)
            {
                exportProjectWiseActiveEmployeeCount.Add(new List<object> {
                    dojoProject.ProjectName,
                    dojoProject.IsActiveCount,
                    dojoProject.IsResignedCount,
                    dojoProject.TotalCount
                });
            }
            var aiStudioSummary = dojoActivityRequest.DetailedReport
                .GroupBy(x => x.AiStudio)
                .Select(g => new
                {
                    AiStudio = g.Key,
                    Count = g.Count()
                });
            int grandTotal = 0;
            foreach (var item in aiStudioSummary)
            {
                exportAiStudioSummary.Add(new List<object>
                    {
                        item.AiStudio,
                        item.Count
                    });
                grandTotal += item.Count;
            }
            exportAiStudioSummary.Add(new List<object>
            {
                "Grand Total",
                grandTotal
            });

            string WorksheetId = string.Empty;
            //var folderId = await _googleApiManager.CreateFileOnDrive("DojoEngagementReport", "application/vnd.google-apps.folder");
            //string exportReportFileId = await _googleApiManager.CreateFileOnDrive("test1", "text/plain",_appSetting.DojoEngagementReportFolderId);
            KeyValuePair<string, string> sheet = await _googleApiManager.CreateNewWorksheet($"DojoEngagement_{_authenticatedUserService.AuthUser.Name}_{DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat)}");
            WorksheetId = sheet.Key;
            exportDojoActivitiesReportResponse.FileUrl = sheet.Value;
            int? configurationSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Configuration");
            int? summarySheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Summary");
            int? detailedReportSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Detailed Report");
            int? summarizedReportSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Summarized Report");
            int? nonAssignableReportSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Non Assignable Report");
            int? aiStudioSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "AI Studio Summary");
            await _googleApiManager.RemoveSheetFromWorksheet(WorksheetId, "Sheet1");
            await _googleApiManager.MoveFileToAnotherFolder(WorksheetId, _appSetting.DojoEngagementReportFolderId);
            await _googleApiManager.GrantPermissionTo(WorksheetId, [_authenticatedUserService.AuthUser.GloberEmail]);
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Detailed Report", exportDetailedtLists, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Summary", exportSummaryLists, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Configuration", exportFilters, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Summarized Report", exportProjectWiseActiveEmployeeCount, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Configuration", exportEngagementCount, $"A{4}:B");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Non Assignable Report", exportNonAssignableDetails, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "AI Studio Summary", exportAiStudioSummary, $"A{1}:B");
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, configurationSheetId ?? 0, 3, 4, 0, 1, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, configurationSheetId ?? 0, 0, 1, 0, 5, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, summarySheetId ?? 0, 0, 1, 0, 2, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, detailedReportSheetId ?? 0, 0, 1, 0, 15, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, detailedReportSheetId ?? 0, 0, 1, 0, 18, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, summarizedReportSheetId ?? 0, 0, 1, 0, 4, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, nonAssignableReportSheetId ?? 0, 0, 1, 0, 15, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, nonAssignableReportSheetId ?? 0, 0, 1, 0, 18, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, aiStudioSheetId ?? 0, 0, 1, 0, 4, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.MergeColumns(WorksheetId, "Configuration", 3, 0, 2);
            return Result.Success(exportDojoActivitiesReportResponse);
        }
        public async Task<Result<AssignedThroughTrainingReportResponse>> FetchAssignThroughTrainingReport(FetchAssignedThroughTrainingRequest request)
        {
            AssignedThroughTrainingReportResponse getDojoDetailsResponse = new();

            var predicateBuilder = _predicateFactory
               .PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            bool isDateSelected = false;

            bool isPermitted = predicateBuilder.CanGetFilteredPagedDojoDetails();
            if (!isPermitted)
            {
                return Result.Failure<AssignedThroughTrainingReportResponse>(Authorization.UnauthorizedAccess);
            }

            Dictionary<string, object> iParams = new();

            if (request.Country != null && request.Country.Any())
            {
                var countries = string.Join(",", request.Country);
                iParams.Add("@Country", countries);
            }
            if (request.Community != null && request.Community.Any())
            {
                var communities = string.Join(",", request.Community);
                iParams.Add("@Community", communities);
            }
            if (request.Account != null && request.Account.Any())
            {
                var accounts = string.Join(",", request.Account);
                iParams.Add("@Account", accounts);
            }
            if (request.AiStudio != null && request.AiStudio.Any())
            {
                var aiStudios = string.Join(",", request.AiStudio);
                iParams.Add("@AiStudio", aiStudios);
            }
            if (!string.IsNullOrEmpty(request.DojoStartDate))
            {
                iParams.Add("@DojoStartDate", request.DojoStartDate.Substring(0, 10));
            }
            if (!string.IsNullOrEmpty(request.DojoEndDate))
            {
                iParams.Add("@DojoEndDate", string.Concat(request.DojoEndDate.Substring(0, 10), " 23:59:59.0"));
            }

            var dataset = await _adoClient.XecuteReaderDataSetAsync("usp_GetAssignedThroughTrainingReport", iParams);

            if (dataset.Tables.Count > 0)
            {
                var reader = dataset.Tables[0];

                foreach (DataRow row in reader.Rows)
                {
                    getDojoDetailsResponse.Items.Add(new AssignedThroughTrainingInfo
                    {
                        DojoDetailId = row["DojoDetailId"] == DBNull.Value ? 0 : Convert.ToInt32(row["DojoDetailId"]),
                        EmployeeId = row["EmployeeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["EmployeeId"]),
                        EmployeeName = row["EmployeeName"] == DBNull.Value ? string.Empty : Convert.ToString(row["EmployeeName"]),
                        GlobantEmailAddress = row["GlobantEmailAddress"] == DBNull.Value ? string.Empty : Convert.ToString(row["GlobantEmailAddress"]),
                        DojoStartDate = row["DojoStartDate"] == DBNull.Value ? null : Convert.ToDateTime(row["DojoStartDate"]),
                        DojoEndDate = row["DojoEndDate"] == DBNull.Value ? null : Convert.ToDateTime(row["DojoEndDate"]),
                        AssignedThroughTraining = row["AssignedThroughTraining"] == DBNull.Value ? null : Convert.ToBoolean(row["AssignedThroughTraining"]),
                        Community = row["Community"] == DBNull.Value ? string.Empty : Convert.ToString(row["Community"]),
                        AiStudio = row["AiStudio"] == DBNull.Value ? string.Empty : Convert.ToString(row["AiStudio"]),
                        Account = row["Account"] == DBNull.Value ? string.Empty : Convert.ToString(row["Account"]),
                        Comments = row["Comments"] == DBNull.Value ? string.Empty : Convert.ToString(row["Comments"]),
                        TicketNumber = row["TicketNumber"] == DBNull.Value || Convert.ToInt32(row["TicketNumber"]) == 0 ? null : Convert.ToInt32(row["TicketNumber"]),
                    });
                }

                foreach (var item in getDojoDetailsResponse.Items)
                {
                    if (item.AssignedThroughTraining == true)
                    {
                        getDojoDetailsResponse.AssignedThroughTrainingCount++;
                    }
                    else if (item.AssignedThroughTraining == false)
                    {
                        getDojoDetailsResponse.NotAssignedThroughTrainingCount++;
                    }
                }
                getDojoDetailsResponse.TotalCount = getDojoDetailsResponse.Items.Count;

                //paging
                var pagedList = getDojoDetailsResponse.Items.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                getDojoDetailsResponse.ExportItems.AddRange(getDojoDetailsResponse.Items);
                getDojoDetailsResponse.Items.Clear();
                getDojoDetailsResponse.Items = pagedList;

                getDojoDetailsResponse.PageSize = request.PageSize;
                getDojoDetailsResponse.PageIndex = request.PageIndex;
                getDojoDetailsResponse.TotalPages = (int)Math.Ceiling((double)getDojoDetailsResponse.TotalCount / request.PageSize);

            }
            return Result.Success(getDojoDetailsResponse);
        }

        public async Task<Result<ExportDojoActivitiesReportResponse>> ExportAssignThroughTrainingReport(ExportAssignedThroughTrainingRequest request)
        {
            ExportDojoActivitiesReportResponse exportDojoActivitiesReportResponse = new();
            IList<IList<object>> exportDetailedtLists = new List<IList<object>>();
            IList<IList<object>> exportFilters = new List<IList<object>>();
            IList<IList<object>> exportReportCounts = new List<IList<object>>();
            IList<IList<object>> exportAiStudioSummary = new List<IList<object>>();            
            exportDetailedtLists.Add(new List<object>
            {
                "Glober",
                "Email",
                "AI Studio",
                "Account",
                "Community",
                "Dojo Start",
                "Dojo End",
                "AssignedThroughTraining",
                "Comments",
                "Ticket"
            });

            exportFilters.Add(new List<object>
            {
                "Community",
                "Country",
                "AiStudio",
                "Account",
                "Dojo Start",
                "Dojo End"
            });
            exportReportCounts.Add(new List<object>
            {
                "Glober assigned through training Details"
            });
            exportAiStudioSummary.Add(new List<object>
            {
                "AI Studio",
                "Count Of Glober"
            });
            foreach (var detail in request.DetailedReport)
            {
                exportDetailedtLists.Add(new List<object> {
                    detail.EmployeeName,
                    detail.GlobantEmailAddress,
                    detail.AiStudio,
                    detail.Account,
                    detail.Community,
                    detail.DojoStartDate.HasValue ? detail.DojoStartDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    detail.DojoEndDate.HasValue ? detail.DojoEndDate.Value.ToString("MM-dd-yyyy") : string.Empty,
                    detail.AssignedThroughTraining.HasValue ? detail.AssignedThroughTraining.Value : string.Empty,
                    detail.Comments,
                    detail.TicketNumber.HasValue ? detail.TicketNumber.Value : string.Empty,
                });
            }

            exportFilters.Add(new List<object>
            {
                (request.Filter.Community != null && request.Filter.Community.Any()) ? string.Join(",", request.Filter.Community) : "All",
                (request.Filter.Country != null && request.Filter.Country.Any()) ? string.Join(",", request.Filter.Country) : "All",
                (request.Filter.AiStudio != null && request.Filter.AiStudio.Any()) ? string.Join(",", request.Filter.AiStudio) : "All",
                (request.Filter.Account != null && request.Filter.Account.Any()) ? string.Join(",", request.Filter.Account) : "All",
                request.Filter.DojoStartDate,
                request.Filter.DojoEndDate,
            });

            foreach (var item in request.ReportCounts)
            {
                exportReportCounts.Add(new List<object> {
                    item.Name,
                    item.Count
                });
            }

            var aiStudioSummary = request.DetailedReport
                .GroupBy(x => x.AiStudio)
                .Select(g => new
                {
                    AiStudio = g.Key,
                    Count = g.Count()
                });
            int grandTotal = 0;
            foreach (var item in aiStudioSummary)
            {
                exportAiStudioSummary.Add(new List<object>
                    {
                        item.AiStudio,
                        item.Count
                    });
                grandTotal += item.Count;
            }
            exportAiStudioSummary.Add(new List<object>
            {
                "Grand Total",
                grandTotal
            });

            string WorksheetId = string.Empty;
            //var folderId = await _googleApiManager.CreateFileOnDrive("DojoEngagementReport", "application/vnd.google-apps.folder");
            //string exportReportFileId = await _googleApiManager.CreateFileOnDrive("test1", "text/plain",_appSetting.DojoEngagementReportFolderId);
            KeyValuePair<string, string> sheet = await _googleApiManager.CreateNewWorksheet($"AssignedThroughTrainingReport_{_authenticatedUserService.AuthUser.Name}_{DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat)}");
            WorksheetId = sheet.Key;
            exportDojoActivitiesReportResponse.FileUrl = sheet.Value;
            int? configurationSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Configuration");
            int? detailedReportSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "Detailed Report");
            int? aiStudioSheetId = await _googleApiManager.AddNewEmptySheetAsync(WorksheetId, "AI Studio Summary");
            await _googleApiManager.RemoveSheetFromWorksheet(WorksheetId, "Sheet1");
            await _googleApiManager.MoveFileToAnotherFolder(WorksheetId, _appSetting.DojoEngagementReportFolderId);
            await _googleApiManager.GrantPermissionTo(WorksheetId, [_authenticatedUserService.AuthUser.GloberEmail]);
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Detailed Report", exportDetailedtLists, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Configuration", exportFilters, $"A{1}:Z");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "Configuration", exportReportCounts, $"A{4}:B");
            await _googleApiManager.InsertFormulaByRange(WorksheetId, "AI Studio Summary", exportAiStudioSummary, $"A{1}:B");
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, configurationSheetId ?? 0, 3, 4, 0, 1, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, configurationSheetId ?? 0, 0, 1, 0, 4, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, detailedReportSheetId ?? 0, 0, 1, 0, 8, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.SetCellBackgroundAndForeGroundColor(WorksheetId, aiStudioSheetId ?? 0, 0, 1, 0, 4, 0, 1, 1, 0, 0, 0, true, true);
            await _googleApiManager.MergeColumns(WorksheetId, "Configuration", 3, 0, 2);
            return Result.Success(exportDojoActivitiesReportResponse);
        }
    }
}