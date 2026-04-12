namespace Academy.Shared.DTO
{
    public class DataRequestOptions
    {
        public string SearchText { get; set; }
        public PagingOption PagingOptions { get; set; }
        public List<FilterOption> FilterOptions { get; set; }
        public SortOption SortOptions { get; set; }
    }

    public class PagingOption
    {
        public int PageSize { get; set; } = 20;
        public int PageIndex { get; set; } = 0;
    }

    public class FilterOption
    {
        public string FilterBy { get; set; }
        public string FilterValue { get; set; }
    }

    public class SortOption
    {
        public string SortBy { get; set; }
        public bool SortByDescending { get; set; } = false;
    }
}
