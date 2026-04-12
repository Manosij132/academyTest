using Academy.Domain.Entities;
namespace Academy.Workers.ReportWorker.ReportsCore
{
    internal interface IReport : IDisposable
    {
        Task StartProcess(JobRequest jobRequest);
    }
}
