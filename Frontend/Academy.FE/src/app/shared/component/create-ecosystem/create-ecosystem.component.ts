import { Component, ElementRef, Input, ViewChild } from "@angular/core";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { ToastrService } from "ngx-toastr";
import { CommonModule } from "@angular/common";
import { CreateEcosystemRequest } from "../../dto/create-ecosystem";
import { LoaderService } from "../../../services/loader.service";
import { MatSelectModule } from "@angular/material/select";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatButtonModule } from "@angular/material/button";
import { finalize } from "rxjs";
import { TOASTER_MESSAGES } from "../../constants/app.constants";

@Component({
  selector: "app-create-ecosystem",
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: "./create-ecosystem.component.html",
  styleUrl: "./create-ecosystem.component.scss",
})
export class CreateEcosystemComponent {
  ecosystems: any[] = [];
  request = new CreateEcosystemRequest();
  @ViewChild("secondaryEcosystemName") secondaryEcosystemname!: ElementRef;
  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService
  ) {}
  ngOnInit() {
    this.loadEcosystems();
  }
  loadEcosystems() {
    this.loaderService.start();
    this.academyHttpService
      .fetchPrimaryEcosystems()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.ecosystems = response.data;
            this.ecosystems = this.ecosystems.filter((x) => x.isPrimary);
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  setPrimaryEcosystem(event: any) {
    this.request.PrimaryEcosystemId = event.value;
  }

  insertEcosystemDetails(event: any) {
    event.preventDefault();
    this.request.Name = this.secondaryEcosystemname.nativeElement.value;
    this.loaderService.start();
    console.log(this.request);
    this.academyHttpService
      .insertSecondaryEcosystem(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
            this.secondaryEcosystemname.nativeElement.value = "";
            this.request = new CreateEcosystemRequest();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
}
