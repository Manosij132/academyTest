namespace Academy.Shared.DTO
{
    public class FetchTrainingListRequest
    {
        public string SearchTearm { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
    }
}
