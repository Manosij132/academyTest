namespace Academy.Shared.DTO
{
    public class FetchDojoGlobarsRequest
    {
        public string SearchText { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<string> Community { get; set; }
        public List<string> Country { get; set; }
        public List<string> AiStudio { get; set; }
        public List<string> Account { get; set; }
    }
}
