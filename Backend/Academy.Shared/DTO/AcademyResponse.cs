using Academy.Shared.Response;
using System.Net;

namespace Academy.Shared.DTO
{
    public class AcademyResponse<T>
    {
        public string TimeStamp { get; } = DateTime.UtcNow.ToString("ddMMyyyyHHmmssfff");
        public bool Success { get; set; } = false;
        public HttpStatusCode Status { get; set; }
        public T Data { get; set; }
        public string ErrorResponseId { get; set; }
        public Error Error { get; set; }
        public string StackTrace { get; set; }
        public string Message { get; set; }
    }
}
