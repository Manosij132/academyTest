namespace Academy.Shared.DTO
{
    public class DashboardTilesDataModel
    {
        public int TotalSlots { get; set; }
        public int L1Slots { get; set; }
        public int GKSlots { get; set; }
        public int L1UntilizedSlots { get; set; }
        public int GKUnutilizedSlots { get; set; }
        public int? L1Deficit { get; set; }
        public int? GKDeficit { get; set; }
    }
}
