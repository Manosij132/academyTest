using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
     public class PanelSlotsRequirement : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string TDC { get; set; }
        public int CommunityId { get; set; }
        public short SeniorityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PositionToBeFilled { get; set; }
        public decimal DropRatio { get; set; }
        public int OffersToBeRolledOut { get; set; }
        public int? L1SlotsRequired { get; set; }
        public int? L1SlotsActual { get; set; }
        public int? GKSlotsRequired { get; set; }
        public int? GKSlotsActual { get; set; }
        public decimal? L1SelectionRatio { get; set; }
        public decimal? GKSelectionRatio { get; set; }
        public int? L1Panels { get; set; }
        public int? GKPanels { get; set; }
    }
}
