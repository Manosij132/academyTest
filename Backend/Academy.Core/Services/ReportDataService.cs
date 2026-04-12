using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Enums;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Core.Services
{
    public class ReportDataService : IReportDataService
    {
        private readonly IAcademyDbContext _academyDbContext;
        public ReportDataService(IAcademyDbContext academyDbContext)
        {
            _academyDbContext = academyDbContext;
        }
        public async Task<dynamic> GetReportData(BookMarkRequest request, bool fromExport = false)
        {
            var reportTypeEntity = _academyDbContext.ReportTypes
                    .FirstOrDefault(x => x.ReportId == request.ReportType && x.IsActive);

            if (reportTypeEntity == null)
            {
                if (fromExport)
                {
                    return new DataTable();
                }
                else
                {
                    return "";
                }
            }

            if (reportTypeEntity.ReportId == 1 || reportTypeEntity.ReportId == 2)
            {
                if (reportTypeEntity == null)
                    throw new InvalidOperationException("Invalid or inactive report type.");

                var whereClauseBuilder = new StringBuilder();

                void AppendCondition<T>(IEnumerable<T> items, string column, bool quoteStrings = false)
                {
                    if (items?.Any() != true) return;

                    var formattedItems = quoteStrings
                        ? items.Select(i => $"'{i}'")
                        : items.Select(i => i.ToString());

                    whereClauseBuilder.Append($"{column} IN ({string.Join(",", formattedItems)}) AND ");
                }

                AppendCondition(request.TDC, "Employee.Tdc", quoteStrings: true);
                AppendCondition(request.Client, "Employee.Client", quoteStrings: true);
                AppendCondition(request.Community, "Employee.Community", quoteStrings: true);
                AppendCondition(request.Trainings, "TrainingMaster.TrainingId");
                AppendCondition(request.Seniorities, "Employee.SeniorityId");
                AppendCondition(request.Projects, "Employee.Project", quoteStrings: true);
                AppendCondition(request.AreaPaths, "LearningPath.LearningPathId");

                if (request.activityOptions.Any() && request.activityOptions.Contains(2))
                    AppendCondition(request.PrimaryActivities, "EmployeeActivityMap.ActivityId");

                if (request.Statuses.Any() && request.ReportType != (int)ReportsEnum.Compliance)
                {
                    if (request.activityOptions.Any() && request.activityOptions.Contains(1))
                    {
                        AppendCondition(request.Statuses, "EmployeeTrainingMap.TrainingStatusId");
                    }
                    else if (request.activityOptions.Any() && request.activityOptions.Contains(2))
                    {
                        AppendCondition(request.Statuses, "EmployeeActivityMap.StatusId");
                    }
                }

                if (request.EmployeeId?.Any() == true)
                    AppendCondition(request.EmployeeId, "Employee.Id", quoteStrings: true);
                if (request.activityOptions.Any() && request?.DateTypeFilter != null)
                {
                    var from = request.FromDate?.ToString("yyyy-MM-dd");
                    var to = request.ToDate?.ToString("yyyy-MM-dd");

                    string between(string col1, string? col2 = null) => col2 == null
                        ? $" CAST({col1} AS DATE) >= '{from}' AND CAST({col1} AS DATE) <= '{to}' AND "
                        : $" CAST({col1} AS DATE) >= '{from}' AND CAST({col2} AS DATE) <= '{to}' AND ";

                    switch (request?.DateTypeFilter)
                    {
                        case DateTypeFilters.StartDate: // Training or PrimaryActivity
                            if (request.activityOptions.Contains(1))
                                whereClauseBuilder.Append(between("EmployeeTrainingMap.StartDate"));
                            else if (request.activityOptions.Contains(2))
                                whereClauseBuilder.Append(between("EmployeeActivityMap.StartDate"));
                            break;

                        case DateTypeFilters.ActualEndDate: // Training 
                            if (request.activityOptions.Contains(1))
                                whereClauseBuilder.Append(between("EmployeeTrainingMap.ActualEndDate"));
                            break;

                        case DateTypeFilters.ExpectedEndDate: // Training 
                            if (request.activityOptions.Contains(1))
                                whereClauseBuilder.Append(between("EmployeeTrainingMap.ExpectedEndDate"));
                            break;

                        case DateTypeFilters.StartDateAndActualEndDate: // Training
                            if (request.activityOptions.Contains(1))
                                whereClauseBuilder.Append(between("EmployeeTrainingMap.StartDate", "EmployeeTrainingMap.ActualEndDate"));
                            break;

                        case DateTypeFilters.StartDateAndExpectedEndDate: // Training                                
                            if (request.activityOptions.Contains(1))
                                whereClauseBuilder.Append(between("EmployeeTrainingMap.StartDate", "EmployeeTrainingMap.ExpectedEndDate"));
                            break;

                        case DateTypeFilters.EndDate: // PrimaryActivity 
                            if (request.activityOptions.Contains(2))
                                whereClauseBuilder.Append(between("EmployeeActivityMap.EndDate"));
                            break;

                        case DateTypeFilters.StartDateAndEndDate: // PrimaryActivity 
                            if (request.activityOptions.Contains(2))
                                whereClauseBuilder.Append(between("EmployeeActivityMap.StartDate", "EmployeeActivityMap.EndDate"));
                            break;
                    }
                }
                var whereClause = whereClauseBuilder.ToString().TrimEnd();

                // Remove last "AND" if present
                if (whereClause.EndsWith("AND"))
                {
                    whereClause = whereClause[..^3].TrimEnd();
                }

                var param1 = new SqlParameter("@SelectColumns", string.Join(",", request.SelectColumns));
                var param2 = new SqlParameter("@WhereClause", whereClause);
                var param4 = new SqlParameter("@ActivityType", string.Join(",", request.activityOptions));

                List<SqlParameter> parameters = new() { param1, param2 };

                if (request.ReportType == (int)ReportsEnum.Summmary || request.ReportType == (int)ReportsEnum.Compliance)
                {
                    var param3 = new SqlParameter("@GroupByColumns", string.Join(",", request.GroupByColumns));
                    parameters.Add(param3);
                }

                if (request.ReportType == (int)ReportsEnum.Compliance && request.Statuses.Any())
                    parameters.Add(new SqlParameter("@TrainingStatusId", request.Statuses[0]));

                parameters.Add(param4);

                if (fromExport)
                { 
                    var result = await _academyDbContext.ExecuteStoredProcedureDataTableAsync(reportTypeEntity.StoredProcName, parameters.ToArray());
                    return result;

                }
                else
                {
                    var result = await _academyDbContext.ExecuteStoredProcedureAsync(reportTypeEntity.StoredProcName, parameters.ToArray());
                    return result;
                }
                
            }
            if (reportTypeEntity.ReportId == 3 || reportTypeEntity.ReportId == 4)
            {
                if (reportTypeEntity == null)
                    throw new InvalidOperationException("Invalid or inactive report type.");

                var whereClauseBuilder = new StringBuilder();

                void AppendCondition<T>(IEnumerable<T> items, string column, bool quoteStrings = false)
                {
                    if (items?.Any() != true) return;

                    var formattedItems = quoteStrings
                        ? items.Select(i => $"'{i}'")
                        : items.Select(i => i.ToString());

                    whereClauseBuilder.Append($"{column} IN ({string.Join(",", formattedItems)}) AND ");
                }
                    AppendCondition(request.TDC, "e.Tdc", quoteStrings: true);
                    AppendCondition(request.Client, "e.Client", quoteStrings: true);
                    AppendCondition(request.Community, "e.Community", quoteStrings: true);
                    AppendCondition(request.Projects, "e.Project", quoteStrings: true);
                    AppendCondition(request.AreaPaths, "lp.LearningPathId");
                

                
                if (request.EmployeeId?.Any() == true)
                    AppendCondition(request.EmployeeId, "e.Id", quoteStrings: true);

                var whereClause = whereClauseBuilder.ToString().TrimEnd();

                // Remove last "AND" if present
                if (whereClause.EndsWith("AND"))
                {
                    whereClause = whereClause[..^3].TrimEnd();
                }

                var param2 = new SqlParameter("@WhereClause", whereClause);

                List<SqlParameter> parameters = new() { param2 };

                if (fromExport)
                {
                    var result = await _academyDbContext.ExecuteStoredProcedureDataTableAsync(reportTypeEntity.StoredProcName, parameters.ToArray());
                    return result;

                }
                else
                {
                    var result = await _academyDbContext.ExecuteStoredProcedureAsync(reportTypeEntity.StoredProcName, parameters.ToArray());
                    return result;
                }
            }

            if (reportTypeEntity.ReportId == 5)
            {
                if (reportTypeEntity == null)
                    throw new InvalidOperationException("Invalid or inactive report type.");

                var whereClauseBuilder = new StringBuilder();

                void AppendCondition<T>(IEnumerable<T> items, string column, bool quoteStrings = false)
                {
                    if (items?.Any() != true) return;

                    var formattedItems = quoteStrings
                        ? items.Select(i => $"'{i}'")
                        : items.Select(i => i.ToString());

                    whereClauseBuilder.Append($"{column} IN ({string.Join(",", formattedItems)}) AND ");
                }
                AppendCondition(request.TDC, "TDC", quoteStrings: true);
                AppendCondition(request.Community, "Community", quoteStrings: true);
                AppendCondition(request.Projects, "Project", quoteStrings: true);
                AppendCondition(request.Client, "GloberAccount", quoteStrings: true);




                if (request.EmployeeId?.Any() == true)
                {
                    var Employee = _academyDbContext.Employees.Where(x => x.Id == request.EmployeeId.FirstOrDefault()).FirstOrDefault();
                    AppendCondition(new List<string>() { Employee.EmployeeName }, "EmployeeName", quoteStrings: true);
                }
                var whereClause = whereClauseBuilder.ToString().TrimEnd();

                // Remove last "AND" if present
                if (whereClause.EndsWith("AND"))
                {
                    whereClause = whereClause[..^3].TrimEnd();
                }

                var param2 = new SqlParameter("@WhereClause", whereClause);

                List<SqlParameter> parameters = new() { param2 };

                if (fromExport)
                {
                    var result = await _academyDbContext.ExecuteStoredProcedureDataTableAsync(reportTypeEntity.StoredProcName, parameters.ToArray());
                    return result;

                }
                else
                {
                    var result = await _academyDbContext.ExecuteStoredProcedureAsync(reportTypeEntity.StoredProcName, parameters.ToArray());
                    return result;
                }
            }

            if (fromExport)
            {
                return new DataTable();
            }
            else
            {
                return "";
            }

        }
    }
}
