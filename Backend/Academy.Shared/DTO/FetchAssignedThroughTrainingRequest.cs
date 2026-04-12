namespace Academy.Shared.DTO
{
    public class FetchAssignedThroughTrainingRequest
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public List<string> Country { get; set; } = new();
        public List<string> Community { get; set; } = new();
        public List<string> Account { get; set; }
        public List<string> AiStudio { get; set; }
        public string DojoStartDate { get; set; }
        public string DojoEndDate { get; set; }
    }
}
