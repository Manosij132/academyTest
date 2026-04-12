import { Component, inject } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { AcademyHttpService } from "../../services/academy-http.service";
import { LoaderService } from "../../services/loader.service";
import { ExportReportMetadata } from "../../shared/dto/ExportReportMetadata";
import { finalize } from "rxjs";

@Component({
  selector: "app-report",
  standalone: true,
  imports: [],
  templateUrl: "./report.component.html",
  styleUrl: "./report.component.scss",
})
export class ReportComponent {
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private loaderService: LoaderService,
    private readonly toastr: ToastrService,
    private readonly router: Router
  ) {}
  request = new ExportReportMetadata();
  ngOnInit() {}

  exportFullReport() {
    this.request = new ExportReportMetadata();
    this.request.Type = "FullReport";
    this.loaderService.start();
    this.academyHttpService
      .requestReport(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          console.clear();
          console.log(response);
          if (response.status === 200) {
            this.router.navigate(["/track/worker-request", response.data]);
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
}
