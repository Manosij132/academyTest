using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Academy.Core.Services
{
    public class EmailDumpService : IEmailDumpService
    {
        private readonly AppSetting _appSetting;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EmailDump> _repossitoryEmailDump;
        private readonly ISmtp<GlobantSmtpSetting> _smtpSettings;
        private readonly IEmployeeService _employeeService;
        public EmailDumpService(IOptions<AppSetting> appSetting, IUnitOfWork unitOfWork, ISmtp<GlobantSmtpSetting> smtpSettings, IEmployeeService employeeService)
        {
            _appSetting = appSetting.Value;
            _unitOfWork = unitOfWork;
            _repossitoryEmailDump = _unitOfWork.GetRepository<EmailDump>();
            _smtpSettings = smtpSettings;
            _employeeService = employeeService;
        }

        public async Task SendEmail()
        {
            var email = await _repossitoryEmailDump.GetFirstOrDefaultAsync(predicate: x => x.IsActive == false && string.IsNullOrWhiteSpace(x.ErrorText));
            if (email != null) 
            {
                var employee = await _employeeService.FetchById(email.CreatedBy);
                if (employee == null) 
                {
                    email.ErrorText = string.Format(Messages.ERROR_EmployeeIsNull, email.To);
                }
                else
                {
                    try
                    {
                        string body = await File.ReadAllTextAsync(email.Template);
                        
                        await _smtpSettings.SendEmailAsync(email.To, email.Subject, body, email.Cc, email.Bcc);
                        email.UpdatedOn = DateTime.UtcNow;
                        email.UpdatedBy = _appSetting.SystemUser;
                        email.IsActive = true;
                    }
                    catch (Exception ex)
                    {
                        email.ErrorText = ex.Message;
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }
     
    }
}
