using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Exceptions;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace Academy.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPredicateFactory _predicateFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Employee> _repositoryEmployee;
        private readonly IAdoClient<AcademyDbSetting> _academyDB;
        private readonly IAcademyDbContext _academyDbContext;
        public EmployeeService(IAuthenticatedUserService authenticatedUserService, IUnitOfWork unitOfWork,
            IAdoClient<AcademyDbSetting> academyDB, IPredicateFactory predicateFactory, IAcademyDbContext academyDbContext)
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _repositoryEmployee = _unitOfWork.GetRepository<Employee>();
            _academyDB = academyDB;
            _predicateFactory = predicateFactory;
            _academyDbContext = academyDbContext;
        }

        public async Task<Result<IList<Employee>>> FetchByOptions(DataRequestOptions dataRequestOptions)
        {
            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);

            Expression<Func<Employee, bool>> predicate = predicateBuilder.FetchEmployees();

            int countEmployee = _repositoryEmployee.Count(predicate);

            if (countEmployee > 0)
            {
                var queryEmployee = await _repositoryEmployee.GetPagedListAsync(
                                predicate: x => x.IsActive,
                                pageIndex: dataRequestOptions.PagingOptions.PageIndex,
                                pageSize: dataRequestOptions.PagingOptions.PageSize
                );

                if (queryEmployee != null)
                {
                    var employee = queryEmployee.Items;
                    return (Result<IList<Employee>>)employee;
                }
            }
            return new List<Employee>();
        }

        public async Task<Result<Employee>> FetchById(int globerId)
        {
            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            Expression<Func<Employee, bool>> predicate = predicateBuilder.FetchEmployeeById(globerId);
            Employee employee = await _repositoryEmployee.GetFirstOrDefaultAsync(predicate: predicate);

            if (employee == null)
            {
                return Result.Failure<Employee>(DomainErrors.Common.NotFound(globerId.ToString()));
            }

            return employee;
        }

        public async Task<Result<IList<Employee>>> FetchAll()
        {
            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            Expression<Func<Employee, bool>> predicate = predicateBuilder.FetchEmployees();
            int countEmployee = _repositoryEmployee.Count(predicate);
            if (countEmployee > 0)
            {
                var queryEmployee = await _repositoryEmployee.GetPagedListAsync(
                                predicate: predicate,
                                pageIndex: 0,
                                pageSize: countEmployee
                );
                if (queryEmployee != null)
                {
                    var employee = queryEmployee.Items;
                    return (Result<IList<Employee>>)employee;
                }
            }
            return new List<Employee>();
        }
        public async Task<Result<List<EmployeeResponse>>> FetchByEcosystemAndEmailStartsWith(string startsWith, int ecosystemId, string account)
        {
            List<EmployeeResponse> employees = [];

            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            string Where = predicateBuilder.FetchEmployeeFilteredStartesWith(startsWith);

            Dictionary<string, object> iParam = new()
            {
                { DbConstants.PARAM_WHERE, Where },
                { DbConstants.PARAM_ECOSYSTEM_ID, ecosystemId }
            };

            if (!string.IsNullOrWhiteSpace(account))
                iParam.Add(DbConstants.PARAM_CLIENT, account);

            DataTable table = await _academyDB.ExecuteReaderAsync(DbConstants.FETCH_EMPLOYEES_STARTS_WITH, iParam);
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    employees.Add(new EmployeeResponse()
                    {
                        EmployeeEmail = row["GlobantEmailAddress"].ToString(),
                        EmployeeId = Convert.ToInt32(row["Id"].ToString()),
                        ImageUrl = row["Image"].ToString(),
                        Seniority = row["SeniorityId"].ToString()
                    });
                }
            }
            return employees;
        }
        public async Task<Result<List<EmployeeResponse>>> FetchByGexLeaderNameStartsWith(string startsWith)
        {
            List<EmployeeResponse> employees = [];

            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            string Where = predicateBuilder.FetchGexLeaderFilteredStartesWith(startsWith);

            Dictionary<string, object> iParam = new()
            {
                { DbConstants.PARAM_WHERE, Where }
            };

            DataTable table = await _academyDB.ExecuteReaderAsync(DbConstants.FETCH_GEXLEADER_STARTS_WITH, iParam);

            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    employees.Add(new EmployeeResponse()
                    {
                        EmployeeEmail = row["GlobantEmailAddress"].ToString(),
                        EmployeeId = Convert.ToInt32(row["Id"].ToString()),
                        ImageUrl = row["Image"].ToString(),
                        Seniority = row["SeniorityId"].ToString(),
                        EmployeeName = row["EmployeeName"].ToString()
                    });
                }
            }
            return employees;
        }

        public async Task<Result<List<EmployeeRoleDto>>> Search(string keyword)
        {
            List<EmployeeRoleDto> employees_roles = [];

            byte zero = 0;
            var userroles = _authenticatedUserService.AuthUser.Roles.Select(x => x.RoleName).ToList();
            if (userroles.Contains(Roles.SystemAdmin.ToString()))
            {
                var employees = _academyDbContext.Employees.Where(x => x.IsActive && x.GlobantEmailAddress.ToLower().Contains(keyword.ToLower())).ToList();
                var employeesIds = employees.Select(x => x.Id).ToList();
                var roles = _academyDbContext.EmployeeRoleMaps.Where(x => x.IsActive && employeesIds.Contains(x.EmployeeId)).ToList();

                employees_roles = [.. (from e in employees
                                   join r in roles on e.Id equals r.EmployeeId into erGroup
                                   from r in erGroup.DefaultIfEmpty()
                                   join rm in _academyDbContext.RoleMasters on r?.RoleId equals rm?.RoleId into rmGroup
                                   from rm in rmGroup.DefaultIfEmpty()
                                   group new { r, rm } by e into grouped
                                   select new EmployeeRoleDto
                                   {
                                       EmployeeId = grouped.Key.Id,
                                       EmployeeName = grouped.Key.EmployeeName,
                                       GlobantEmailAddress = grouped.Key.GlobantEmailAddress,
                                       Seniority = grouped.Key.Seniority,
                                       Roles = grouped?.Select(gr => new RoleDto
                                       {
                                           RoleId = gr.rm == null ? zero : gr.rm.RoleId,
                                           RoleName = gr.rm?.RoleName ?? "User",
                                           RoleAssignment = gr.r?.RoleAssignment ?? string.Empty
                                       }).ToList()
                                   })];
            }
            return employees_roles;
        }
        public async Task<Result<List<string>>> FetchAllTdc()
        {
            var result = _academyDbContext.Employees.Where(x => !string.IsNullOrWhiteSpace(x.Tdc)).Select(x => x.Tdc).Distinct().ToList();
            return result;
        }

        public async Task<Result<DojoCommunityCountryListResponse>> FetchAllTdcCommunityDojo()
        {
            DojoCommunityCountryListResponse dojoCommunityCountryListResponses = new DojoCommunityCountryListResponse
            {
                Countries = new List<string>(),
                Communities = new List<string>(),
                Accounts = new List<AiStudioAccount>(),
                AiStudios = new List<string>()
            };
            var dataset = await _academyDB.XecuteReaderDataSetAsync(DbConstants.FETCH_DOJO_REPORT_FILTERS, null);

            if (dataset.Tables.Count > 0)
            {
                DataTable reader = dataset.Tables[0];
                foreach (DataRow row in reader.Rows)
                {
                    var country = row.Field<string>("Country");
                    if (!string.IsNullOrWhiteSpace(country))
                    {
                        dojoCommunityCountryListResponses.Countries.Add(country);
                    }
                }

                reader = dataset.Tables[1];
                foreach (DataRow row in reader.Rows)
                {
                    var community = row.Field<string>("Community");
                    if (!string.IsNullOrWhiteSpace(community))
                    {
                        dojoCommunityCountryListResponses.Communities.Add(community);
                    }
                }

                reader = dataset.Tables[2];
                foreach (DataRow row in reader.Rows)
                {
                    var aiStudio = row.Field<string>("AiStudio");
                    if (!string.IsNullOrWhiteSpace(aiStudio))
                    {
                        dojoCommunityCountryListResponses.AiStudios.Add(aiStudio.Trim());
                    }
                }

                reader = dataset.Tables[3];
                foreach (DataRow row in reader.Rows)
                {
                    var account = row.Field<string>("Account");
                    var aiStudio = row.Field<string>("AiStudio");
                    if (!string.IsNullOrWhiteSpace(account) &&
                        !string.IsNullOrWhiteSpace(aiStudio))
                    {
                        dojoCommunityCountryListResponses.Accounts.Add(
                            new AiStudioAccount
                            {
                                Account = account,
                                AiStudio = aiStudio
                            });
                    }
                }
            }
            return Result.Success(dojoCommunityCountryListResponses);
        }

        public async Task<Result<List<string>>> FetchAllCommunity()
        {
            return await _academyDbContext.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Community))
                .Select(x => x.Community).Distinct().ToListAsync();
        }

        public async Task<Result<List<string>>> FetchAllClients()
        {
            return await _academyDbContext.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Client))
                .Select(x => x.Client).Distinct().ToListAsync();
        }

        public async Task<Result<List<string>>> FetchAllAccount()
        {
            return await _academyDbContext.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Client))
                .Select(x => x.Client).Distinct()
                .OrderBy(q => q).ToListAsync();
        }

        public async Task<Result<List<ActivityMasterDto>>> FetchAllActivities()
        {
            return await _academyDbContext.ActivityMasters
                .Where(x => x.IsActive)
                .Select(x => new ActivityMasterDto
                {
                    ActivityId = x.ActivityId,
                    ActivityName = x.ActivityName,
                    ActivityDescription = x.ActivityDescription
                })
                .Distinct()
                .ToListAsync();
        }
        public async Task<List<string>> FetchAllProject(CancellationToken cancellationToken = default)
        {
            return await _academyDbContext.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Project))
                .Select(x => x.Project)
                .Distinct().OrderBy(q => q).ToListAsync<string>();
        }
        public async Task<List<string>> FetchAllProjectBasedonClient(string[] Client, CancellationToken cancellationToken = default)
        {
            return await _academyDbContext.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Project) && Client.Contains(x.Client))
                .Select(x => x.Project)
                .Distinct().OrderBy(q => q).ToListAsync<string>();
        }

        public async Task<Result<List<LearningPathDto>>> FetchAllAreaPaths()
        {
            var result = _academyDbContext.LearningPaths
                .Where(x => x.IsActive)
                .Select(x => new LearningPathDto
                {
                    LearningPathId = x.LearningPathId,
                    LearningPathName = x.LearningPathName,
                    LearningPathDescription = x.LearningPathDescription
                })
                .Distinct()
                .ToList();
            return result;
        }

        public async Task<Employee> FetchByEmail(string email)
        {
            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            Expression<Func<Employee, bool>> predicate = predicateBuilder.FetchEmployeeByEmail(email);
            Employee employee = await _repositoryEmployee.GetFirstOrDefaultAsync(predicate: predicate);
            return employee;
        }

        public IEnumerable<Employee> FetchByName(string name)
        {
            var result = _academyDbContext.Employees.Where(x => x.IsActive && x.EmployeeName.Contains(name)).ToList();
            return result;
        }

        public async Task<Result<List<string>>> FetchAllAiStudio()
        {
            return await _academyDbContext.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.AiStudio))
                .Select(x => x.AiStudio).Distinct().ToListAsync();
        }

        public async Task<Result<List<AiStudioAccount>>> FetchAllAiStudioAccount()
        {
            var data = await _academyDbContext.Employees
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.AiStudio) &&
                    !string.IsNullOrWhiteSpace(x.Client))
                .Select(x => new AiStudioAccount
                {
                    AiStudio = x.AiStudio,
                    Account = x.Client
                })
                .Distinct()
                .ToListAsync();

            return Result.Success(data);
        }

        public async Task<List<Employee>> FetchByEmails(List<string> emails)
        {
            var data = await _academyDbContext.Employees.Where(c => emails.Contains(c.GlobantEmailAddress)).ToListAsync();
            return data;
        }
    }
}
