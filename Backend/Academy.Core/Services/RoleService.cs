using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Exceptions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Academy.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EmployeeRoleMap> _repositoryEmployeeRoleMap;
        private readonly IPredicateFactory _predicateFactory;
        private readonly AbstractAdminPredicate predicateBuilder;

        public RoleService(IAcademyDbContext academyDbContext, IAuthenticatedUserService authenticatedUserService, IEmployeeService employeeService, IUnitOfWork unitOfWork, IPredicateFactory predicateFactory)
        {
            _academyDbContext = academyDbContext;
            _authenticatedUserService = authenticatedUserService;
            _unitOfWork = unitOfWork;
            _repositoryEmployeeRoleMap = _unitOfWork.GetRepository<EmployeeRoleMap>();
            _predicateFactory = predicateFactory;
            predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
        }

        public async Task<Result<List<RoleMaster>>> GetRoleMaster()
        {
            return await _academyDbContext.RoleMasters.AsNoTracking().ToListAsync();
        }

        public async Task<Result<bool>> AddRoleMaster(Role role)
        {
            bool isPermitted = predicateBuilder.CanAddOrInsertRoleMaster();
            if (!isPermitted)
            {
                return Result.Failure<bool>(DomainErrors.Authorization.UnauthorizedAccess);
            }
            byte lastRoleId = _academyDbContext.RoleMasters.Select(x => x.RoleId).Max();

            RoleMaster roleMaster = new()
            {
                RoleId = lastRoleId++,
                RoleName = role.RoleName,
                DisplayName = role.DisplayName,
                IsActive = true,
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CreatedOn = DateTime.UtcNow
            };

            await _academyDbContext.RoleMasters.AddAsync(roleMaster);
            var count = await _academyDbContext.SaveChangesAsync();

            return count > 0;
        }

        public async Task<Result<int>> UpdateRoleMaster(byte roleId, JsonPatchDocument<Role> patchRoleDoc)
        {
            bool isPermitted = predicateBuilder.CanAddOrInsertRoleMaster();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }
            RoleMaster existingRoleMaster = await _academyDbContext.RoleMasters.FindAsync(roleId);
            if (existingRoleMaster == null)
            {
                return Result.Failure<int>(DomainErrors.Common.NotFound(roleId.ToString()));
            }

            Role role = new()
            {
                DisplayName = existingRoleMaster.DisplayName
            };

            patchRoleDoc.ApplyTo(role);

            existingRoleMaster.DisplayName = role.DisplayName;
            existingRoleMaster.UpdatedOn = DateTime.UtcNow;
            existingRoleMaster.UpdatedBy = _authenticatedUserService.AuthUser.Id;

            _academyDbContext.Entry(existingRoleMaster).State = EntityState.Modified;
            var count = await _academyDbContext.SaveChangesAsync();

            return count;
        }

        public async Task<Result<bool>> AddEmployeeRole(EmployeeRoleRequest request)
        {
            // Only System Admins are allowed to access this resource
            bool isPermitted = predicateBuilder.CanAddOrInsertRoleMaster();
            if (!isPermitted)
            {
                return Result.Failure<bool>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            // If request is SystemAdmin then soft delete all records and add new 
            // If request is User then soft delete all records
            // else if RoleId And RoleAssignment is already active then ignore
            // else add new one


            List<EmployeeRoleMap> existingEmpRoleMap = await _academyDbContext.EmployeeRoleMaps
                    .Where(x => x.EmployeeId.Equals(request.EmployeeId)
                                                && x.RoleId.Equals(request.SelectedRole)
                                                && x.IsActive).ToListAsync();

            // If the same permission are added, then ignore.
            if (existingEmpRoleMap.Count > 0)
            {
                List<string> existingRolesAssignments = [.. existingEmpRoleMap.Select(x => x.RoleAssignment).OrderBy(x => x)];
                List<string> newRoleAssignments = [.. request.RoleAssignments.OrderBy(x => x)];
                if (existingRolesAssignments.SequenceEqual(newRoleAssignments))
                {
                    return true;
                }
            }

            List<EmployeeRoleMap> data = [];
            // if user is system admin then reset the role assignments in request
            if (request.SelectedRole.Equals((int)Roles.SystemAdmin))
            {
                request.RoleAssignments = [string.Empty];
            }

            // if new role is User, then Delete all existing roles.
            if (request.SelectedRole.Equals((int)Roles.User))
            {
                await DeleteEmployeeRole(null, request.EmployeeId);
            }
            else
            {
                await DeleteEmployeeRole(null, request.EmployeeId);
                foreach (var item in request.RoleAssignments)
                {
                    data.Add(new EmployeeRoleMap()
                    {
                        CreatedBy = _authenticatedUserService.AuthUser.Id,
                        CreatedOn = DateTime.UtcNow,
                        EmployeeId = request.EmployeeId,
                        RoleId = (byte)request.SelectedRole,
                        IsActive = true,
                        RoleAssignment = item
                    });
                }
            }
            if (data.Count > 0)
            {
                await _repositoryEmployeeRoleMap.InsertAsync(data);
                await _unitOfWork.SaveChangesAsync();
            }
            return true;
        }

        public async Task<Result<bool>> DeleteEmployeeRole(int? employeeRoleId, int? employeeId)
        {
            bool isPermitted = predicateBuilder.CanAddOrInsertRoleMaster();
            if (!isPermitted)
            {
                return Result.Failure<bool>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            if (employeeRoleId.HasValue)
            {
                EmployeeRoleMap employeeRole = await _academyDbContext.EmployeeRoleMaps.FirstOrDefaultAsync(x => x.EmployeeRoleId == employeeRoleId.Value && x.IsActive);
                if (employeeRole is not null)
                {
                    employeeRole.IsActive = false;
                    employeeRole.UpdatedOn = DateTime.UtcNow;
                    employeeRole.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                    await _academyDbContext.SaveChangesAsync();
                }
            }
            else if (employeeId.HasValue)
            {
                List<EmployeeRoleMap> employeeRoles = await _academyDbContext.EmployeeRoleMaps.Where(x => x.EmployeeId.Equals(employeeId) && x.IsActive).ToListAsync();
                if (employeeRoles.Count > 0)
                {
                    employeeRoles.ForEach(x =>
                    {
                        x.IsActive = false;
                        x.UpdatedOn = DateTime.UtcNow;
                        x.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                    });
                    await _academyDbContext.SaveChangesAsync();
                }
            }
            return true;
        }
    }
}
