namespace Academy.Domain.Entities
{
    public class EmployeeRoleMap: BaseEntity
    {
        public int EmployeeRoleId { get; set; }
        public int EmployeeId { get; set; }
        public byte RoleId { get; set; }
        public string RoleAssignment { get; set; }
    }
}
