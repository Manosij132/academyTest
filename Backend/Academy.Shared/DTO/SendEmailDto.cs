namespace Academy.Shared.DTO
{
    public class SendEmailDto
    {
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int EmpId { get; set; }
    }
}
