namespace Academy.Shared.DTO
{
    public class FetchDojoActivityRequest
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }        
        public List<string> Country { get; set; }
        public List<string> Community { get; set; }
        public List<string> Account { get; set; }
        public List<string> AiStudio { get; set; }
        public string DojoStartDate { get; set; }
        public string DojoEndDate { get; set; }
        public bool IsPrimaryRecord { get; set; } = true;
        public string SearchText { get; set; }
    }
}
