using Academy.Domain.Entities;
using Academy.Shared.Enums;
using System.Linq.Expressions;
namespace Academy.Workers.ReportWorker
{
    internal class Utilities
    {
        public static Expression<Func<Dashboard, bool>> BuildRoleGuard(List<EmployeeRoleMap> userRoles, Employee requestor)
        {
            Expression<Func<Dashboard, bool>> predicate;
            if (userRoles.Exists(x => x.IsActive && x.RoleId == (int)Roles.SystemAdmin))
            {
                predicate = x => x.IsActive;
            }
            else if (userRoles.Exists(x => x.IsActive && x.RoleId == (int)Roles.AccountAdmin))
            {
                List<string> accounts = userRoles.Select(x => x.RoleAssignment).ToList();
                predicate = x => x.IsActive && accounts.Contains(x.Client) && x.EmployeeEmail == requestor.GlobantEmailAddress;
            }
            else if (userRoles.Exists(x => x.IsActive && x.RoleId == (int)Roles.TdcAdmin))
            {
                List<string> tdcs = userRoles.Select(x => x.RoleAssignment).ToList();
                predicate = x => x.IsActive && tdcs.Contains(x.Tdc) && x.EmployeeEmail == requestor.GlobantEmailAddress;
            }
            else if (userRoles.Exists(x => x.IsActive && x.RoleId == (int)Roles.CommunityAdmin))
            {
                List<string> communities = userRoles.Select(x => x.RoleAssignment).ToList();
                predicate = x => x.IsActive && communities.Contains(x.Community) && x.EmployeeEmail == requestor.GlobantEmailAddress;
            }
            else if (userRoles.Exists(x => x.IsActive && x.RoleId == (int)Roles.EcosystemAdmin))
            {
                List<string> ecosystems = userRoles.Select(x => x.RoleAssignment).ToList();
                predicate = x => x.IsActive && ecosystems.Contains(x.WorkingEcosystem) && x.EmployeeEmail == requestor.GlobantEmailAddress;
            }
            else
            {
                List<string> gexLeaders = requestor.GexLeaders.Split(',').ToList();
                predicate = x => (x.EmployeeEmail == requestor.GlobantEmailAddress || gexLeaders.Contains(x.EmployeeEmail) || x.CareerMentorEmail == requestor.GlobantEmailAddress)
                && x.IsActive;
            }
            return predicate;
        }
    }
}
