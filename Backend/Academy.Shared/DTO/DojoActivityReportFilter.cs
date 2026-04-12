namespace Academy.Shared.DTO
{
    public class DojoActivityReportFilter
    {        
        public List<string> Community { get; set; }
        public List<string> Country { get; set; }
        public List<string> AiStudio { get; set; }
        public List<string> Account { get; set; }
        public string DojoStartDate { get; set; }
        public string DojoEndDate { get; set; }
        public bool IsPrimaryRecord { get; set; }
        public string SearchText { get; set; }
    }
}