namespace Academy.Shared.DTO
{
    public class InterviewPanelModel
    {
        public int Id { get; set; }
        public string EmailId { get; set; } = string.Empty;
        public string PanelName { get; set; } = string.Empty;
        public string PanelType { get; set; } = string.Empty;
        public int SeniorityId { get; set; }
        public string SeniorityName { get; set; } = string.Empty;
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public int RequiredSlots { get; set; }
        public int SlotCount { get; set; }
        public int NonUtilizedSlot { get; set; }
        public int? Deficit { get; set; }
        public string? Quater { get; set; } = string.Empty;
        public string TDC { get; set; }
        public string GlobantLeaderEmailId { get; set; }
        public string? CommunityGKFocalEmailId { get; set; }
        public string SeniorityUpTo { get; set; } = string.Empty;
        public List<AllPanelSlots> Slots { get; set; }

        //public InterviewPanelModel()
        //{
        //    Slots = new List<AllPanelSlots>();
        //}
    }
}
