namespace Academy.Shared.DTO
{
    public class FetchTrainingListResponse
    {
        public int TotalRecords { get; set; }
        public List<Training> TrainingList { get; set; } = [];
    }
}
