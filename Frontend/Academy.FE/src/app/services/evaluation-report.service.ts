import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from "@environments/environment";

@Injectable({
  providedIn: 'root'
})
export class EvaluataionReportService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
     this.apiUrl = `${environment.apiMockinterviewBaseURL}`
  }

  downloadReport(): Observable<any> {
    const url = `${this.apiUrl}/reports/interviews/export`;

    return this.http.get(url,{
      responseType: 'blob' as 'json', 
      observe: 'response',
      headers: new HttpHeaders({
        'accept': '*/*'
      })
    });
  }
}