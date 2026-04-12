using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Shared.Constants;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Academy.Shared.Enums;
namespace Academy.Workers.SendEmailWorker
{
    public class SendEmailService : BackgroundService
    {
        private readonly AppSetting _appSetting;
        private readonly ISmtp<BrevoSmtpSetting> _smtp;
        private readonly IServiceProvider _serviceProvider;
        private IAcademyDbContext? dbContext;
        private IUnitOfWork? _unitOfWork;
        public SendEmailService(IOptions<AppSetting> appSetting, ISmtp<BrevoSmtpSetting> smtp, IServiceProvider serviceProvider)
        {
            _appSetting = appSetting.Value;
            _smtp = smtp;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                if (!_appSetting.EmailWorkerConfig.Enabled)
                {
                    await Console.Out.WriteLineAsync("Email worker is not enabled");
                    return;
                }
                else
                {
                    await Do_Work();
                }
            }
        }

        private string ReplaceYear(string bodyText)
        {
            return bodyText.Replace(ApplicationConstants.HTML_PLACEHOLDER_YEAR, DateTime.UtcNow.Year.ToString());
        }
        private string ReplaceGlober(string bodyText, Employee emp)
        {
            return bodyText.Replace(ApplicationConstants.HTML_PLACEHOLDER_GLOBER, emp.EmployeeName);
        }
        private string ReplaceAppUri(string bodyText)
        {
            return bodyText.Replace(ApplicationConstants.HTML_PLACEHOLDER_APP_URI, _appSetting.AppUri);
        }

        private string ReplaceTrainings(string bodyText, Employee employee, out List<int> ids)
        {
            string data = string.Empty;
            var trainings = (from ET in dbContext.EmployeeTrainingMaps
                             join S in dbContext.SkillMasters on ET.SkillId equals S.SkillId
                             join T in dbContext.TrainingMasters on ET.TrainingId equals T.TrainingId
                             where ET.EmployeeId == employee.Id
                             && !ET.EmailSent && ET.IsActive
                             select new { ET.EmployeeTrainingId, S.SkillName, T.TrainingName, T.TrainingUrl }).ToList();
            foreach (var trng in trainings)
            {
                data += "<a href='" + trng.TrainingUrl + "'>" + trng.TrainingName + "</a><br/>";
            }
            ids = trainings.Select(x => x.EmployeeTrainingId).ToList();
            return bodyText.Replace(ApplicationConstants.HTML_PLACEHOLDER_APP_URI, data);
        }
        private void MarkEMailSent(List<int> ids)
        {
            var trainings = (from ET in dbContext.EmployeeTrainingMaps where !ET.EmailSent && ids.Contains(ET.EmployeeTrainingId) select ET).ToList();
            trainings.ForEach(x =>
            {
                x.EmailSent = true;
                x.UpdatedBy = _appSetting.SystemUser;
                x.UpdatedOn = DateTime.UtcNow;
            });
            dbContext.EmployeeTrainingMaps.UpdateRange(trainings);
        }
        private async Task Do_Work()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IRepository<EmailDump> _repEmailDump = _unitOfWork.GetRepository<EmailDump>();
                IRepository<Employee> _repEmployee = _unitOfWork.GetRepository<Employee>();
                dbContext = scope.ServiceProvider.GetRequiredService<IAcademyDbContext>();
                EmailDump emaildump = await _repEmailDump.GetFirstOrDefaultAsync(predicate: x => x.IsActive);
                if (emaildump == null)
                {
                    return;
                }
                emaildump.UpdatedOn = DateTime.UtcNow;
                emaildump.UpdatedBy = _appSetting.SystemUser;
                Employee employee = await _repEmployee.GetFirstOrDefaultAsync(predicate: x => x.Id == emaildump.CreatedBy);
                if (employee == null)
                {
                    emaildump.ErrorText = "Employee is null";
                    emaildump.IsActive = false;
                }
                else
                {
                    try
                    {
                        List<int> ids = [];
                        string body = string.Empty;
                        string file = $"EmailTemplates/{emaildump.Template}.HTM";

                        if (string.IsNullOrEmpty(emaildump.Template) && string.IsNullOrEmpty(emaildump.PlainText))
                        {
                            emaildump.ErrorText = $"Template and PlainText, both are empty";
                            emaildump.IsActive = false;
                        }

                        else if (string.IsNullOrEmpty(emaildump.Template) && !string.IsNullOrEmpty(emaildump.PlainText))
                        {
                            body = emaildump.PlainText;
                        }
                        else
                        {
                            body = emaildump.Template;
                            if (!File.Exists(file))
                            {
                                emaildump.ErrorText = "Template Not Found";
                                emaildump.IsActive = false;
                            }
                            else
                            {
                                body = await File.ReadAllTextAsync(file);
                                body = ReplaceGlober(body, employee);
                                body = ReplaceAppUri(body);
                                body = ReplaceYear(body);
                                if (emaildump.Template == Template.GU_USER_ADDED.ToString())
                                {
                                    body = ReplaceTrainings(body, employee, out ids);
                                }
                            }
                        }
                        await _smtp.SendEmailAsync(emaildump.To, emaildump.Subject, body, emaildump.Cc, emaildump.Bcc);
                        emaildump.IsActive = false;
                        if(ids.Count > 0)
                        {
                            MarkEMailSent(ids);
                        }
                    }
                    catch (Exception ex)
                    {
                        emaildump.ErrorText = ex.Message;
                        emaildump.IsActive = false;
                    }
                }
                _repEmailDump.Update(emaildump);
                int result = await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}