using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using System.Linq.Expressions;

namespace Academy.Core.PredicateBuilder
{
    public class RecruiterPredicate : AbstractAdminPredicate
    {
        private readonly Expression<Func<Employee, bool>> rolePredicate;

        public RecruiterPredicate(IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
            
        }

        public override bool CanPerformTrackerTasks(Employee employee)
        {
            return false;
        }

        public override Expression<Func<Dashboard, bool>> FetchDashboard()
        {
            throw new NotImplementedException();
        }

        public override Expression<Func<Employee, bool>> FetchEmployeeById(int employeeId)
        {
            throw new NotImplementedException();
        }

        public override string FetchEmployeeFilteredStartesWith(string startsWith)
        {
            throw new NotImplementedException();
        }

        public override Expression<Func<Employee, bool>> FetchEmployees()
        {
            throw new NotImplementedException();
        }

        public override string FetchGexLeaderFilteredStartesWith(string startsWith)
        {
            throw new NotImplementedException();
        }
    }
}
