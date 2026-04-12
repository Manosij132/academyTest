using Microsoft.AspNetCore.Http;
using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Academy.Core.Services
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUser AuthUser { get; set; }

        public AuthenticatedUserService(IHttpContextAccessor contextAccessor)
        {
            ClaimsPrincipal user = contextAccessor.HttpContext?.User;
            ClaimsIdentity identity = (ClaimsIdentity)user.Identity;
            if (identity?.IsAuthenticated == true)
            {
                AuthUser = JsonConvert.DeserializeObject<AuthenticatedUser>(user.FindFirst("claimjson").Value);
            }
        }
    }
}
