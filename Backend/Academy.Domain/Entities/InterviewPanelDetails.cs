using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public partial class InterviewPanelDetails : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string TDC { get; set; }
        public int CommunityId { get; set; }
        public int PrimaryPanelId { get; set; }
        public Int16 SeniorityId { get; set; }  
        public string Type { get; set; }
        public string SeniorityUpTo { get; set; }
        public virtual Employee PrimaryPanel { get; set; }
    }
}
