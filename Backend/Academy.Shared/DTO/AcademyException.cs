using System.Net;

namespace Academy.Shared.DTO
{
    public class AcademyException: Exception
    {
        public HttpStatusCode Status { get; set; }
    }
}
