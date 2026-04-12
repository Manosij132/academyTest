using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Services
{
    public interface ISendEmailService
    {
        Task SendEmail(SendEmailDto sendEmailDto);
    }
}
