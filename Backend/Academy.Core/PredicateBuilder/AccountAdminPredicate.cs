using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Extensions;
using System.Linq.Expressions;

namespace Academy.Core.PredicateBuilder
{
    public class AccountAdminPredicate : AbstractAdminPredicate
    {
        private readonly Expression<Func<Employee, bool>> rolePredicate;
        public AccountAdminPredicate(IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
            rolePredicate = e => authUserRoles.Contains(e.Client.ToLower());
        }

        /// <summary>
        /// Fetches an expression that filters active employees based on the authenticated user's roles and emails.
        /// </summary>
        /// <returns>A lambda expression representing the filter criteria for fetching active employees.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployees()
        {
            return FetchEmployeesBase(rolePredicate);
        }

        /// <summary>
        /// Fetches an expression that filters active employees by their ID.
        /// </summary>
        /// <param name="employeeId">The ID of the employee to fetch.</param>
        /// <returns>A lambda expression representing the filter criteria for fetching the employee.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployeeById(int employeeId)
        {
            return FetchEmployeeByIdBase(employeeId, rolePredicate);
        }

        /// <summary>
        /// Constructs a SQL filter string for fetching employees whose details match certain criteria.
        /// </summary>
        /// <param name="startsWith">The string that the employee's Globant email address should start with.</param>
        /// <returns>A SQL WHERE clause string for filtering employees.</returns>
        public override string FetchEmployeeFilteredStartesWith(string startsWith)
        {
            return string.Format(FetchEmployeeFilteredStartesWithBase(startsWith), $"OR Client IN ('{string.Join("','", authUserRoles)}')");
        }
        public override string FetchGexLeaderFilteredStartesWith(string startsWith)
        {
            return string.Format(FetchGexFilteredStartesWithBase(startsWith));
        }
        public override Expression<Func<Dashboard, bool>> FetchDashboard()
        {
            Expression<Func<Dashboard, bool>> dashboardPredicate = e => authUserRoles.Contains(e.Client.ToLower());
            return FetchDashboardBase(dashboardPredicate);
        }

        public override bool CanPerformTrackerTasks(Employee employee)
        {
            return true;
        }
        public override bool CanExtendEndDate(Employee employee)
        {
            return authUserRoles.Contains(employee.Client.ToLower());
        }
        public override bool CanInsertOrUpdateEmployeeActivities()
        {
            return true;
        }
        public override bool CanUpdateDojoGxLeadxer()
        {
            return true;
        }
        public override bool CanUpdateTrainingProficiency()
        {
            return true;
        }
        public override bool CanInsertTrainingProficiencyMapping()
        {
            return true;
        }
        public override bool CanInsertOrUpdateTraining()
        {
            return true;
        }
        public override bool CanInsertOrUpdateSkill()
        {
            return true;
        }
        public override bool CanInsertBulkActivities()
        {
            return true;
        }

        public override bool CanGetFilteredPagedDojoDetails()
        {
            return true;
        }

        public override bool CanFetchDojoActivityReport()
        {
            return true;
        }
    }
}
