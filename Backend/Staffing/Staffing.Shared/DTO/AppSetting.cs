using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Shared.DTO
{
    public class AppSetting
    {
        public string DateTimeAsIdFormat { get; set; } = string.Empty;
     
        public string IssuerWebAuthority { get; set; } = string.Empty;
        public string LoggedInUserEmail { get; set; } = string.Empty;
        public JWTSettings JWTSetting { get; set; } = new();
        public bool AuthenticateLocal { get; set; } = false;
        public int SystemUser { get; set; }
        public string AppUri { get; set; }           
        
    }

    public class JWTSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double DurationInMinutes { get; set; } = 0;
    }

}
