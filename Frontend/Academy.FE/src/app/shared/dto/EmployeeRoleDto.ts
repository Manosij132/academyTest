export class EmployeeRoleDto {
  employeeId: number = 0;
  employeeName: string = '';
  globantEmailAddress: string = '';
  seniority: string = '';
  roles: Role[] = [];
}

export interface Role {
  roleId: number; 
  roleName: string;
  roleAssignment: string;
}
