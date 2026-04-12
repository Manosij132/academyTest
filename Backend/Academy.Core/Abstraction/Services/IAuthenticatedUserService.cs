using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Services
{
    public interface IAuthenticatedUserService
    {
        AuthenticatedUser AuthUser { get; set; }        
    }
}
