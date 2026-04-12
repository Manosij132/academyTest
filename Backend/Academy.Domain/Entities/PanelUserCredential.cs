using System.ComponentModel.DataAnnotations;

namespace Academy.Domain.Entities
{
    public class PanelUserCredential : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiryTime { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
