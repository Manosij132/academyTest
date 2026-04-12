namespace Academy.Shared.DTO
{
    public class SlotRequirementModel
    {
        public int Id { get; set; }
        public string TDC { get; set; }
        public int CommunityId { get; set; }
        public short SeniorityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PositionToBeFilled { get; set; }
        public Decimal DropRatio { get; set; }
        public int OffersToBeRolledOut { get; set; }
        public int? L1SlotsRequired { get; set; }
        public int? L1SlotsActual { get; set; }
        public int? GKSlotsRequired { get; set; }
        public int? GKSlotsActual { get; set; }
        public Decimal? L1SelectionRatio { get; set; }
        public Decimal? GKSelectionRatio { get; set; }
        public int? L1Panels { get; set; }
        public int? GKPanels { get; set; }
    }
}
