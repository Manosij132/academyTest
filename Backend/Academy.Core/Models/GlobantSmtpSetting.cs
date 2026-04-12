using Academy.Shared.DTO;
using Microsoft.Extensions.Configuration;

namespace Academy.Core.Models
{
    public class BaseSmtpSetting
    {
        protected GlobantSmtpSettings _globantSmtpSetting;
        public string Host => _globantSmtpSetting.Host;
        public int Port => _globantSmtpSetting.Port;
        public string UserName => _globantSmtpSetting.SenderEmail;
        public string Password => _globantSmtpSetting.SenderKey;
        public bool UseDefaultCredentials => _globantSmtpSetting.UseDefaultCredentials;
        public bool EnableSsl => _globantSmtpSetting.RequireSsl;
        public string SenderEmail => _globantSmtpSetting.SenderEmail;
        public string SenderName => _globantSmtpSetting.SenderDisplayName;
    }
    public class GlobantSmtpSetting : BaseSmtpSetting, ISmtpSettings
    {
        
        public GlobantSmtpSetting(IConfiguration configuration)
        {
            _globantSmtpSetting = configuration
                            .GetSection("AppSetting:GlobantSmtpSetting")
                            .Get<GlobantSmtpSettings>();
        }
    }

    public class BrevoSmtpSetting : BaseSmtpSetting, ISmtpSettings 
    {
        public BrevoSmtpSetting(IConfiguration configuration)
        {
            _globantSmtpSetting = configuration
                            .GetSection("AppSetting:BrevoSmtpSetting")
                            .Get<GlobantSmtpSettings>();
        }
    }
}
