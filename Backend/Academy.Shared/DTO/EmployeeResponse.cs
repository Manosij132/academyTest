namespace Academy.Shared.DTO
{
    public class EmployeeResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string CareerMentorEmail { get; set; }
        public string Position { get; set; }
        public string Project { get; set; }
        public string Client { get; set; }
        public string Tdc { get; set; }
        public string ImageUrl {  get; set; }
        public string Status { get; set; }
        public double TrainingCompletetionScore { get; set; }
        public decimal ProficiencyScore { get; set; }
        public string Seniority { get; set; }
        public string BaseLocation { get; set; }
        public int TotalTrainings { get; set; }
        public int InProgressTrainings { get; set; }
        public int CompletedTrainings { get; set; }
        public string DojoGexLeaderEmail { get; set; }
        public int DojoDetailId { get; set; }
    }
}
