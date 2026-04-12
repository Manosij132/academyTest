// user-dashboard-data.resolver.ts
import { Injectable } from "@angular/core";
import { Resolve } from "@angular/router";
import { Observable, forkJoin, of } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { AcademyHttpService } from "@services/academy-http.service";
import { ToastrService } from "ngx-toastr";

@Injectable({
  providedIn: "root",
})
export class BookmarkFilterDataResolver implements Resolve<any> {
  activitytype: string = 'Training';
  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService
  ) { }

  resolve(): Observable<any> {
    return forkJoin({
      AllReportTypes: this.academyHttpService.fetchAllReportTypes().pipe(map((response: any) => response.data)),
      AllTdc: this.academyHttpService.fetchAllTdc().pipe(map((response: any) => response.data)),
      AllCommunitySettings: this.academyHttpService.fetchAllCommunity().pipe(map((response: any) => response.data)),
      Seniorities: this.academyHttpService.fetchSeniorities().pipe(map((response: any) => response.data)),
      AllTrainings: this.academyHttpService.fetchAllTrainings().pipe(map((response: any) => response.data)),
      AllProjects: this.academyHttpService.fetchAllProjects([]).pipe(map((response: any) => response.data)),
      AllTrainingStatus: this.academyHttpService.fetchAllTrainingStatus().pipe(map((response: any) => response.data)),
      AllSelectColumns: this.academyHttpService.fetchAllSelectColumns(this.activitytype).pipe(map((response: any) => response.data)),
      AllGroupByColumns: this.academyHttpService.fetchAllGroupByColumns(this.activitytype).pipe(map((response: any) => response.data)),
      AllAreaPaths: this.academyHttpService.fetchAllAreaPaths().pipe(map((response: any) => response.data)),
      AllActivitiesType: this.academyHttpService.fetchAllActivityType().pipe(map((response: any) => response.data)),
      AllClients: this.academyHttpService.fetchAllClients().pipe(map((response: any) => response.data)),
    }).pipe(
      catchError((error) => {
        console.error("Error fetching data: ", error);
        this.toastr.error(
          "Failed to load dropdown values. Please try again later.",
          "Error",
          {
            timeOut: 3000,
            closeButton: true,
          }
        );
        return of({});
      })
    );
  }
}
