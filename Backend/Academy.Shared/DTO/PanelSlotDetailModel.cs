namespace Academy.Shared.DTO
{
    public class PanelSlotDetailModel
    {
        public DateTime SlotDate { get; set; }
        public string Recruiter { get; set; }
        public string CandidateName { get; set; }

        public string CandidateEmail { get; set; }
        public string FileEncoded { get; set; }
        public string LoggedInUserEmailID { get; set; }
        public string ResumeFileName { get; set; }
        public string EventTitle { get; set; }
    }
}
