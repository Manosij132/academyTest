import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";

export interface FitmentType {
  id: number;
  name: string;
}

export interface Client {
  id: number;
  name?: string;
}

@Injectable({
  providedIn: "root",
})
export class DataService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}/data`;
  }

  getAllFitmentTypes(): Observable<FitmentType[]> {
    return this.http.get<FitmentType[]>(`${this.apiUrl}/fitment-types`);
  }

  getAllClients(): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.apiUrl}/clients`);
  }
}
