namespace Academy.Domain.Entities
{
    public class RoleMaster: BaseEntity
    {
        public byte RoleId { get; set; }
        public string RoleName { get; set; }
        public string DisplayName { get; set; }
    }
}
