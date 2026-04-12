using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Extensions;
using System.Linq.Expressions;

namespace Academy.Core.PredicateBuilder
{
    public class UserPredicate : AbstractAdminPredicate
    {
        public UserPredicate(IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
        }

        /// <summary>
        /// Fetches an expression that filters active employees based on the authenticated user's roles and emails.
        /// </summary>
        /// <returns>A lambda expression representing the filter criteria for fetching active employees.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployees()
        {
            return e =>
               // Check if the employee is active
               e.IsActive &&

               // Check if the employee's Globant email matches the authenticated user's Globant email,
               // or if the employee's BetterMe leader email matches the authenticated user's Globant email,
               // or if the authenticated user is one of the GEX leaders associated with this employee,
               // or if the authenticated user has a role that matches the employee's Community
               (e.GlobantEmailAddress.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower()) ||
                e.BetterMeLeaderEmail.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower())
                //|| //_authenticatedUserService.AuthUser.GexLeaders.Intersect(e.GexLeaders.ToList<string>()).Any()
                );
        }

        /// <summary>
        /// Fetches an expression that filters active employees by their ID.
        /// </summary>
        /// <param name="employeeId">The ID of the employee to fetch.</param>
        /// <returns>A lambda expression representing the filter criteria for fetching the employee.</returns>
        public override Expression<Func<Employee, bool>> FetchEmployeeById(int employeeId)
        {
            return e =>
                // Check if the employee is active
                e.IsActive &&

                // Check if the employee's ID matches the provided employee ID
                e.Id.Equals(employeeId) &&
               (e.GlobantEmailAddress.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower()) ||
                e.BetterMeLeaderEmail.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower())
                //|| //_authenticatedUserService.AuthUser.GexLeaders.Intersect(e.GexLeaders.ToList<string>()).Any()
                );

        }

        /// <summary>
        /// Constructs a SQL filter string for fetching employees whose details match certain criteria.
        /// </summary>
        /// <param name="startsWith">The string that the employee's Globant email address should start with.</param>
        /// <returns>A SQL WHERE clause string for filtering employees.</returns>
        public override string FetchEmployeeFilteredStartesWith(string startsWith)
        {
            return string.Format(FetchEmployeeFilteredStartesWithBase(startsWith), string.Empty);
        }
        public override string FetchGexLeaderFilteredStartesWith(string startsWith)
        {
            return string.Format(FetchGexFilteredStartesWithBase(startsWith));
        }
        public override Expression<Func<Dashboard, bool>> FetchDashboard()
        {
            return e => e.IsActive
                         && (e.EmployeeEmail.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower())
                            || e.CareerMentorEmail.ToLower().Equals(_authenticatedUserService.AuthUser.GloberEmail.ToLower())
                         //|| _authenticatedUserService.AuthUser.GexLeaders.Intersect(e.GexLeaders.ToList<string>()).Any()
                         );
        }

        public override bool CanExtendEndDate(Employee employee)
        {
            return _authenticatedUserService.AuthUser.CareerMentorEmail.Equals(employee.BetterMeLeaderEmail, StringComparison.OrdinalIgnoreCase)
                   || _authenticatedUserService.AuthUser.GexLeaders.Intersect(employee.GexLeaders.ToList<string>()).Any();
        }

        public override bool CanPerformTrackerTasks(Employee employee)
        {
            if (_authenticatedUserService.AuthUser.CareerMentorEmail.Equals(employee.BetterMeLeaderEmail, StringComparison.OrdinalIgnoreCase)
                    || _authenticatedUserService.AuthUser.GexLeaders.Intersect(employee.GexLeaders.ToList<string>()).Any())
                return true;
            else if (_authenticatedUserService.AuthUser.Id == employee.Id)
                return true;
            return false;
        }

        public override bool CanInsertOrUpdateProficiency(Employee employee)
        {
            if (_authenticatedUserService.AuthUser.Id == employee.Id) return false;
            return true;
        }
    }
}
