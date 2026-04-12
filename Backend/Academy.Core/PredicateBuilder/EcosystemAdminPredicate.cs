using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Extensions;
using System.Linq.Expressions;

namespace Academy.Core.PredicateBuilder
{
    public class EcosystemAdminPredicate : AbstractAdminPredicate
    {
        private readonly Expression<Func<Employee, bool>> rolePredicate;
        public EcosystemAdminPredicate(IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
            rolePredicate = e => authUserRoles.Contains(e.WorkingEcosystem.ToLower());
        }

        /// <summary>
        /// Fetches an expression that filters active employees based on the authenticated user's roles and emails.
        /// </summary>
        /// <returns>A lambda expression representing the filter criteria for fetching active employees.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployees()
        {
            return FetchEmployeesBase(rolePredicate).Or(e => authUserRoles.Contains(e.WorkingEcosystem.ToLower()));
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
            return string.Format(FetchEmployeeFilteredStartesWithBase(startsWith), $"OR WorkingEcosystem IN ('{string.Join("','", authUserRoles)}')");
        }
        public override string FetchGexLeaderFilteredStartesWith(string startsWith)
        {
            return string.Format(FetchGexFilteredStartesWithBase(startsWith));
        }
        public override Expression<Func<Dashboard, bool>> FetchDashboard()
        {
            Expression<Func<Dashboard, bool>> dashboardPredicate = e => authUserRoles.Contains(e.WorkingEcosystem.ToLower());
            return FetchDashboardBase(dashboardPredicate);
        }

        public override bool CanExtendEndDate(Employee employee)
        {
            return authUserRoles.Contains(employee.WorkingEcosystem.ToLower());
        }

        public override bool CanPerformTrackerTasks(Employee employee)
        {
            return authUserRoles.Contains(employee.WorkingEcosystem.ToLower());
        }

        public override Expression<Func<EcosystemMaster, bool>> FetchAndInsertEcosystems(IAcademyDbContext _dbContext)
        {
            // Define a predicate expression to filter EcosystemMaster entities that are active, belong to the user's roles,
            // and are marked as primary.
            Expression<Func<EcosystemMaster, bool>> pPredicate = e => e.IsActive && authUserRoles.Contains(e.EcosystemName.ToLower()) && e.IsPrimary;

            // Query the database context to retrieve a list of EcosystemId for EcosystemMasters that are both primary and active, 
            // and also belong to the user's roles (case-insensitive).
            List<int> ids = [.. (from p in _dbContext.EcosystemMasters
                             where p.IsPrimary && p.IsActive
                             && authUserRoles.Contains(p.EcosystemName.ToLower())
                             select p.EcosystemId)];

            // Define a second predicate expression to filter EcosystemMaster entities that are active,
            // are not marked as primary, and whose IDs are in the previously retrieved list.
            Expression<Func<EcosystemMaster, bool>> sPredicate = e => e.IsActive && !e.IsPrimary && ids.Contains(e.ParentEcosystemId.Value);

            // Combine the two predicates using an 'And' operation to create a comprehensive filter for EcosystemMasters.
            return pPredicate.And(sPredicate);
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
    }
}
