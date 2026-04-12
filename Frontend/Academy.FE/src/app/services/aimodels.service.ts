import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from "../../environments/environment";

export interface AIModel {
  id?: number;
  modelName: string;
  version: string;
  usage?: string[];
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AIModelsService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}/aimodels`
  }

  getAll(): Observable<AIModel[]> {
    return this.http.get<AIModel[]>(this.apiUrl);
  }

  getById(id: number): Observable<AIModel> {
    return this.http.get<AIModel>(`${this.apiUrl}/${id}`);
  }

  create(aiModel: AIModel): Observable<AIModel> {
    return this.http.post<AIModel>(this.apiUrl, aiModel);
  }

  update(id: number, aiModel: AIModel): Observable<AIModel> {
    return this.http.put<AIModel>(`${this.apiUrl}/${id}`, aiModel);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
