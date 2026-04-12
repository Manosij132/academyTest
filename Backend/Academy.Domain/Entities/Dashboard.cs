namespace Academy.Domain.Entities
{
    public class Dashboard
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string CareerMentorEmail { get; set; }
        public string Status { get; set; }
        public string Position { get; set; }
        public string Seniority { get; set; }
        public string Designation { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
        public string Tdc { get; set; }
        public string Image { get; set; }
        public string Community { get; set; }
        public double TrainingScore { get; set; }
        public decimal ProficiencyScore { get; set; }
        public string GexLeaders { get; set; }
        public bool IsActive { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string WorkingEcosystem { get; set; }
        public string ProposedDojoGxLeader { get; set; }
        public bool IsProposedGxLeaderOnDojo { get; set; }
        public string CVLink { get; set; }
        public DateTime? CVUpdatedOn { get; set; }
        public string ProfileLink { get; set; }
        public DateTime? ProfileUpdatedOn { get; set; }
        public string Engaged { get; set; }
    }
}
