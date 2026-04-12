using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using System.Linq.Expressions;

namespace Academy.Core.PredicateBuilder
{
    public class SystemAdminPredicate : AbstractAdminPredicate
    {
        public SystemAdminPredicate(IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
        }

        /// <summary>
        /// Fetches an expression that filters active employees based on the authenticated user's roles and emails.
        /// </summary>
        /// <returns>A lambda expression representing the filter criteria for fetching active employees.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployees()
        {
            return e => e.IsActive;
        }

        /// <summary>
        /// Fetches an expression that filters active employees by their ID.
        /// </summary>
        /// <param name="employeeId">The ID of the employee to fetch.</param>
        /// <returns>A lambda expression representing the filter criteria for fetching the employee.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployeeById(int employeeId)
        {
            return e => e.IsActive && e.Id == employeeId;
        }

        /// <summary>
        /// Constructs a SQL filter string for fetching employees whose details match certain criteria.
        /// </summary>
        /// <param name="startsWith">The string that the employee's Globant email address should start with.</param>
        /// <returns>A SQL WHERE clause string for filtering employees.</returns>
        public override string FetchEmployeeFilteredStartesWith(string startsWith)
        {
            return $"LOWER(GlobantEmailAddress) LIKE '%{startsWith}%' AND IsActive = 1";
        }
        public override string FetchGexLeaderFilteredStartesWith(string startsWith)
        {
            return $"(LOWER(EmployeeName) LIKE '%{startsWith}%' OR LOWER(GlobantEmailAddress) LIKE '%{startsWith}%') AND IsActive = 1";
        }
        public override Expression<Func<EcosystemMaster, bool>> FetchAndInsertEcosystems(IAcademyDbContext _dbContext)
        {
            return e => e.IsActive == true;
        }
        public override Expression<Func<Dashboard, bool>> FetchDashboard()
        {
            return e => e.IsActive;
        }

        public override bool CanExtendEndDate(Employee employee)
        {
            return true;
        }
        public override bool CanPerformTrackerTasks(Employee employee)
        {
            return true;
        }
        public override bool CanInsertOrUpdateProficiency(Employee employee)
        {
            return true;
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
        public override bool CanInsertOrUpdateSeniority()
        {
            return true;
        }

        public override bool CanAddOrInsertRoleMaster()
        {
            return true;
        }
        public override bool CanInsertBulkActivities()
        {
            return true;
        }
        public override bool CanCreateCategoryOrSubCategory() { return true; }
        public override bool CanCreateOrUpdateTrainings() { return true; }
        public override bool CanGetFilteredPagedDojoDetails()
        {
            return true;
        }
    }
}
