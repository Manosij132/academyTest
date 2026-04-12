using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class CommunitySelectionRatio : BaseEntity
    {

        [Key]
        public int Id { get; set; }
        public string TDC { get; set; }
        public int CommunityId { get; set; }
        public decimal? L1SelectionRatio { get; set; }
        public decimal? GKSelectionRatio { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
