import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Skills {
  id?: number;
  name: string;
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SkillsServiceService {

  private apiUrl :string;
  private baseUrl :string;

  constructor(private http: HttpClient) {
   this.baseUrl = `${environment.apiMockinterviewBaseURL}`
   this.apiUrl = `${environment.apiMockinterviewBaseURL}/skills`
  }

  getAll(): Observable<Skills[]> {
    return this.http.get<Skills[]>(this.apiUrl);
  }

  getById(id: number): Observable<Skills> {
    return this.http.get<Skills>(`${this.apiUrl}/${id}`);
  }

  create(skill: Skills): Observable<Skills> {
    return this.http.post<Skills>(this.apiUrl, skill);
  }

  update(id: number, skill: Skills): Observable<Skills> {
    return this.http.put<Skills>(`${this.apiUrl}/${id}`, skill);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
