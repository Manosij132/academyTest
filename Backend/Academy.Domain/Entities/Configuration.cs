namespace Academy.Domain.Entities
{
    public class Configuration : BaseEntity
    {
        public short ConfigurationId { get; set; }
        public string Environment { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
