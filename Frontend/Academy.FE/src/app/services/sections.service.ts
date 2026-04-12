import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";

export interface Section {
    id: number,
    name: string,
    skillId: number,
    skillName: string
}

@Injectable({
  providedIn: "root",
})
export class SectionsService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiMockinterviewBaseURL}/sections`;
  }

 getBySkillId(skillId: number): Observable<Section[]> {
  return this.http.get<Section[]>(
    `${this.apiUrl}/skill/${skillId}`
  );
}

}
