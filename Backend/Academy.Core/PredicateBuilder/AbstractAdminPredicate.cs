using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.Extensions;
using System.Linq.Expressions;

namespace Academy.Core.PredicateBuilder
{
    public abstract class AbstractAdminPredicate
    {
        protected readonly IAuthenticatedUserService _authenticatedUserService;
        protected readonly List<string> authUserRoles;
        protected readonly Expression<Func<Employee, bool>> isActive = e => e.IsActive;
        protected readonly Expression<Func<Employee, bool>> bettermeLeaderCheck;
        protected readonly Expression<Func<Employee, bool>> isAuthenticatedUser;
        public AbstractAdminPredicate(IAuthenticatedUserService authenticatedUserService)
        {
            _authenticatedUserService = authenticatedUserService;
            authUserRoles = authenticatedUserService.AuthUser.Roles.Select(r => r.RoleAssignment.ToLower()).ToList();
            bettermeLeaderCheck = e => e.BetterMeLeaderEmail.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower());
            isAuthenticatedUser = e => e.GlobantEmailAddress.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower());
        }

        /// <summary>
        /// Fetches an expression that filters active employees based on the authenticated user's roles and emails.
        /// </summary>
        /// <returns>A lambda expression representing the filter criteria for fetching active employees.</returns>
        protected Expression<Func<Employee, bool>> FetchEmployeesBase(Expression<Func<Employee, bool>> rolePredicate)
        {
            // Check if the employee's Globant email matches the authenticated user's Globant email,
            // or if the employee's BetterMe leader email matches the authenticated user's Globant email,
            // or if the authenticated user is one of the GEX leaders associated with this employee,
            Expression<Func<Employee, bool>> isAuthenticatedUserOrBetterMeLeader = isAuthenticatedUser.Or(bettermeLeaderCheck);
            Expression<Func<Employee, bool>> leadershipOrRolePredicate = isAuthenticatedUserOrBetterMeLeader.Or(rolePredicate);

            return isActive.And(leadershipOrRolePredicate);
        }

        /// <summary>
        /// Fetches an expression that filters active employees by their ID.
        /// </summary>
        /// <param name="employeeId">The ID of the employee to fetch.</param>
        /// <returns>A lambda expression representing the filter criteria for fetching the employee.</returns>
        public Expression<Func<Employee, bool>> FetchEmployeeByIdBase(int employeeId, Expression<Func<Employee, bool>> rolePredicate)
        {
            // Check if the employee is active
            // Check if the employee's ID matches the provided employee ID
            Expression<Func<Employee, bool>> isActiveEmployeeWithId = e => e.IsActive && e.Id.Equals(employeeId);
            Expression<Func<Employee, bool>> leadershipOrRolePredicate = bettermeLeaderCheck.Or(rolePredicate);
            return isActiveEmployeeWithId.And(leadershipOrRolePredicate);
        }

        public string FetchEmployeeFilteredStartesWithBase(string startsWith)
        {
            return $"IsActive = 1 AND SeniorityId IN ({string.Join(",", ApplicationConstants.ALLOWED_SENIORITIES)})" +
                   // Filter employees whose Globant email address starts with the specified string
                   $" AND (LOWER(GlobantEmailAddress) LIKE '%{startsWith}%' " +
                   // Include employees where the BetterMe leader email matches the authenticated user's Globant email
                   $" OR LOWER(BetterMeLeaderEmail) = '{_authenticatedUserService.AuthUser.GloberEmail.ToLower()}' " +
                   // Include employees where the GEX leaders contains the authenticated user's Globant email
                   $" OR LOWER(GexLeaders) LIKE '%{_authenticatedUserService.AuthUser.GloberEmail.ToLower()}%' " +
                   // Include employees whose Community matches any of the authenticated user's roles
                   " {0})";
        }
        public string FetchGexFilteredStartesWithBase(string startsWith)
        {
            return $"IsActive = 1 AND SeniorityLevel IN ({string.Join(",", ApplicationConstants.ALLOWED_SENIORITIES)})" +
                   // Filter employees whose Globant email address starts with the specified string
                   $"(LOWER(EmployeeName) LIKE '%{startsWith}%' OR LOWER(GlobantEmailAddress) LIKE '%{startsWith}%')";
        }


        public Expression<Func<Dashboard, bool>> FetchDashboardBase(Expression<Func<Dashboard, bool>> rolePredicate)
        {
            Expression<Func<Dashboard, bool>> isActiveDashboard = d => d.IsActive;
            Expression<Func<Dashboard, bool>> betterMeLeaderPredicate = d => d.CareerMentorEmail.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower());
            return isActiveDashboard.And(betterMeLeaderPredicate.Or(rolePredicate));
        }



        public virtual Expression<Func<EcosystemMaster, bool>> FetchAndInsertEcosystems(IAcademyDbContext _dbContext)
        {
            return null;
        }

        public virtual Expression<Func<SeniorityMaster, bool>> FetchAndInsertSeniorities()
        {
            return null;
        }

        public Expression<Func<SkillMaster, bool>> FetchAndInsertSkills()
        {
            return null;
        }

        public Expression<Func<TrainingMaster, bool>> FetchAndInsertTrainings()
        {
            return null;
        }
        public virtual bool CanFetchActivitiesByEmployeeId()
        {
            return true;
        }
        public virtual bool CanInsertOrUpdateEmployeeActivities()
        {
            return false;
        }

        public virtual bool CanExtendEndDate(Employee employee)
        {
            return false;
        }

        public virtual bool CanInsertOrUpdateProficiency(Employee employee)
        {
            return false;
        }

        public virtual bool CanUpdateDojoGxLeadxer()
        {
            return false;
        }
        public virtual bool CanUpdateTrainingProficiency()
        {
            return false;
        }
        public virtual bool CanInsertTrainingProficiencyMapping()
        {
            return false;
        }
        public virtual bool CanInsertOrUpdateTraining()
        {
            return false;
        }
        public virtual bool CanInsertOrUpdateSkill()
        {
            return false;
        }
        public virtual bool CanInsertOrUpdateSeniority()
        {
            return false;
        }
        public virtual bool CanAddOrInsertRoleMaster()
        {
            return false;
        }

        public virtual bool CanInsertBulkActivities()
        {
            return false;
        }

        public virtual bool CanCreateCategoryOrSubCategory()
        {
             return false;

        }

        public virtual bool CanGetFilteredPagedDojoDetails() { return false; }

        public virtual bool CanCreateOrUpdateTrainings() { return false; }
        public abstract bool CanPerformTrackerTasks(Employee employee);
        public abstract Expression<Func<Employee, bool>> FetchEmployees();
        public abstract Expression<Func<Employee, bool>> FetchEmployeeById(int employeeId);
        public abstract string FetchEmployeeFilteredStartesWith(string startsWith);
        public abstract string FetchGexLeaderFilteredStartesWith(string startsWith);
        public abstract Expression<Func<Dashboard, bool>> FetchDashboard();
        public virtual bool CanFetchDojoActivityReport() { return false; }

        public Expression<Func<Employee, bool>> FetchEmployeeByEmail(string email)
        {
            return e => e.IsActive && e.GlobantEmailAddress == email;
        }

    }
}
