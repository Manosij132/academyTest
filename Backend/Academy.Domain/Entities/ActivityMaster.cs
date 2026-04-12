
namespace Academy.Domain.Entities
{
    public class ActivityMaster : BaseEntity
    {
        public short ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string? ActivityDescription { get; set; }
        public int Priority { get; set; }
    }
}
