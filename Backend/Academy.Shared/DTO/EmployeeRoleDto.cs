namespace Academy.Shared.DTO
{
    public class EmployeeRoleDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string GlobantEmailAddress { get; set; }
        public string Seniority { get; set; }
        public List<RoleDto> Roles { get; set; }
    }

    public class EmployeeRoleRequest
    {
        public int EmployeeId { get; set; }
        public int SelectedRole { get; set; }
        public List<string> RoleAssignments { get; set; } = [];
    }
}
