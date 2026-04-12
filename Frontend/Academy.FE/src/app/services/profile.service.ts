import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Seniority {
  id: number;
  name?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface Skill {
  id: number;
  name?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface ProfileRequest {
  fitmentType: number;
  position: string;
  seniority: number;
  primarySkillId: number;
  skillsAndSections: { [skillId: string]: number[] };
  profileName: string;
  clientId: string;
}

 export interface Profile {
  profileId?: number;
  profileName: string;
  fitmentTypeId: number;
  fitmentTypeName?: string;
  clientId: number;
  clientName?: string;
  position: string;
  seniorityId: number;
  seniorityName?: string;
  primarySkillId: number;
  primarySkillName?: string;
  skillsAndSections: {
  skillId: number;
  skillName?: string;
  sections: {
    id: number;
    name: string;
    skillId?: number;
    skillName?: string;
  }[]
}[];
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  private apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}/profiles/v1`
  }

  getAll(): Observable<Profile[]> {
    return this.http.get<Profile[]>(this.apiUrl);
  }

  getById(id: number): Observable<Profile> {
    return this.http.get<Profile>(`${this.apiUrl}/${id}`);
  }

  create(profile: ProfileRequest): Observable<ProfileRequest> {
    return this.http.post<ProfileRequest>(this.apiUrl, profile);
  }

  update(id: number, profile: ProfileRequest): Observable<ProfileRequest> {
    return this.http.put<ProfileRequest>(`${this.apiUrl}/${id}`, profile);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
