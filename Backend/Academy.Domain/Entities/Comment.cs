namespace Academy.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public int CommentId { get; set; }
        public int EmployeeId { get; set; }
        public string CommentText { get; set; }
    }
}
