using Academy.Core;
using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Core.PredicateBuilder;
using Academy.Core.Services;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static Academy.Shared.Exceptions.DomainErrors;

namespace Academy.ApplicationCore.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPredicateFactory _predicateFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EmployeeTrainingMap> _repositoryEmployeeTrainingMap;
        private readonly IEmployeeService _employeeService;
        private readonly ISkillAndTrainingService _skillAndTrainingService;
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IAdoClient<AcademyDbSetting> _academyDbAdoClient;
        private readonly IRepository<Dashboard> _repositoryDashboard;
        private readonly IRepository<EmailDump> _repositoryEmail;
        private readonly IRepository<Comment> _repositoryComment;
        private readonly AbstractAdminPredicate predicateBuilder;
        private readonly IRepository<JobRequest> _repositoryJobrequest;
        private readonly IRepository<DojoDetail> _repositoryDojoDetail;
        private readonly IRepository<TrainingMaster> _repositoryTrainingMaster;
        private readonly IRepository<TrainingStatusMaster> _repositoryTrainingStatusMaster;
        public readonly IRepository<SkillMaster> _repositorySkillMaster;
        private readonly IGoogleApiManager _googleApiManager;
        private readonly AppSetting _appSetting;
        private readonly IRepository<EmployeeDocumentTypeMaster> _repositoryEmployeeDocumentTypeMaster;
        private readonly IReportService _reportService;

        public DashboardService(
            IAuthenticatedUserService authenticatedUserService,
            IUnitOfWork unitOfWork,
            IEmployeeService employeeService,
            IOptions<AppSetting> appSetting,
            IAcademyDbContext academyDbContext,
            IAdoClient<AcademyDbSetting> academyDbAdoClient,
            IPredicateFactory predicateFactory,
            IGoogleApiManager googleApiManager,
            ISkillAndTrainingService skillAndTrainingService,
            IReportService reportService)
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _repositoryEmployeeTrainingMap = _unitOfWork.GetRepository<EmployeeTrainingMap>();
            _employeeService = employeeService;
            _academyDbContext = academyDbContext;
            _academyDbAdoClient = academyDbAdoClient;
            _repositoryDashboard = _unitOfWork.GetRepository<Dashboard>();
            _repositoryEmail = _unitOfWork.GetRepository<EmailDump>();
            _repositoryComment = _unitOfWork.GetRepository<Comment>();
            _predicateFactory = predicateFactory;
             predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            _repositoryJobrequest = _unitOfWork.GetRepository<JobRequest>();
            _repositoryDojoDetail = _unitOfWork.GetRepository<DojoDetail>();
            _googleApiManager = googleApiManager;
            _appSetting = appSetting.Value;
            _skillAndTrainingService = skillAndTrainingService;
            _repositoryTrainingMaster = _unitOfWork.GetRepository<TrainingMaster>();
            _repositoryTrainingStatusMaster = _unitOfWork.GetRepository<TrainingStatusMaster>();
            _repositorySkillMaster = _unitOfWork.GetRepository<SkillMaster>();
            _repositoryEmployeeDocumentTypeMaster = _unitOfWork.GetRepository<EmployeeDocumentTypeMaster>();
            _reportService = reportService;
        }

        /// <summary>
        /// Fetches the dashboard details for a specified employee.
        /// </summary>
        /// <param name="employeeId">The unique identifier of the employee whose dashboard is to be fetched.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="DashboardResponse"/> 
        /// object with the dashboard details of the specified employee.
        /// </returns>
        public async Task<Result<DashboardResponse>> FetchDashboard(int employeeId)
        {
            // Get Employee By Id.
            // This will return only that employee which loggedin user can see
            var result = await _employeeService.FetchById(employeeId);

            if (result.IsFailure)
                return Result.Failure<DashboardResponse>(result.Error);

            var employee = result.Value;
            Dictionary<string, object> iParams = new()
            {
                { DbConstants.PARAM_EMPLOYEE_ID, employee.Id }
            };

            DataTable dashboardResult = await _academyDbAdoClient.ExecuteReaderAsync(DbConstants.FETCH_DASHBOARD_TRAININGS, iParams);
            List<TrainingResponse> trainings = dashboardResult.ToList<TrainingResponse>();
            Dashboard dashboard = await _repositoryDashboard.GetFirstOrDefaultAsync(predicate: x => x.EmployeeId.Equals(employeeId));
            var dojoDetails = _academyDbContext.DojoDetails.FirstOrDefault(x => x.EmployeeId == employeeId && x.IsActive == true);
            DashboardResponse response = new();
            response.Employee.EmployeeName = employee.EmployeeName;
            response.Employee.Tdc = employee.Tdc;
            response.Employee.EmployeeEmail = employee.GlobantEmailAddress;
            response.Employee.EmployeeId = employee.Id;
            response.Employee.CareerMentorEmail = employee.BetterMeLeaderEmail;
            response.Employee.Client = employee.Client;
            response.Employee.Seniority = employee.Seniority.ToLower();
            response.Employee.BaseLocation = employee.BaseLocation;
            response.Employee.Project = employee.Project;
            response.Employee.Position = employee.Position;
            response.Employee.ImageUrl = employee.Image;
            response.Employee.TotalTrainings = trainings.Count();
            response.Employee.InProgressTrainings = trainings.Count(x => x.TrainingStatusId == (int)TrainingStatus.Pending || x.TrainingStatusId == (int)TrainingStatus.Ongoing);
            response.Employee.CompletedTrainings = trainings.Count(x => x.TrainingStatusId == (int)TrainingStatus.Completed);
            response.Employee.TrainingCompletetionScore = dashboard.TrainingScore;
            response.Employee.ProficiencyScore = dashboard.ProficiencyScore;
            response.Employee.Status = dashboard.Status;

            if (dojoDetails != null)
            {
                response.Employee.DojoGexLeaderEmail = dojoDetails.DojoGexLeaderEmail ?? "";
                response.Employee.DojoDetailId = dojoDetails.DojoDetailId;
            }
            //if (result.Count > 0)
            //{
            //    response.Employee.Status = result.All(x => x.TrainingStatusId == (int)TrainingStatus.Pending) ? TrainingStatus.Pending.ToString() :
            //                                result.Any(x => x.TrainingStatusId == (int)TrainingStatus.Ongoing) ? TrainingStatus.Ongoing.ToString() :
            //                                result.All(x => x.TrainingStatusId == (int)TrainingStatus.Completed) ? TrainingStatus.Completed.ToString() :
            //                                TrainingStatus.Deferred.ToString();
            //    response.Employee.Score = Math.Round(Convert.ToDecimal(result.Count(x => x.TrainingStatusId == (int)TrainingStatus.Completed) / Convert.ToDecimal(result.Count) * 100), 2);
            //}
            //else
            //{
            //    response.Employee.Status = "#N/A";
            //    response.Employee.Score = 0;
            //}
            response.Trainings = trainings;
            return response;
        }
        /// <summary>
        /// Retrieves a list of dashboards based on the specified data request options.
        /// </summary>
        /// <param name="dataRequestOptions">An object containing filtering, paging, and sorting options to apply when retrieving the dashboard list.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="Dashboard"/> objects
        /// that match the criteria specified in the <paramref name="dataRequestOptions"/>.
        /// </returns>
        public async Task<Result<IPagedList<Dashboard>>> FetchTrackerList(DataRequestOptions dataRequestOptions)
        {
            IPagedList<Dashboard> list = null;
            // Get the Predicate based on Logged in User Role
            Expression<Func<Dashboard, bool>> predicate = predicateBuilder.FetchDashboard();
            
            // Check if FilterOptions are not null
            if (dataRequestOptions.FilterOptions != null && dataRequestOptions.FilterOptions.Count > 0)
            {
                // Get the Actual Lambda Expression of predicate and Store it in variable
                Expression combinedExpression = predicate.Body;
                // Create a node of Dashbaord Type
                ParameterExpression parameter = Expression.Parameter(typeof(Dashboard), "e");

                // Call the Replace Visitor and get the Combined Expression
                ReplaceParameterVisitor visitor = new(predicate.Parameters[0], parameter);
                combinedExpression = visitor.Visit(combinedExpression);

                // GROUP FILTERS BY COLUMN
                var groupedFilters = dataRequestOptions.FilterOptions.GroupBy(f => f.FilterBy);

                foreach (var group in groupedFilters)
                {
                    Expression groupExpression = null;

                    foreach (var filter in group)
                    {
                        Expression newBody = null;

                        if (filter.FilterBy == "ProposedDojoGxLeader")
                        {
                            Expression<Func<Dashboard, bool>> gxLeaderPredicate = filter.FilterValue switch
                            {
                                "InDojo" => d => !string.IsNullOrEmpty(d.ProposedDojoGxLeader) && d.IsProposedGxLeaderOnDojo,
                                "OutDojo" => d => !string.IsNullOrEmpty(d.ProposedDojoGxLeader) && !d.IsProposedGxLeaderOnDojo,
                                _ => d => string.IsNullOrEmpty(d.ProposedDojoGxLeader)
                            };

                            var gxLeaderVisitor = new ReplaceParameterVisitor(gxLeaderPredicate.Parameters[0], parameter);
                            newBody = gxLeaderVisitor.Visit(gxLeaderPredicate.Body);
                        }
                        else
                        {
                            // Special case: Project == DOJO
                            if (filter.FilterBy == "Project" && filter.FilterValue?.ToString() == "DOJO")
                            {
                                // Get the list of projects from DojoProjectConfiguration
                                var dojoProjects = _academyDbContext.DojoProjectConfigurations
                                    .Where(dpc => dpc.IsActive)
                                    .Select(d => d.ProjectName).ToList();

                                // Build an expression: e => dojoProjects.Contains(e.Project)
                                var projectProperty = Expression.Property(parameter, "Project");
                                var dojoProjectsConst = Expression.Constant(dojoProjects);
                                var containsMethod = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
                                newBody = Expression.Call(dojoProjectsConst, containsMethod, projectProperty);
                            }
                            else
                            {
                                Expression<Func<Dashboard, bool>> newCondition =
                                    LinqExtensions.DynamicFilterBy<Dashboard>(
                                        filter.FilterBy,
                                        filter.FilterValue,
                                        ExpressionType.Equal);

                                ReplaceParameterVisitor newVisitor = new(newCondition.Parameters[0], parameter);
                                newBody = newVisitor.Visit(newCondition.Body);
                            }
                        }
                        // OR inside same column
                        if (groupExpression == null)
                            groupExpression = newBody;
                        else
                            groupExpression = Expression.OrElse(groupExpression, newBody);
                    }
                    // AND between different columns
                    combinedExpression = Expression.AndAlso(combinedExpression, groupExpression);
                }
                // Creating lambda from Expression
                predicate = Expression.Lambda<Func<Dashboard, bool>>(combinedExpression, parameter);
            }

            // SEARCH LOGIC
            if (!string.IsNullOrWhiteSpace(dataRequestOptions.SearchText))
            {
                string searchText = dataRequestOptions.SearchText.Trim();
                // Get the Actual Lambda Expression of predicate and Store it in variable
                Expression combinedExpression = predicate.Body;
                // Create a node of Dashbaord Type
                ParameterExpression parameter = Expression.Parameter(typeof(Dashboard), "e");

                // Call the Replace Visitor and get the Combined Expression
                ReplaceParameterVisitor visitor = new(predicate.Parameters[0], parameter);
                combinedExpression = visitor.Visit(combinedExpression);


                if (dataRequestOptions.SearchText.ToLower().EndsWith("@globant.com"))
                {
                    Expression<Func<Dashboard, bool>> emailMatchCondition = LinqExtensions.Equals<Dashboard>("EmployeeEmail", searchText);
                    Expression emailMatchBody = emailMatchCondition.Body;
                    ReplaceParameterVisitor emailMatchVisitor = new(emailMatchCondition.Parameters[0], parameter);
                    emailMatchBody = emailMatchVisitor.Visit(emailMatchBody);
                    // Combined the Expression and New Condition
                    combinedExpression = Expression.AndAlso(combinedExpression, emailMatchBody);

                    predicate = Expression.Lambda<Func<Dashboard, bool>>(combinedExpression, parameter);
                }
                else
                {
                    // This means v.IsActive == true
                    MemberExpression isActiveProperty = Expression.Property(parameter, "IsActive");
                    Expression isActiveCondition = Expression.Equal(isActiveProperty, Expression.Constant(true, typeof(bool)));

                    // EmployeeName IS NOT NULL
                    MemberExpression employeeNameProperty = Expression.Property(parameter, nameof(Dashboard.EmployeeName));
                    Expression employeeNameNotNull = Expression.NotEqual(employeeNameProperty, Expression.Constant(null, typeof(string)));

                    // EmployeeEmail IS NOT NULL
                    MemberExpression employeeEmailProperty = Expression.Property(parameter, "EmployeeEmail");
                    Expression employeeEmailNotNull = Expression.NotEqual(employeeEmailProperty, Expression.Constant(null, typeof(string)));

                    // Combine the AND conditions
                    Expression combinedAndConditions = Expression.AndAlso(
                            Expression.AndAlso(isActiveCondition, employeeNameNotNull),
                            employeeEmailNotNull
                            );

                    // --- Start building the OR conditions for LIKE expressions ---
                    // For LIKE, we use string.StartsWith and string.Contains methods in LINQ.
                    // Get MethodInfo for string.StartsWith(string)
                    MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                    // Get MethodInfo for string.Contains(string)
                    MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                    // [v].[EmployeeName] LIKE N'a.bhagwat@globant%'
                    // Equivalent to v.EmployeeName.StartsWith(searchText)
                    ConstantExpression searchTextStartsWithConstant = Expression.Constant(searchText, typeof(string));
                    Expression employeeNameStartsWith = Expression.Call(employeeNameProperty, startsWithMethod, searchTextStartsWithConstant);

                    // [v].[EmployeeEmail] LIKE N'a.bhagwat@globant%'
                    // Equivalent to v.EmployeeEmail.StartsWith(searchText)
                    Expression employeeEmailStartsWith = Expression.Call(employeeEmailProperty, startsWithMethod, searchTextStartsWithConstant);

                    // [v].[EmployeeName] LIKE N'%a.bhagwat@globant%'
                    // Equivalent to v.EmployeeName.Contains(searchText)
                    ConstantExpression searchTextContainsConstant = Expression.Constant(searchText, typeof(string));
                    Expression employeeNameContains = Expression.Call(employeeNameProperty, containsMethod, searchTextContainsConstant);

                    // [v].[EmployeeEmail] LIKE N'%a.bhagwat@globant%'
                    // Equivalent to v.EmployeeEmail.Contains(searchText)
                    Expression employeeEmailContains = Expression.Call(employeeEmailProperty, containsMethod, searchTextContainsConstant);

                    // Combine the OR conditions:
                    // (employeeNameStartsWith OR employeeEmailStartsWith OR employeeNameContains OR employeeEmailContains)
                    Expression orCondition = Expression.OrElse(
                            Expression.OrElse(employeeNameStartsWith, employeeEmailStartsWith),
                            Expression.OrElse(employeeNameContains, employeeEmailContains));
                    // Final WHERE clause: (AND conditions) AND (OR conditions)
                    combinedExpression = Expression.AndAlso(combinedExpression, Expression.AndAlso(combinedAndConditions, orCondition));

                    predicate = Expression.Lambda<Func<Dashboard, bool>>(combinedExpression, parameter);
                }
            }

            // Adding Dynamic Soring
            if (dataRequestOptions.SortOptions == null)
            {
                dataRequestOptions.SortOptions = new();
                dataRequestOptions.SortOptions.SortBy = nameof(Dashboard.EmployeeEmail);
                dataRequestOptions.SortOptions.SortByDescending = false;
            }
            dataRequestOptions.SortOptions.SortBy = string.IsNullOrWhiteSpace(dataRequestOptions.SortOptions.SortBy) ? nameof(Dashboard.EmployeeEmail) : dataRequestOptions.SortOptions.SortBy;

            bool isEngagedSort = dataRequestOptions.SortOptions.SortBy == "Engaged";
            if (!isEngagedSort)
            {
                list = await _repositoryDashboard.GetPagedListAsync(
                    predicate: predicate,
                    pageIndex: dataRequestOptions.PagingOptions.PageIndex,
                    pageSize: dataRequestOptions.PagingOptions.PageSize,
                    orderBy: q => q.DynamicOrderBy(
                        dataRequestOptions.SortOptions.SortBy,
                        dataRequestOptions.SortOptions.SortByDescending));
            }
            else
            {
                // FETCH ALL DATA (no paging)
                var query = _repositoryDashboard.GetAll();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                var items = await query.ToListAsync();

                // EXISTING DOJO ENGAGEMENT ENRICHMENT
                var dojoActivitiesReportData = await _reportService.FetchAllDojoActivitiesForReport(new FetchDojoActivityRequest());

                var response = new AcademyResponse<DojoActivityReportResponse>
                {
                    Data = dojoActivitiesReportData.IsSuccess ? dojoActivitiesReportData.Value : new(),
                    Error = dojoActivitiesReportData.IsFailure ? dojoActivitiesReportData.Error : null
                };

                var exportData = response.Data.ExportItems;

                // Convert to lookup for fast search
                var exportLookup = exportData
                    .GroupBy(x => x.GlobantEmailAddress?.ToLower())
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var emp in items) // dashboard employees
                {
                    var email = emp.EmployeeEmail?.ToLower();
                   
                    if (email != null && exportLookup.TryGetValue(email, out var dojoRecords))
                    {
                        // Email found → check activity
                        if (dojoRecords.Any(x => !string.IsNullOrEmpty(x.ActivityName)))
                            emp.Engaged = "Engaged";
                        else
                            emp.Engaged = "Not Engaged";
                    }                 
                    else
                    {
                        // Email not present in dojo data
                        emp.Engaged = "Non Assignable";
                    }
                }

                // APPLY ENGAGED SORT
                if (!dataRequestOptions.SortOptions.SortByDescending)
                {
                    items = items
                        .OrderBy(x => x.Engaged == "Not Engaged" ? 0 :
                                      x.Engaged == "Engaged" ? 1 : 2)
                        .ThenBy(x => x.EmployeeEmail)
                        .ToList();
                }
                else
                {
                    items = items
                        .OrderByDescending(x => x.Engaged == "Not Engaged" ? 0 :
                                                x.Engaged == "Engaged" ? 1 : 2)
                        .ThenBy(x => x.EmployeeEmail)
                        .ToList();
                }

                // APPLY PAGING AFTER SORT
                int pageIndex = dataRequestOptions.PagingOptions.PageIndex;
                int pageSize = dataRequestOptions.PagingOptions.PageSize;

                list = items.ToPagedList(pageIndex, pageSize);
            }
            return Result.Success(list);
        }

        public async Task<Result<int>> ExtendEndDate(ExtendEndDateRequest request)
        {
            var result = await _employeeService.FetchById(request.EmployeeId);

            if (result.IsFailure)
                return Result.Failure<int>(result.Error);

            bool isPermitted = predicateBuilder.CanExtendEndDate(result.Value);
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }

            EmployeeTrainingMap record = await _repositoryEmployeeTrainingMap.GetFirstOrDefaultAsync(predicate: x => x.EmployeeTrainingId.Equals(request.EmployeeTrainingId));

            if (request.NewExpectedDate < DateTime.UtcNow || request.NewExpectedDate < record.ActualEndDate)
            {
                return Result.Failure<int>(DashboardErrors.EndDateIsLessThanStartDate);
            }

            record.UpdatedOn = DateTime.UtcNow;
            record.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            record.ExpectedEndDate = request.NewExpectedDate;

            _repositoryEmployeeTrainingMap.Update(record);

            int count = await _unitOfWork.SaveChangesAsync();
            return count;
        }

        public async Task<Result<int>> PostComment(CommentRequest request)
        {
            var result = await _employeeService.FetchById(request.EmployeeId);

            if (result.IsFailure)
                return Result.Failure<int>(result.Error);

            bool isPermitted = predicateBuilder.CanPerformTrackerTasks(result.Value);
            if (!isPermitted)
            {
                throw new InvalidOperationException(Messages.ERROR_InSufficientPermissions);
            }
            Comment comment = new()
            {
                CommentText = request.CommentText,
                IsActive = true,
                EmployeeId = request.EmployeeId,
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CreatedOn = DateTime.UtcNow,
            };


            await _repositoryComment.InsertAsync(comment);
            int count = await _unitOfWork.SaveChangesAsync();

            return count;
        }

        public async Task<Result<int>> UpdateDojoGxLeadxer(DojoGxLeadxerRequest request)
        {
            bool dojoGexLeaderChanged = false;

            var result = await _employeeService.FetchById(request.EmployeeId);

            if (result.IsFailure)
                return Result.Failure<int>(result.Error);

            bool isPermitted = predicateBuilder.CanUpdateDojoGxLeadxer();
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }

            var employeeDojoDetails = await _academyDbContext.DojoDetails.Where(x => x.EmployeeId == request.EmployeeId && x.IsActive == true).ToListAsync();

            if (employeeDojoDetails.Count <= 0)
            {
                // add new record
                await _repositoryDojoDetail.InsertAsync(new DojoDetail
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    EmployeeId = request.EmployeeId,
                    DojoStartDate = request.DojoStartDate,
                    DojoEndDate = request.DojoEndDate,
                    DojoGexLeaderEmail = request.DojoGexLeaderEmail,
                    IsActive = true,
                });
                dojoGexLeaderChanged = true;
            }
            else
            {
                var dojoDetail = employeeDojoDetails.FirstOrDefault(x => x.DojoGexLeaderEmail == request.DojoGexLeaderEmail);

                if (dojoDetail == null)
                {
                    // case 1
                    foreach (var item in employeeDojoDetails)
                    {
                        item.IsActive = false;
                    }

                    // add new record
                    await _repositoryDojoDetail.InsertAsync(new DojoDetail
                    {
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = _authenticatedUserService.AuthUser.Id,
                        EmployeeId = request.EmployeeId,
                        DojoStartDate = request.DojoStartDate,
                        DojoEndDate = request.DojoEndDate,
                        DojoGexLeaderEmail = request.DojoGexLeaderEmail,
                        IsActive = true,
                    });
                    dojoGexLeaderChanged = true;

                    _repositoryDojoDetail.Update(employeeDojoDetails);
                }
                else
                {
                    // case 2
                    dojoDetail.DojoEndDate = request.DojoEndDate;
                    _repositoryDojoDetail.Update(dojoDetail);
                }
            }

            if (dojoGexLeaderChanged)
            {
                // add entries into email dump
                var globarEmailDump = new EmailDump
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Subject = "DOJO GX Leader Change Notification",
                    Template = "DOJOGX_GLOBER",
                    Cc = request.DojoGexLeaderEmail,
                    To = request.DojoGexGlobarEmail,
                };
                _repositoryEmail.Insert(globarEmailDump);

                var dojoLeaderEmailDump = new EmailDump
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Subject = "New DOJO GX Leader Assignment",
                    Template = "DOJOGX_LEADER",
                    Cc = request.DojoGexGlobarEmail,
                    To = request.DojoGexLeaderEmail,
                };
                _repositoryEmail.Insert(dojoLeaderEmailDump);

            }

            int count = await _unitOfWork.SaveChangesAsync();

            return count;
        }

        public async Task<Result<List<CommentResponse>>> FetchComments(int employeeId, bool latestOnly = false)
        {
            var result = await _employeeService.FetchById(employeeId);

            if (result.IsFailure)
                return Result.Failure<List<CommentResponse>>(result.Error);

            bool isPermitted = predicateBuilder.CanPerformTrackerTasks(result.Value);

            if (!isPermitted)
            {
                return Result.Failure<List<CommentResponse>>(Authorization.UnauthorizedAccess);
            }

            List<CommentResponse> response = [];
            if (!latestOnly)
            {
                response = [.. (from c in _academyDbContext.Comments
                                      join e in _academyDbContext.Employees
                                      on c.CreatedBy equals e.Id
                                      where c.EmployeeId == employeeId
                                      select new CommentResponse()
                                      {
                                          CommentBy = e.GlobantEmailAddress,
                                          CommentDate = c.CreatedOn,
                                          CommentText = c.CommentText,
                                          CommentByImage = e.Image,
                                          CommentByEmpId = e.Id
                                      }).OrderBy(x=>x.CommentDate)];
            }
            else
            {
                Dictionary<string, object> iParams = new()
                    {
                        {DbConstants.PARAM_EMPLOYEE_ID,employeeId }
                    };
                var reader = await _academyDbAdoClient.ExecuteReaderAsync(DbConstants.FETCH_LATEST_COMMENT, iParams);
                response = reader.ToList<CommentResponse>();
            }
            return response;

        }

        public async Task<Result<int>> ChangeStatus(ChangeStatusRequest request)
        {
            var result = await _employeeService.FetchById(request.EmployeeId);

            if (result.IsFailure)
                return Result.Failure<int>(result.Error);

            bool isPermitted = predicateBuilder.CanPerformTrackerTasks(result.Value);
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }
            var record = await _repositoryEmployeeTrainingMap.GetFirstOrDefaultAsync(predicate: x => x.EmployeeTrainingId.Equals(request.EmployeeTrainingId));
            if (record == null)
            {
                return Result.Failure<int>(Common.NullOrEmptyValue(nameof(request)));
            }
            record.UpdatedOn = DateTime.UtcNow;
            record.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            record.TrainingStatusId = request.TrainingStatusId;

            //var a1 = (record.TrainingStatusId == (int)TrainingStatus.Pending && request.TrainingStatusId == (int)TrainingStatus.Ongoing);
            //var a2 = (record.TrainingStatusId == (int)TrainingStatus.Ongoing && request.TrainingStatusId == (int)TrainingStatus.Completed);
            //var a3 = (record.TrainingStatusId != (int)TrainingStatus.Completed && request.TrainingStatusId == (int)TrainingStatus.Deferred);


            //if (a1 || a2 || a3)
            //{
            //    record.TrainingStatusId = request.TrainingStatusId;
            //}
            //else
            //{
            //    if (record.TrainingStatusId == (int)TrainingStatus.Pending && request.TrainingStatusId == (int)TrainingStatus.Completed)
            //    {
            //        return Result.Failure<int>(DashboardErrors.InvalidStatusChangeRequestPendingToCompleted);
            //    }
            //    return Result.Failure<int>(DashboardErrors.InvalidStatusChangeRequest);
            //}

            _repositoryEmployeeTrainingMap.Update(record);
            int count = await _unitOfWork.SaveChangesAsync();
            return count;
        }

        public async Task<int> ChangeStatusByEmail(TrainingUpdateRequest trainingUpdate)
        {
            try
            {

                Domain.Entities.Employee employee = await _employeeService.FetchByEmail(trainingUpdate.EmployeeEmail);
                if (employee is null)
                {
                    throw new KeyNotFoundException($"No employee exists for given email: {trainingUpdate.EmployeeEmail}");

                }
                bool isPermitted = predicateBuilder.CanPerformTrackerTasks(employee);
                if (!isPermitted)
                {
                    throw new KeyNotFoundException(Messages.ERROR_InSufficientPermissions);
                }

                var trainingMaster = await _repositoryTrainingMaster.GetFirstOrDefaultAsync(predicate: x => x.TrainingName.ToLower().Equals(trainingUpdate.TrainingName.ToLower()) && x.IsActive);
                if (trainingMaster is null)
                {
                    throw new KeyNotFoundException($"No training exists for given training name: {trainingUpdate.TrainingName}");
                }

                var skillMaster = await _repositorySkillMaster.GetFirstOrDefaultAsync(predicate: x => x.SkillName.ToLower().Equals(trainingUpdate.SkillName.ToLower()) && x.IsActive);
                if (skillMaster is null)
                {
                    throw new KeyNotFoundException($"No skill exists for given skill name: {trainingUpdate.SkillName}");
                }

                var statusMaster = await _repositoryTrainingStatusMaster.GetFirstOrDefaultAsync(predicate: x => x.TrainingStatusName.ToLower().Equals(trainingUpdate.TrainingStatus.ToLower()) && x.IsActive);
                if (statusMaster is null)
                {
                    throw new KeyNotFoundException($"No status exists for given training status: {trainingUpdate.TrainingStatus}");
                }

                var record = await _repositoryEmployeeTrainingMap.GetFirstOrDefaultAsync(predicate: x => x.EmployeeId.Equals(employee.Id) && x.TrainingId.Equals(trainingMaster.TrainingId) && x.SkillId.Equals(skillMaster.SkillId));

                if (record is null)
                {
                    throw new KeyNotFoundException("No record exists with given training for an employee.");
                }
                record.UpdatedOn = DateTime.UtcNow;
                record.UpdatedBy = _authenticatedUserService.AuthUser.Id;

                var a1 = (record.TrainingStatusId == (int)TrainingStatus.Pending && statusMaster.TrainingStatusId == (int)TrainingStatus.Ongoing);
                var a2 = (record.TrainingStatusId == (int)TrainingStatus.Ongoing && statusMaster.TrainingStatusId == (int)TrainingStatus.Completed);
                var a3 = (record.TrainingStatusId != (int)TrainingStatus.Completed && statusMaster.TrainingStatusId == (int)TrainingStatus.Deferred);

                if (a1 || a2 || a3)
                {
                    record.TrainingStatusId = statusMaster.TrainingStatusId;
                }
                else
                {
                    throw new InvalidOperationException(Messages.ERROR_InvalidStatusChangeRequest);
                }
                _repositoryEmployeeTrainingMap.Update(record);
                int result = await _unitOfWork.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<string> ExecuteTrainingAssignmentJob(SpinTrainingRequest request, List<string> emails = null)
        {
            List<EmployeeResponse> employeeList = new List<EmployeeResponse>();
            emails = new List<string>();

            if (request.Mapping.Count <= 0 || emails.Any())
            {
                // get employees

                if (emails.Any())
                {
                    foreach (var email in emails)
                    {
                        var employeeResult = (await _employeeService.FetchByEcosystemAndEmailStartsWith(email, request.Ecosystem, request.Account));
                        var employee = employeeResult.Value.FirstOrDefault();
                        if (employee != null)
                            employeeList.Add(employee);
                    }

                }
                else
                {
                    var employeeListResult = await _employeeService.FetchByEcosystemAndEmailStartsWith(string.Empty, request.Ecosystem, request.Account);
                    if (employeeListResult.IsSuccess)
                    {
                        employeeList = employeeListResult.Value;
                    }

                }

                // get training meta data
                var skillResult = await _skillAndTrainingService.FetchSkillTrainingsMetaData(request.Ecosystem);

                if (skillResult.IsFailure)
                    return Result.Failure<string>(skillResult.Error).Value; // Fix: Access the Value property of Result<string>

                List<TrainingsGroupedBySkill> skills = skillResult.Value;
                var distinctTrainings = skills
                                        .SelectMany(skill => skill.Trainings.Select(training => new EcosystemTraining
                                        {
                                            SkillId = skill.SkillId,
                                            TrainingName = training.TrainingName,
                                            TrainingLink = training.TrainingLink,
                                            SeniorityId = training.SeniorityId,
                                            Seniority = training.Seniority,
                                            TrainingId = training.TrainingId,
                                            TrainingDescription = training.TrainingDescription,
                                            TrainingCompletionHours = training.TrainingCompletionHours,
                                            IsMvP = training.IsMvP
                                        }))
                                        .GroupBy(t => t.TrainingName)
                                        .Select(g => g.First())
                                        .ToList();

                foreach (var emp in employeeList)
                {
                    var seniority = Convert.ToInt32(emp.Seniority) > 0
                                    ? _academyDbContext.SeniorityMasters.FirstOrDefault(x => x.SeniorityId == Convert.ToInt32(emp.Seniority)).SeniorityName
                                    : string.Empty;

                    var userTrainingMapping = new UserTrainingMapping
                    {
                        UserId = emp.EmployeeId,
                        SeniorityId = Convert.ToInt32(emp.Seniority),
                        Seniority = seniority,
                        UserEmail = emp.EmployeeEmail,
                        UserImage = emp.ImageUrl,
                        Parent = false,
                        Trainings = [],
                    };
                    userTrainingMapping.Trainings = distinctTrainings; // .Where(x => x.SeniorityId == Convert.ToInt32(emp.Seniority)).ToList();
                    userTrainingMapping.SelectedTraning = request.SelectedTraning;
                    request.Mapping.Add(userTrainingMapping);
                }
            }

            string requestAsString = JsonConvert.SerializeObject(request);
            string fileName = DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat);
            using MemoryStream content = new(Encoding.UTF8.GetBytes(requestAsString));
            string fileId = await _googleApiManager.UploadFileOnDrive($"{fileName}.json", ApplicationConstants.MIME_TYPE_JSON, content);
            string id = await _googleApiManager.MoveFileToAnotherFolder(fileId, _appSetting.SpinTrainingRequestDriveId);
            var jobResult = await CreateNewJobRequest(JobRequestType.TrainingAssignment, id);

            if (jobResult.IsFailure) { return Result.Failure<string>(jobResult.Error).Value; } // Fix: Access the Value property of Result<string>

            return jobResult.Value;
        }

        public async Task<Result<string>> ExecuteTrainingAssignmentJob(SpinTrainingRequest request)
        {
            if (request.Mapping.Count <= 0)
            {
                // get employees
                var result = await _employeeService.FetchByEcosystemAndEmailStartsWith(string.Empty, request.Ecosystem, request.Account);

                if (result.IsFailure)
                {
                    return Result.Failure<string>(result.Error);
                }

                // get training meta data
                var skillResult = await _skillAndTrainingService.FetchSkillTrainingsMetaData(request.Ecosystem);

                if (skillResult.IsFailure)
                    return Result.Failure<string>(skillResult.Error);

                List<TrainingsGroupedBySkill> skills = skillResult.Value;
                var distinctTrainings = skills
                                        .SelectMany(skill => skill.Trainings.Select(training => new EcosystemTraining
                                        {
                                            SkillId = skill.SkillId,
                                            TrainingName = training.TrainingName,
                                            TrainingLink = training.TrainingLink,
                                            SeniorityId = training.SeniorityId,
                                            Seniority = training.Seniority,
                                            TrainingId = training.TrainingId,
                                            TrainingDescription = training.TrainingDescription,
                                            TrainingCompletionHours = training.TrainingCompletionHours,
                                            IsMvP = training.IsMvP
                                        }))
                                        .GroupBy(t => t.TrainingName)
                                        .Select(g => g.First())
                                        .ToList();

                var employees = result.Value;

                foreach (var emp in employees)
                {
                    var seniority = Convert.ToInt32(emp.Seniority) > 0
                                    ? _academyDbContext.SeniorityMasters.FirstOrDefault(x => x.SeniorityId == Convert.ToInt32(emp.Seniority)).SeniorityName
                                    : string.Empty;

                    var userTrainingMapping = new UserTrainingMapping
                    {
                        UserId = emp.EmployeeId,
                        SeniorityId = Convert.ToInt32(emp.Seniority),
                        Seniority = seniority,
                        UserEmail = emp.EmployeeEmail,
                        UserImage = emp.ImageUrl,
                        Parent = false,
                        Trainings = [],
                    };
                    userTrainingMapping.Trainings = distinctTrainings; // .Where(x => x.SeniorityId == Convert.ToInt32(emp.Seniority)).ToList();
                    userTrainingMapping.SelectedTraning = request.SelectedTraning;
                    request.Mapping.Add(userTrainingMapping);
                }
            }

            string requestAsString = JsonConvert.SerializeObject(request);
            string fileName = DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat);
            using MemoryStream content = new(Encoding.UTF8.GetBytes(requestAsString));
            string fileId = await _googleApiManager.UploadFileOnDrive($"{fileName}.json", ApplicationConstants.MIME_TYPE_JSON, content);
            string id = await _googleApiManager.MoveFileToAnotherFolder(fileId, _appSetting.SpinTrainingRequestDriveId);
            var jobResult = await CreateNewJobRequest(JobRequestType.TrainingAssignment, id);

            if (jobResult.IsFailure) { return Result.Failure<string>(jobResult.Error); }

            return jobResult.Value;
        }

        public async Task<Result<string>> ExecuteReportJob(ExportReportMetadata request)
        {
            string metadata = JsonConvert.SerializeObject(request);
            var result = await CreateNewJobRequest(JobRequestType.Report, metadata);

            if (result.IsFailure) { return Result.Failure<string>(result.Error); }

            return result.Value;
        }

        public async Task<Result<Tuple<JobRequest, List<JobRequestDetail>>>> RequestTrackerStatus(string transactionId)
        {
            Tuple<JobRequest, List<JobRequestDetail>> response;
            JobRequest request = await _repositoryJobrequest.GetFirstOrDefaultAsync(predicate: x => x.TransactionId == transactionId && x.IsActive);
            if (request is null)
            {
                return Result.Failure<Tuple<JobRequest, List<JobRequestDetail>>>(DashboardErrors.InvalidTransactionId(transactionId));
            }

            List<JobRequestDetail> requestData = (from d in _academyDbContext.JobRequestDetails
                                                  where d.TransactionId == transactionId
                                                   && d.IsActive
                                                  select d).ToList();
            response = Tuple.Create(request, requestData);
            return response;
        }

        private async Task<Result<string>> CreateNewJobRequest(JobRequestType requestType, string requestMeteData)
        {
            string transactionId = DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat);
            JobRequest jobrequest = new()
            {
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CreatedOn = DateTime.UtcNow,
                HasErrors = false,
                ErrorDetail = string.Empty,
                IsActive = true,
                RequestMetadata = requestMeteData,
                RetryCount = 0,
                Status = TrainingStatus.Pending.ToString(),
                TransactionId = transactionId,
                RequestType = requestType.ToString()
            };
            await _repositoryJobrequest.InsertAsync(jobrequest);
            var count = await _unitOfWork.SaveChangesAsync();
            return transactionId;
        }

        public async Task<string> ExecuteReportJob(ExportDetailReportMetadata request)
        {
            string metadata = JsonConvert.SerializeObject(request);
            var trnxId = await CreateNewJobRequest(JobRequestType.Report, metadata);

            return trnxId.Value;
        }
        public async Task<bool> FetchTraining(string trainingName)
        {
            var trainingMaster = await _repositoryTrainingMaster.GetFirstOrDefaultAsync(predicate: x => x.TrainingName.ToLower().Equals(trainingName.ToLower()) && x.IsActive);
            if (trainingMaster is not null)
            {
                return true;
            }

            return false;
        }

        public async Task<string> UploadEmployeeCV(IFormFile file, int employeeId, string community, int docTypeId, string existingCVFileId = null)
        {
            var folderId = _appSetting.CvProfileFolderId;
            var foldersData = await _googleApiManager.ListAllChildFolders(folderId);
            string communityFolderId = foldersData.TryGetValue(community, out string value) ? value : await _googleApiManager.CreateFolder(community, folderId);
            var result = await _employeeService.FetchById(employeeId);
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{result.Value.GlobantEmailAddress.Split('@')[0]}{extension}";
            var docType = _repositoryEmployeeDocumentTypeMaster.GetAll().Where(x => x.EmployeeDocumentTypeId == docTypeId).Select(x => x.DocumentType).FirstOrDefault();

            var cvProfileFoldersData = await _googleApiManager.ListAllChildFolders(communityFolderId);
            string cvProfileFoldersId = cvProfileFoldersData.TryGetValue(docType.Equals("cv", StringComparison.InvariantCultureIgnoreCase) ? "CV" : "Profile", out string cvProfileValues) ? 
                cvProfileValues : await _googleApiManager.CreateFolder(docType.Equals("cv", StringComparison.InvariantCultureIgnoreCase) ? "CV" : "Profile", communityFolderId);

            if (!string.IsNullOrEmpty(cvProfileFoldersId))
            {
                var uploadresult = await _googleApiManager.UploadFile(file, fileName, cvProfileFoldersId);

                if (!string.IsNullOrEmpty(uploadresult.webContentLink))
                {
                    Dictionary<string, object> iParams = new()
                    {
                         { "@EmployeeId",employeeId },
                         { "@DocumentLink", uploadresult.webContentLink },
                         { "@DocumentTypeId", docTypeId },
                         { "@CurrentUserId", _authenticatedUserService.AuthUser.Id },
                    };

                    _ = await _academyDbAdoClient.ExecuteNonQueryAsync("dbo.usp_InsertOrUpdateEmployeeDocument", iParams);
                }
                else
                {
                    return "FAILED";
                }

                if (!string.IsNullOrEmpty(existingCVFileId))
                {
                    await _googleApiManager.DeleteFile(existingCVFileId);
                }
            }

            return "UPLOAD";
        }

        public async Task<Result<List<EmployeeDocumentType>>> FetchAllDocumentType()
        {
            var result = _repositoryEmployeeDocumentTypeMaster.GetAll()
                .Select(d => new EmployeeDocumentType
                {
                    DocumentTypeId = d.EmployeeDocumentTypeId,
                    DocumentType = d.DocumentType
                })
                .ToList();
            return Result.Success(result);
        }
    }
}