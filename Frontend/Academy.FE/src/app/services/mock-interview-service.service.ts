import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { ScheduleInterviewRequest } from '../shared/Interface/mock-interview';
import { environment } from "../../environments/environment";
import { Observable } from 'rxjs';

export interface UserInterviewStatisticsDTO {
  averageScore: number;
  totalTrainings: number;
  scheduledTrainings: number;
  completedTrainings: number;
}



@Injectable({
  providedIn: 'root'
})
export class MockInterviewServiceService {


  private mockApiUrl :string;

  constructor(private http: HttpClient) {
   this.mockApiUrl = `${environment.apiMockinterviewBaseURL}/mock-interviews`
  }

  fetchSkills() {
    return this.http.get<any[]>(`${this.mockApiUrl}/skills`);
  }

  fetchSeniority() {
    return this.http.get<any[]>(`${this.mockApiUrl}/seniority`);
  }

  // fetchEmployeeMockInterviewHistory(employeeId: number) {
  //   return this.http.get<any[]>(`${this.mockApiUrl}/employees/${employeeId}`);
  // }

  scheduleInterview(request: ScheduleInterviewRequest) {
    let url = `${this.mockApiUrl}/schedule`
    return this.http.post(url, request);
  }
  
  // getEmployeeInterviewStatistics(employeeId: number): Observable<UserInterviewStatisticsDTO> {
  //   const statsUrl = `${environment.apiMockinterviewBaseURL}/mock-interviews/employee/statistics/${employeeId}`;
  //   return this.http.get<UserInterviewStatisticsDTO>(statsUrl);
  // }

}
