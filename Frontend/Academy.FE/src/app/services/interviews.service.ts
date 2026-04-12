import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable,map } from 'rxjs';
import { environment } from "@environments/environment";

export interface Interview {
  id?: number;
  interviewType?: string;
  profile?: any;
  status?: string;
  sectionStatus?: any[];
  candidate: { id: number, name?: string, email?: string };
  interviewCode?: string;
  createdAt?: string;
  updatedAt?: string;
  skills?: any[];
  seniority?:{ id: number, name?: string};
  profileImage?: any;
  comments?: string;
  score?: number;
  totalScore?: number;
  tdc?: string;
  community?: string;
  evaluationType?:string;
  account?:string;
  position?:string;
  candidateId?:number;
  profileId?:number;
  interviewLink?:string;
  ccEmailIds?: string;
  scheduleDateTime?: string;
  scheduledAt?: string;
  completedAt?: string;
}

export interface SelfRating {
  skillName: string;
  ratingOutOfFive: number;
}

@Injectable({
  providedIn: 'root'
})
export class InterviewsService {
  private apiUrl: string;
  private emailBaseUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}`;
    this.emailBaseUrl = `${environment.emailBaseUrl}`
  }

  getAll(): Observable<Interview[]> {
    return this.http.get<Interview[]>(`${this.apiUrl}/interviews`);
  }

  getById(id: number): Observable<Interview> {
    return this.http.get<Interview>(`${this.apiUrl}/interviews/${id}`);
  }

  create(interview: Interview): Observable<Interview> {
    return this.http.post<Interview>(`${this.apiUrl}/interviews`, interview);
  }

  update(id: number, interview: Interview): Observable<Interview> {
    return this.http.put<Interview>(`${this.apiUrl}/interviews/${id}`, interview);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/interviews/${id}`);
  }

  getInterviewSummary(code: string) {
    return this.http.get<void>(`${this.apiUrl}/api/v1/mistral/summary?interviewCode=${code}`)
  }

  fetchInterviewDetails(code: string) {
    return this.http.get<void>(`${this.apiUrl}/api/interview-details`)
  }

  fetchInterviewAnalysisDetails(code: string) {
    return this.http.get<void>(`${this.apiUrl}/api/interview-detail-analysis`)
  }

  fetchModelScoringDetailsDetails(code: string) {
    return this.http.get<void>(`${this.apiUrl}/api/model-scoring`)
  }

  createInterviewDetails(payload: any) {
    return this.http.post<void>(`${this.apiUrl}/api/interview-details`, payload);
  }

  deleteInterview(payload: any) {
    return this.http.delete<void>(`${this.apiUrl}/api/interview-details/${payload.id}`);
  }

  updateInterview(payload: any) {
    return this.http.put<void>(`${this.apiUrl}/api/interview-details/${payload.id}`, payload);
  }

  createInterviewAnalysis(payload: any) {
    return this.http.post<void>(`${this.apiUrl}/api/interview-detail-analysis`, payload);
  }

  deleteInterviewAnalysis(payload: any) {
    return this.http.delete<void>(`${this.apiUrl}/api/interview-detail-analysis/${payload.id}`);
  }

  updateInterviewAnalysis(payload: any) {
    return this.http.put<void>(`${this.apiUrl}/api/interview-detail-analysis/${payload.id}`, payload);
  }

  createInterviewScoring(payload: any) {
    return this.http.post<void>(`${this.apiUrl}/api/model-scoring`, payload);
  }

  deleteInterviewScoring(payload: any) {
    return this.http.delete<void>(`${this.apiUrl}/api/model-scoring/${payload.id}`);
  }

  updateInterviewScoring(payload: any) {
    return this.http.put<void>(`${this.apiUrl}/api/model-scoring/${payload.id}`, payload);
  }
  fetchCandidateInterviewDetails(id: number) {
    return this.http.get<void>(`${this.apiUrl}/candidate/${id}/interviews`)
  }
  fetchInterviewDetailById(id: string) {
    return this.http.get<void>(`${this.apiUrl}/candidate/code/${id}/details`)
  }
  getRabbitMQData(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/interview/dlq/messages`);
  }
  retryRabbitMQEntry(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/interview/dlq/messages`, [payload]);
  }
  sendInterviewEmail(payload: any): Observable<any> {
    return this.http.post<any>(`${this.emailBaseUrl}/email/send`, payload);
  }
  sendInterrviewSummaryEmail(payload: any): Observable<any> {
    return this.http.post<any>(`${this.emailBaseUrl}/email/summary`, payload);
  }
  getSkillWiseScore(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/api/interviews/${id}/skill-wise-score`);
  }
  shareInterviewDetails(payload: any): Observable<any> {
    return this.http.post(`${this.emailBaseUrl}/email/summary`, payload, { responseType: 'text' });
  }
   sendRatings(id: string, payload: SelfRating): Observable<SelfRating> {
    return this.http.post<SelfRating>(`${this.apiUrl}/api/interviews/${id}/self-rating`, payload);
  }

  fetchSignedVideoUrl(interviewCode:string){
     return this.http.get(`${this.apiUrl}/video/signed-url/${interviewCode}`, { responseType: 'text' });
  }
  fetchAllAccounts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/data/clients`);
  }

  fetchEvalutionTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/data/fitment-types`);
  }
}
