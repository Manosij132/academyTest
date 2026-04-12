export interface UpdateDojoGexLeaderRequest {
  dojoDetailId: number;
  employeeId: number;
  dojoStartDate: Date;
  dojoEndDate?: Date;
  dojoGexLeaderEmail: string;
  dojoGexGlobarEmail:string;
}
