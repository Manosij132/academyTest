namespace Academy.Domain.Entities
{
    public class JobRequestDetail
    {
        public int JobRequestDetailId { get; set; }
        public string TransactionId { get; set; }
        public string GlobantEmailAddress { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
