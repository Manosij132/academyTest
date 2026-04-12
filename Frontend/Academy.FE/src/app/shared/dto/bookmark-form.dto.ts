export class BookmarkForms {
  emailCC: string | null = null;
  emailSubject: string | null = null;
  emailTo: string | null = null;
  BookMarkId: number = 0;
  BookMarkName: string = "";
  TDC: String[] = [];
  Community: String[] = [];
  Trainings: number[] = [];
  Seniorities: number[] = [];
  Projects: number[] = [];
  Statuses: number[] = [];
  ReportType: number = 0;
  SelectColumns: number[] = [];
  GroupByColumns: number[] = [];
  EmployeeId: number[] = [];
  AreaPaths: number[] = [];
  PrimaryActivities: number[] = [];
  activityOptions: number[] = [];
  DateTypeFilter: DateTypeFilter | null = null;
  FromDate: string | null = null;
  ToDate: string | null = null;
  ActivityType: number[] = [];
  DateTypeFilterSelect: number[] = [];
  Client: string[] =[];

  static fromBookmarkDto(b: any, bookMarkId: number): any {
    return {
      ...b,
      BookMarkId:      bookMarkId,
      Community:       b.communities       ?? [],
      SelectColumns:   b.configureColumns  ?? [],
      activityOptions: b.activityOptions   ?? [],
      EmployeeId:      b.employees?.map((e: any) => e.employeeId) ?? [],
    };
  }
}

export interface EmailColumnsModel {
  emailCC: string;
  emailSubject: string;
  emailTo: string;
  emailBody: string;
}

export interface DateTypeFilter {
  Type: string | "";
  FromDate: Date | null;
  ToDate: Date | null;
}