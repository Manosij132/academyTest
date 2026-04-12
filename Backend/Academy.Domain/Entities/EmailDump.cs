namespace Academy.Domain.Entities
{
    public class EmailDump : BaseEntity
    {
        public int EmailDumpId { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public string PlainText { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string ErrorText { get; set; }
    }
}
