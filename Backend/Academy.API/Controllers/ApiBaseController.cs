using Academy.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
        //To add code common to all controllers

        protected IActionResult ToActionResult<T>(T result)
        {
            AcademyResponse<T> response = new()
            {
                Data = result,
                Status = HttpStatusCode.OK,
                Success = true
            };

            return Ok(response);
        }
    }
}
