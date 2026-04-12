namespace Academy.Shared.DTO
{
    public class EmployeeRole
    {
        public int EmployeeRoleId { get; set; }
        public int EmployeeId { get; set; }
        public byte RoleId { get; set; }
        public string RoleAssignment { get; set; }
    }
}
