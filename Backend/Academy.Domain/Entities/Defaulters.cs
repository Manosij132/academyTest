using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class Defaulters : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int PanelId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DefaulterCount { get; set; }
        public string Quarter { get; set; }
    }
}
