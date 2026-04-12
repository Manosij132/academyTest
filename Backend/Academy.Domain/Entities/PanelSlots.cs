using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class PanelSlots : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int PanelId { get; set; }
        public DateTime SlotDate { get; set; }
        public string Recruiter { get; set; }
        public string CandidateName { get; set; }
        public bool IsUtilized { get; set; }
        public string LoggedinUserEmailId { get; set; }
        public string EventTitle { get; set; }
        public string CandidateEmail { get; set; }
        public string ResumeFileName { get; set; }
        public string FileEncoded { get; set; }
        public string CalenderEventID { get; set; }
    }
}
