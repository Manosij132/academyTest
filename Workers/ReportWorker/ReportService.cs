using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Workers.ReportWorker.ReportsCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
namespace Academy.Workers.ReportWorker
{
    public class ReportService : BackgroundService
    {
        private AppSetting _appSetting;
        private readonly IServiceProvider _serviceProvider;
        private IAcademyDbContext? _dbContext;
        Dictionary<string, IReport> factory = [];
        public ReportService(IOptions<AppSetting> appSetting, IServiceProvider serviceProvider)
        {
            _appSetting = appSetting.Value;
            _serviceProvider = serviceProvider;
            if (!factory.ContainsKey(ExportReportType.FullReport.ToString()))
                factory.Add(ExportReportType.FullReport.ToString(), new FullReport(_serviceProvider, _appSetting));
            if (!factory.ContainsKey(ExportReportType.DetailReport.ToString()))
                factory.Add(ExportReportType.DetailReport.ToString(), new DetailReport(_serviceProvider, _appSetting));
            if (!factory.ContainsKey(ExportReportType.Compliance.ToString()))
                factory.Add(ExportReportType.Compliance.ToString(), new ComplianceReport(_serviceProvider, _appSetting));
            if (!factory.ContainsKey(ExportReportType.Summary.ToString()))
                factory.Add(ExportReportType.Summary.ToString(), new SummaryReport(_serviceProvider, _appSetting));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_appSetting.ExportReportWorkerConfig.Enabled)
                {
                    //await Console.Out.WriteLineAsync("Export Report worker is not enabled");
                    return;
                }
                else
                {
                    await Do_Work();
                }
                await Task.Delay(TimeSpan.FromSeconds(30),stoppingToken);
            }
        }
        private async Task Do_Work()
        {
            //Console.WriteLine($"[ReportService >> Do_Work] Started...");

            using var scope = _serviceProvider.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<IAcademyDbContext>();

            JobRequest? job = _dbContext.JobRequests.FirstOrDefault(x => x.IsActive && x.Status == TrainingStatus.Pending.ToString()
                            && x.RequestType == JobRequestType.Report.ToString());
            if (job is null)
            {
                //Console.WriteLine($"[ReportService >> Do_Work] No Jobs Found...");
            }
            else
            {
                ExportReportMetadata metadata = JsonConvert.DeserializeObject<ExportReportMetadata>(job.RequestMetadata) ?? new();
                if (factory.ContainsKey(metadata.Type))
                {
                    using IReport export = factory[metadata.Type];
                    await export.StartProcess(job);
                }
                else
                {
                    //Console.WriteLine($"[ReportService >> Do_Work] Unknown Command Type {job.RequestMetadata}...");
                }
            }
            //Console.WriteLine($"[ReportService >> Do_Work] Completed...");
        }
    }
}