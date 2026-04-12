export interface BookmarkFilterData {
  AllReportTypes: [];
  AllTdc: [];
  AllCommunitySettings: [];
  AllClients: [];
  Seniorities: [];
  AllTrainings: [];
  AllProjects: [];
  AllTrainingStatus: [];
  AllSelectColumns: [];
  AllGroupByColumns: [];
  AllAreaPaths: [];
  AllActivitiesType: [];
  AllActivities: [];
}

export interface User {
  employeeId: number;
  employeeName: string;
  globantEmailAddress: string;
  seniority: string;
}

export interface ApiResponse {
  success: boolean;
  data: string;
  error?: string;
}