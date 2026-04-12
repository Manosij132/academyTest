namespace Academy.Domain.Entities
{
    public class JobRequest
    {
        public int RequestId { get; set; }
        public string TransactionId { get; set; }
        public string RequestType { get; set; }
        public string RequestMetadata { get; set; }
        public string Status { get; set; } = "Pending";
        public bool HasErrors { get; set; } = false;
        public string ErrorDetail { get; set; }
        public byte RetryCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
