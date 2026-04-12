namespace Academy.Shared.DTO
{
    public record Role
    {
        public byte RoleId;
        public string RoleName;
        public string RoleAssignment;
        public string DisplayName;
    }

    public class RoleDto
    {
        public byte RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleAssignment { get; set; }
    }
}
