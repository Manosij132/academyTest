namespace Academy.Domain.Entities
{
    public class Employee: BaseEntity
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string GlobantEmailAddress { get; set; }
        public string BetterMeLeaderEmail { get; set; }
        public string Seniority { get; set; }
        public short? SeniorityId { get; set; } = 0;
        public string Tdc { get; set; }
        public string Community { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
        public string BaseLocation { get; set; }
        public string Designation { get; set; }
        public string Position { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string MobileNo { get; set; }
        public decimal TotalExperience { get; set; }
        public decimal Aging { get; set; }
        public string Gender { get; set; }
        public short? NoOfDays { get; set; }
        public int? NotificationSendCount { get; set; }
        public string ProjectManagerEmail { get; set; }
        public string ProjectTL { get; set; }
        public string ProjectTLEmailsCsv { get; set; }
        public string ProposedLeaderEmail { get; set; }
        public string GlobalId { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public int? OnHoldBy { get; set; }
        public bool? OnHoldForProject { get; set; }
        public string OtherInfo { get; set; }
        public string ProfileLink { get; set; }
        public string ResumeLink { get; set; }
        public bool? IsNewJoiner { get; set; }
        public string Comments { get; set; }
        public string GexLeaders { get; set; }
        public short? MyGrowthReminderCount { get; set; }
        public string WorkingEcosystem { get; set; }
        public int? EcosystemId { get; set; }
        public string? AiStudio { get; set; }
    }
}
