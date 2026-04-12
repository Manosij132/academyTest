using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;
using Microsoft.AspNetCore.JsonPatch;

namespace Academy.Core.Abstraction.Services
{
    public interface IRoleService
    {
        Task<Result<List<RoleMaster>>> GetRoleMaster();
        Task<Result<bool>> AddRoleMaster(Role role);
        Task<Result<int>> UpdateRoleMaster(byte roleId, JsonPatchDocument<Role> patchRoleDoc);
        Task<Result<bool>> AddEmployeeRole(EmployeeRoleRequest request);
    }
}
