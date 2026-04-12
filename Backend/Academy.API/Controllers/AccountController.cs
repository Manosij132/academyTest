using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ApiBaseController
    {
        private readonly AppSetting _appSetting;
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IOptions<AppSetting> appSetting, IAuthenticationService authenticationService)
        {
            _appSetting = appSetting.Value;
            _authenticationService = authenticationService;
        }

        [HttpGet("Health")]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet("Authenticate")]
        public async Task<IActionResult> Authenticate()
        {
            AcademyResponse<string> academyResponse = new();

            //Authenticate google token from the Http Request
            var result = await _authenticationService.ValidateGoogleToken(HttpContext);

            if (result.IsFailure)
            {
                academyResponse.Success = result.IsSuccess;
                academyResponse.Error = result.Error;
                return Unauthorized(academyResponse);
            }

            var authResult = await _authenticationService.AuthenticateUser(result.Value);

            if (authResult.IsFailure)
            {
                academyResponse.Success = authResult.IsSuccess;
                academyResponse.Error = authResult.Error;
                return Unauthorized(academyResponse);
            }

            academyResponse.Data = authResult.Value;
            academyResponse.Status = HttpStatusCode.OK;
            academyResponse.Success = true;

            return Ok(academyResponse);
        }
    }
}
