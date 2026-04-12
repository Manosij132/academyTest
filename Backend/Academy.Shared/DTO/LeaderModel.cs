namespace Academy.Shared.DTO
{
    public class LeaderModel
    {
        public int Id { get; set; }
        public int MenteesCount { get; set; }
        public string EmployeeName { get; set; }
        public string GlobantEmailAddress { get; set; }
        public string? Seniority { get; set; }
        public string Designation { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
        public string ProposedLeaderEmail { get; set; }
        public string GexLeaders { get; set; }
        public string? BetterMeLeaderEmail { get; set; }
        public string SeniorityName { get; set; }
        public int SeniorityId { get; set; }
        public bool IsDeleted { get; set; }
        public int MinMentee { get; set; }
        public int MaxMentee { get; set; }
        public bool IsLeader { get; set; }
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string Desc
        {
            get
            {
                return string.Format($"{SeniorityName}-{Client}-{EmployeeName}");
            }
        }
        public string LeaderAssignDate { get; set; }
        public DateTime? InOutDate { get; set; }//added by shweta
        public bool InOut { get; set; }//added by shweta
        public string tdc { get; set; }
    }
}