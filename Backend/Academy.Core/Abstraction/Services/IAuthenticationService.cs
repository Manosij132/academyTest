using Academy.Shared.DTO;
using Academy.Shared.Response;
using Microsoft.AspNetCore.Http;

namespace Academy.Core.Abstraction.Services
{
    public interface IAuthenticationService
    {
        Task<Result<string>> AuthenticateUser(string email);
        Task<Result<AuthenticatedUser>> FetchAuthenticatedUser(string email);
        Task<Result<string>> ValidateGoogleToken(HttpContext httpContext);
    }
}
