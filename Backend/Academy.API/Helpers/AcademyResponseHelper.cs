using Academy.Shared.DTO;
using Academy.Shared.Response;
using System.Net;

namespace Academy.API.Helpers
{
    public static class ApiResponseHelper
    {
        public static AcademyResponse<T> ToAcademyResponse<T>(Result<T> result)
        {
            return new AcademyResponse<T>
            {
                Data = result.IsSuccess ? result.Value : default,
                Status = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                Success = result.IsSuccess,
                Error = result.IsFailure ? result.Error : null
            };
        }
    }
}
