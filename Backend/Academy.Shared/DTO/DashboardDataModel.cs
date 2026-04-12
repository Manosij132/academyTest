namespace Academy.Shared.DTO
{
    public class DashboardDataModel
    {
        public List<InterviewPanelModel> PanelData { get; set; }
        public DashboardTilesDataModel DashboardTiles { get; set; }
        public List<ChartDataModel> CommunityChartDataModel { get; set; }
        public List<ChartDataModel> PanelTypeChartDataModel { get; set; }
        public List<InterviewScheduleData> InterviewScheduleData { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
