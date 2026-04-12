using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Academy.Workers.ReminderWorker
{
    public class ReminderService : BackgroundService
    {
        private readonly AppSetting _appSetting;
        private readonly IServiceProvider _serviceProvider;
        public readonly EmailSetting _emailSetting;

        public ReminderService(IOptions<AppSetting> appSetting, IServiceProvider serviceProvider)
        {
            _appSetting = appSetting.Value;
            _serviceProvider = serviceProvider;
            _emailSetting = _appSetting.EmailSettings.Find(x => x.Key == "DAILY_REMINDER");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_appSetting.ReminderWorkerConfig.Enabled)
            {
                await Console.Out.WriteLineAsync("Reminder worker is not enabled");
            }
            else
            {
                await Do_Work();
            }
        }

        private async Task Do_Work()
        {
            Console.WriteLine("[ReminderService >> Do_Work] Started...");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                IAdoClient<AcademyDbSetting> _academyAdoClient = scope.ServiceProvider.GetService<IAdoClient<AcademyDbSetting>>();
                Dictionary<string, object> iParam = new()
                    {
                        { DbConstants.PARAM_REMINDER_EMAIL_SUBJECT, _emailSetting?.Subject },
                        { DbConstants.PARAM_REMINDER_EMAIL_TEMPLATE, _emailSetting.Key },
                        { DbConstants.PARAM_BCC, _appSetting.DailyRemindersBcc }
                    };
                int result = await _academyAdoClient.ExecuteNonQueryAsync(DbConstants.EXECUTE_REMINDERS, iParam);
                Console.WriteLine($"[ReminderService >> Do_Work] Reminders Execution Completed...Total Rows Affected: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReminderService >> Do_Work] Exception Occured...{ex.Message}");
            }
            Console.WriteLine("[ReminderService >> Do_Work] Completed...");
        }
    }
}
