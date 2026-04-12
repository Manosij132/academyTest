using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Staffing.Shared;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public IActionResult HandleServiceResponse<T>(ServiceResponse<T> response, int successStatusCode = StatusCodes.Status200OK)
        {
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        public ActionResult HandleServiceResponseForActionResult<T>(ServiceResponse<T> response, int successStatusCode = StatusCodes.Status200OK)
        {
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
    }
}
