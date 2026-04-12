using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Shared.DTO;
using Microsoft.Extensions.Options;

namespace Academy.Core.Services
{
    public class SendEmailService : ISendEmailService
    {
        private readonly AppSetting _appSetting;
        private readonly ISmtp<GlobantSmtpSetting> _smtp;

        public SendEmailService(IOptions<AppSetting> appSetting, ISmtp<GlobantSmtpSetting> smtp)
        {
            _appSetting = appSetting.Value;
            _smtp = smtp;
        }

        public async Task SendEmail(SendEmailDto sendEmailDto)
        {
            if (!_appSetting.SendEmailEnabled)
            {
                await Console.Out.WriteLineAsync("Email Send is not enabled");
                return;
            }
            else
            {
                try
                {
                    await _smtp.SendEmailAsync(sendEmailDto.To, sendEmailDto.Subject, sendEmailDto.Body, sendEmailDto.CC);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }
    }
}
