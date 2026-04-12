using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class CommunityGKFocal : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int CommunityId { get; set; } 
        public string GKFocalEmailId { get; set; }
    }
}
