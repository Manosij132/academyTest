using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;

namespace Staffing.Core.Abstraction.Services
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUser AuthUser { get; set; } = new();

        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                var ctx = httpContextAccessor?.HttpContext;
                var principal = ctx?.User;
                if (principal?.Identity?.IsAuthenticated != true) return;

                // 1) existing serialized claim (Academy)
                var claimJson = principal.FindFirst("claimjson")?.Value;
                if (!string.IsNullOrWhiteSpace(claimJson))
                {
                    try
                    {
                        var deserialized = JsonSerializer.Deserialize<AuthenticatedUser>(claimJson);
                        if (deserialized != null)
                        {
                            AuthUser = deserialized;
                            return;
                        }
                    }
                    catch
                    {
                        // fall through to claim mapping
                    }
                }

                // 2) map common claims
                var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? principal.FindFirst("sub")?.Value
                              ?? principal.FindFirst(ClaimTypes.Email)?.Value;

                if (int.TryParse(idClaim, out var numericId))
                    AuthUser.Id = numericId;

                AuthUser.IsAuthenticated = true;
                AuthUser.GloberEmail = principal.FindFirst("GloberEmail")?.Value
                                      ?? principal.FindFirst(ClaimTypes.Email)?.Value
                                      ?? string.Empty;

                AuthUser.Name = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                if (!roles.Any()) roles = principal.FindAll("role").Select(c => c.Value).ToList();

                var rolesProp = typeof(AuthenticatedUser).GetProperty("Roles");
                if (rolesProp != null)
                {
                    if (rolesProp.PropertyType == typeof(string[]))
                        rolesProp.SetValue(AuthUser, roles.ToArray());
                    else if (rolesProp.PropertyType == typeof(System.Collections.Generic.List<string>))
                        rolesProp.SetValue(AuthUser, roles);
                }
            }
            catch
            {
                // swallow - leave AuthUser default (unauthenticated)
            }
        }
    }
}