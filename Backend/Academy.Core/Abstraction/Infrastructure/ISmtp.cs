using Academy.Core.Models;
namespace Academy.Core.Abstraction.Infrastructure
{
    public interface ISmtp<T> where T : ISmtpSettings
    {
        Task SendEmailAsync(string commaSeperatedTo, string subject, string bodyText, string commaSeperatedCc = "", string commSeperatedBcc = "");
    }
}
