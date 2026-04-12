namespace Academy.Shared.DTO
{
    public class AssignedThroughTrainingReportFilter
    {
        public List<string> Community { get; set; }
        public List<string> Country { get; set; }
        public List<string> AiStudio { get; set; }
        public List<string> Account { get; set; }
        public string DojoStartDate { get; set; }
        public string DojoEndDate { get; set; }
    }
}
