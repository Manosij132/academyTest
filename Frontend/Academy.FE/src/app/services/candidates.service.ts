import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Candidate {
  id: number;
  name: string;
  createdAt?: string;
  updatedAt?: string;
  email:string
}

@Injectable({
  providedIn: 'root'
})
export class CandidatesService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}/candidates`
  }

  getAll(): Observable<Candidate[]> {
    return this.http.get<Candidate[]>(this.apiUrl);
  }

  getById(id: number): Observable<Candidate> {
    return this.http.get<Candidate>(`${this.apiUrl}/${id}`);
  }

  add(candidate: Partial<Candidate>): Observable<Candidate> {
    return this.http.post<Candidate>(this.apiUrl, candidate);
  }

  update(id: number, candidate: Partial<Candidate>): Observable<Candidate> {
    return this.http.put<Candidate>(`${this.apiUrl}/${id}`, candidate);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
