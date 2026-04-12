using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class InterviewData : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int? L1Select { get; set; }
        public int? L1Reject { get; set; }
        public int? GKSelect { get; set; }
        public int? GKReject { get; set; }
        public int? GrandTotal { get; set; }
    }
}
