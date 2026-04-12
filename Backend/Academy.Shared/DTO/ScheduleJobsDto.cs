namespace Academy.Shared.DTO
{
    public class ScheduleJobsDto
    {
        public int ScheduledJobId { get; set; }
        public string JobName { get; set; }
        public string JobDescription { get; set; }
        public string? JobSchedule { get; set; }
        public string JobState { get; set; }
        public bool IsActive { get; set; }

    }
}
