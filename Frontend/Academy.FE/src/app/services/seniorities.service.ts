import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Seniority {
  id?: number;
  name: string;
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SenioritiesService {
  private apiUrl :string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}/seniorities`
  }

  // constructor(private http: HttpClient) { }

  getAll(): Observable<Seniority[]> {
    return this.http.get<Seniority[]>(this.apiUrl);
  }

  getById(id: number): Observable<Seniority> {
    return this.http.get<Seniority>(`${this.apiUrl}/${id}`);
  }

  create(seniority: Seniority): Observable<Seniority> {
    return this.http.post<Seniority>(this.apiUrl, seniority);
  }

  update(id: number, seniority: Seniority): Observable<Seniority> {
    return this.http.put<Seniority>(`${this.apiUrl}/${id}`, seniority);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
