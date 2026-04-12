using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using System.Data;
using static Academy.Shared.Exceptions.DomainErrors;

namespace Academy.Core.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPredicateFactory _predicateFactory;
        private readonly IAdoClient<AcademyDbSetting> _academyDB;
        private readonly IEmployeeService _employeeService;
        private readonly IRepository<EmailDump> _repositoryEmail;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IDojoService _dojoService;
        public ActivityService(IAuthenticatedUserService authenticatedUserService,
           IAdoClient<AcademyDbSetting> academyDB, IPredicateFactory predicateFactory, IEmployeeService employeeService, IUnitOfWork unitOfWork,
           IAcademyDbContext academyDbContext, IDojoService dojoService)
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _academyDB = academyDB;
            _predicateFactory = predicateFactory;
            _employeeService = employeeService;
            _repositoryEmail = _unitOfWork.GetRepository<EmailDump>();
            _academyDbContext = academyDbContext;
            _dojoService = dojoService;
        }


        public async Task<Result<List<DojoActivity>>> FetchAllActivities(string employeeEmails)
        { 
            var employees = await _dojoService.FetchDojoActivityByIds(employeeEmails.Split(',').ToList());
            var empIds = employees.Value.Select(e => e.EmployeeId).ToList();
 
            // Fetching active employee activities based on specific conditions
            var employeeActivityMap = _academyDbContext.EmployeeActivityMaps
                .Where(a => a.IsActive && empIds.Contains(a.EmployeeId))
                .Join(_academyDbContext.ActivityMasters.Where(b => b.IsActive && b.ActivityId == 2),
                      a => a.ActivityId,
                      b => b.ActivityId,
                      (a, b) => new { a, b })
                .Where(joined => joined.a.StartDate >= _academyDbContext.DojoDetails
                    .Where(c => c.EmployeeId == joined.a.EmployeeId)
                    .OrderByDescending(c => c.DojoStartDate)
                    .Select(c => c.DojoStartDate)
                    .FirstOrDefault())
                .Select(joined => new
                {
                    joined.a.EmployeeId,
                    joined.a.StartDate,
                    joined.a.IsActive,
                    joined.a.CreatedOn,
                    joined.a.ActivityDetail
                })
                .ToList();

            var result = employees.Value
                .GroupJoin(employeeActivityMap,
                           emp => emp.EmployeeId,
                           act => act.EmployeeId,
                           (emp, activityGroup) => new DojoActivity
                           {
                               EmployeeId = emp.EmployeeId,
                               EmployeeName = emp.EmployeeName,
                               Comments = emp.Comments,
                               TicketNumber = emp.TicketNumber,
                               DojoStartDate = emp.DojoStartDate,
                               DojoDetailId = emp.DojoDetailId,
                               GlobantEmailAddress= emp.GlobantEmailAddress,
                               ProjectName=emp.ProjectName,
                               Client=emp.Client,
                               PositionTitle=emp.PositionTitle,
                               Skills=emp.Skills,
                               ActivityDetail = activityGroup.Select(c => c.ActivityDetail).ToList()
                           })
                .ToList();

                return Result.Success(result.ToList());
        }

        public async Task<Result<List<EmployeeActivity>>> FetchActivityById(int employeeId)
        {
            List<EmployeeActivity> employeeActivities = new();

            var predicateBuilder = _predicateFactory
                .PredicateGenerator(_authenticatedUserService.AuthUser.Roles);

            bool isPermitted = predicateBuilder.CanFetchActivitiesByEmployeeId();
            if (!isPermitted)
            {
                return Result.Failure<List<EmployeeActivity>>(Authorization.UnauthorizedAccess);
            }

            // Build input parameters (only required ones)
            Dictionary<string, object> iParams = new()
            {
                { "@EmployeeId", employeeId }
            };

            var reader = await _academyDB.ExecuteReaderAsync("usp_GetActivitiesByEmployeeId", iParams);

            foreach (DataRow row in reader.Rows)
            {
                EmployeeActivity activity = new()
                {
                    EmployeeActivityId = Convert.ToInt32(row["EmployeeActivityId"]),
                    EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                    ActivityId = Convert.ToInt16(row["ActivityId"]),
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    EndDate = row["EndDate"] == DBNull.Value ? null : Convert.ToDateTime(row["EndDate"]),
                    StatusId = Convert.ToByte(row["StatusId"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedBy = Convert.ToInt32(row["CreatedBy"]),
                    CreatedOn = Convert.ToDateTime(row["CreatedOn"]),
                    UpdatedBy = row["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(row["UpdatedBy"]),
                    UpdatedOn = row["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(row["UpdatedOn"]),
                    ActivityName = row["ActivityName"].ToString(),
                    ActivityDetail = row["ActivityDetail"].ToString(),
                    Comments = row["Comments"].ToString(),
                    ActivitySource = row["ActivitySource"].ToString(),
                    Account = row["Account"].ToString()
                };
                employeeActivities.Add(activity);
            }

            return employeeActivities;
        }
        public async Task<Result<int>> InsertOrUpdateEmployeeActivities(EmployeeActivityMapRequest request)
        {
            if (request == null)
            {
                return Result.Failure<int>(Common.NullOrEmptyValue(nameof(request)));
            }
            else if(!string.IsNullOrWhiteSpace(request.Action) && string.Equals(request.Action,"delete"))
            {
                request.IsActive = false;
            }

            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            bool isPermitted = predicateBuilder.CanInsertOrUpdateEmployeeActivities();
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }

            // Fetch authenticated user ID
            int loggedInUserId = _authenticatedUserService.AuthUser.Id;
            string loggedInUserEmail = _authenticatedUserService.AuthUser.GloberEmail;

            var result = await _employeeService.FetchById(request.EmployeeId);

            if (result.IsFailure)
            {
                return Result.Failure<int>(result.Error);
            }
            
            var employee = result.Value;

            // Build input parameters (only required ones)
            Dictionary<string, object> iParams = new()
            {
                 { "@EmployeeActivityId",request.EmployeeActivityId },
                 { "@EmployeeId", request.EmployeeId },
                 { "@ActivityId", request.ActivityId },
                 { "@ActivitySource", request.ActivitySource },
                 { "@ActivityDetail",request.ActivityDetail },
                 { "@Comments",request.Comments },
                 { "@IsActive", request.IsActive },
                 { "@StartDate", request.StartDate},
                 { "@EndDate", request.EndDate},
                 { "@StatusId", request.Status},
                 { "@recordInsertOrUpdateBy", loggedInUserId },
                 { "@recordInsertOrUpdateDate", DateTime.UtcNow }, // Or adjust to IST if needed
                 { "@account", request.Account }
            };

            DataTable resultTable = await _academyDB.ExecuteReaderAsync("dbo.usp_InsertOrUpdateEmployeeActivityMap", iParams);

            if (resultTable.Rows.Count > 0 && resultTable.Columns.Contains("EmployeeActivityId"))
            {
                string action = Convert.ToString(resultTable.Rows[0]["Result"]);
                int employeeActivityId = Convert.ToInt32(resultTable.Rows[0]["EmployeeActivityId"]);
                int activityId = Convert.ToInt32(resultTable.Rows[0]["ActivityId"]);

                // add entry in email dump table
                if (!string.IsNullOrEmpty(action))
                {
                    EmailDump email = new()
                    {
                        CreatedBy = _authenticatedUserService.AuthUser.Id,
                        CreatedOn = DateTime.UtcNow,
                        Subject = action.ToLower().Equals("inserted", StringComparison.CurrentCultureIgnoreCase) ? "New Activity Assignment" : "Activity Details Updated",
                        Template = action.ToLower().Equals("inserted", StringComparison.CurrentCultureIgnoreCase) ? "ACTIVITY_ASSIGNED" : "ACTIVITY_UPDATED",
                        Cc = loggedInUserEmail,
                        To = employee.GlobantEmailAddress,
                        PlainText = Convert.ToString(employeeActivityId),
                        IsActive = true
                    };

                    _repositoryEmail.Insert(email);
                    
                    await _unitOfWork.SaveChangesAsync();
                }

                return activityId;
            }
            else
            {
                return Result.Failure<int>(Activity.ActivityMappingFailure);
            }
        }

        public async Task<Result<int>> BulkInsertActivities(List<EmployeeActivityMapRequest> employeeActivities)
        {
            if (employeeActivities.Count <= 0)
            {
                return Result.Failure<int>(Activity.ActivitiesCountZero);
            }

            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            bool isPermitted = predicateBuilder.CanInsertBulkActivities();
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }

            // Create a DataTable to hold the data for the TVP
            DataTable tvpTable = new DataTable();
            tvpTable.Columns.Add("EmployeeId", typeof(int));
            tvpTable.Columns.Add("ActivityId", typeof(short));
            tvpTable.Columns.Add("ActivityDetail", typeof(string));
            tvpTable.Columns.Add("ActivitySource", typeof(string));
            tvpTable.Columns.Add("StartDate", typeof(DateTime));
            tvpTable.Columns.Add("EndDate", typeof(DateTime));
            tvpTable.Columns.Add("Account", typeof(string));

            foreach (var req in employeeActivities)
            {
                tvpTable.Rows.Add(
                    req.EmployeeId,
                    req.ActivityId,
                    req.ActivitySource,
                    req.ActivityDetail,
                    req.StartDate,
                    req.EndDate.HasValue ? (object)req.EndDate.Value : DBNull.Value,
                    req.Account
                );
            }

            Dictionary<string, object> iParams = new()
            {
                { "@EmployeeActivityMaps", tvpTable},
                { "@LoggedInUserId", _authenticatedUserService.AuthUser.Id },
                { "@EmailSubject","New Activity Assignment"},
                { "@EmailTemplate", "ACTIVITY_ASSIGNED"},
            };

            DataTable resultTable = await _academyDB.ExecuteReaderAsync("dbo.usp_BulkInsertEmployeeActivityMaps", iParams);

            if (resultTable.Rows.Count > 0)
            {
                return Result.Success(resultTable.Rows.Count);
            }
            else
            {
                return Result.Failure<int>(Activity.BulkInsertFailed);
            }
        }
    }
}
