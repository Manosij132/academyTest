import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from "../../environments/environment";

@Injectable({ providedIn: 'root' })
export class QuestionsService {
  private baseUrl: string;

  constructor(private http: HttpClient) {
    this.baseUrl = `${environment.apiMockinterviewBaseURL}/questions`
  }

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl);
  }

  get(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  create(question: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, question);
  }

  update(id: number, question: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, question);
  }

  delete(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }

  getDistinctSections(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/sections/distinct`);
  }
}
