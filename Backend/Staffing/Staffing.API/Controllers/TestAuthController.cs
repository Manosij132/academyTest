using Academy.Core.Abstraction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Staffing.API.Controllers
{
    [ApiController]
    [Route("api/testauth")]
    public class TestAuthController : ControllerBase
    {
        private readonly IAuthenticatedUserService _auth;
        public TestAuthController(IAuthenticatedUserService auth) => _auth = auth;

        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI() => Ok(_auth.AuthUser);
    }
}
