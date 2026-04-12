namespace Academy.Shared.DTO
{
    public class CommentResponse
    {
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
        public string CommentBy { get; set; }
        public string CommentByImage { get; set; }
        public int CommentByEmpId { get; set; }

    }
}
