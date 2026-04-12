using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Services
{
    public interface IReminderService
    {
        Task<IList<ReminderResponse>> FetchReminderSummary();
        Task<IList<ReminderResponse>> FetchReminderSummary(int employeeId);
        Task<int> InsertReminder(int employeeId);
        Task<int> InsertReminders();
    }
}
