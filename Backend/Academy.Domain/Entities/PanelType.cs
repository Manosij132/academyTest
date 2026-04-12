using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class PanelType : BaseEntity
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
