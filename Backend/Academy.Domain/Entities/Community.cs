using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class Community : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
