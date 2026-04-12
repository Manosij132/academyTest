import { CommonModule } from "@angular/common";
import { Component, ElementRef, ViewChild } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatPaginatorModule } from "@angular/material/paginator";
import { MatSortModule } from "@angular/material/sort";
import { MatTableModule } from "@angular/material/table";
import { ActivatedRoute } from "@angular/router";
import { finalize } from "rxjs";
import { AcademyHttpService } from "@services/academy-http.service";
import { DataTransferService } from "@services/data-transfer.service";
import { DialogService } from "@services/dialog.service";
import { LoaderService } from "@services/loader.service";
import { AddCommentComponent } from "@shared/component/add-comment/add-comment.component";
import { ModalConfirmDialogComponent } from "@shared/component/modal-confirm-dialog/modal-confirm-dialog.component";
import { SelectionControlComponent } from "@shared/component/selection-control/selection-control.component";
import { CvProfileUploadDialogComponent } from "@components/document/cv-profile-upload-dialog/cv-profile-upload-dialog.component";
import { Overlay } from "@angular/cdk/overlay";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import {
  DialogData,
  UpdateAbilityKnowledgeDialogComponent,
} from "@shared/component/update-ability-knowledge-dialog/update-ability-knowledge-dialog.component";
import { UpdateGexLeaderComponent } from "@components/dojo-gx-leader/update-gex-leader/update-gex-leader.component";
import { ViewCommentComponent } from "@shared/component/view-comment/view-comment.component";
import { ChangeStatusRequest } from "@shared/dto/change-status-request";
import { DomSanitizer, SafeUrl } from "@angular/platform-browser";

@Component({
  selector: "app-dashboard",
  standalone: true,
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.css"],
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSortModule,
    ReactiveFormsModule,
    MatDialogModule,
    AddCommentComponent,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    SelectionControlComponent,
    UpdateGexLeaderComponent,
  ],
})
export class DashboardComponent {
  displayedColumns: string[] = [
    "trainingName",
    "mvpSkill",
    "startDate",
    "endDate",
    "status",
    "trainingScore",
    "upload",
  ];
  trainingData: any[] = [];

  $empId: number = 0;
  employeeId: number = 0;
  employee: any;
  dashboard: any;
  showCommentsPopup = false;
  color='#bfd732';
  onDojo = false;
  @ViewChild("viewCommentsModal") viewCommentsModal!: ElementRef;

  safeProfileImageUrl: SafeUrl = "";

  selectedValue: string = "";
  trainingStatusList: any[] = [
    // Use SelectionOption interface
    { id: 2, value: "Completed", viewValue: "Completed" },
    { id: 1, value: "Pending", viewValue: "Pending" },
    { id: 3, value: "Ongoing", viewValue: "Ongoing" },
  ];

  statusClassMap: { [status: string]: string } = {
    // Define the mappings
    Completed: "bg-success",
    Pending: "bg-danger",
    Ongoing: "bg-warning",
  };

  traingProgressClass = "bg-success";
  proficiencyProgressClass = "bg-success";

  constructor(
    private dialog: MatDialog,
    private readonly academyHttpService: AcademyHttpService,
    private readonly route: ActivatedRoute,
    private dataTransferService: DataTransferService,
    private loaderService: LoaderService,
    private dialogService: DialogService,
    private sanitizer: DomSanitizer,
    private overlay: Overlay
  ) {
    this.employeeId = Number(this.route.snapshot.paramMap.get("id"));
  }

  ngOnInit() {
    // this.dataTransferService.employee$.subscribe((data) => {
    //   this.employee = data;
    // });
    // this.dataTransferService.dashboard$.subscribe((data) => {
    //   this.dashboard = data;
    // });
    this.loadEmployeeData();
  }

  onImageError(event: Event): void {
    const imgElement = event.target as HTMLImageElement;
    imgElement.src = 'assets/img/user-icon.png';
  }

  onStatusUpdate(element: any, newValue: any) {
    const oldValue = element.trainingStatus;

    const dialogRef = this.dialogService.openConfirmDialog({
      component: ModalConfirmDialogComponent,
      componentProps: {
        description: "Are you sure you want to update status?",
      },
      title: "Confirm",
      panelClass: "my-custom-panel", // Add a custom panel class
    });

    dialogRef.afterClosed().subscribe((result) => {
      console.log("The dialog was closed", result);

      if (result.confirm) {
        // api call to save data
        const status = this.trainingStatusList.find(
          (x) => x.value === newValue
        );
        const request: ChangeStatusRequest = {
          EmployeeId: this.employeeId,
          EmployeeTrainingId: element.employeeTrainingMapId,
          TrainingStatusId: status.id,
        };
        element.trainingStatus = newValue;
        this.academyHttpService
          .changeTrainingStatus(request)
          .pipe(finalize(() => {}))
          .subscribe(
            (response: any) => {
              console.log(response);
            },
            (error) => {
              element.trainingStatus = oldValue;
            }
          );
      }
    });
    console.log(`status selected for ${element.name}: ${newValue}`);
  }

  loadEmployeeData() {
    this.loaderService.start();
    this.academyHttpService
      .fetchEmployeeDashboard(this.employeeId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.employee = response.data.employee;
            this.traingProgressClass = this.getProgressBarColorClass(
              this.employee.trainingCompletetionScore
            );

            this.proficiencyProgressClass = this.getProgressBarColorClass(
              this.employee.proficiencyScore
            );

            this.safeProfileImageUrl = this.sanitizer.bypassSecurityTrustUrl(
              this.employee.imageUrl
            );

            this.onDojo = response.data.employee.client.toLowerCase() === "globant";

            this.dashboard = response.data.dashboard;
            this.updateProfileData(response.data);
            this.trainingData = response.data.trainings;
            for (let x = 0; x <= this.trainingData.length - 1; x++) {
              this.trainingData[x].changedTrainingId = 0;
            }
          }
        },
      });
  }

  getProgressBarColorClass(progressValue: number): string {
    if (progressValue < 20) {
      return "bg-danger"; // Warning for less than 20
    } else if (progressValue >= 20 && progressValue < 50) {
      return "bg-warning"; // Warning for 20 to 50
    } else if (progressValue >= 50 && progressValue < 75) {
      return "bg-info"; // Info for 50 to 75
    } else {
      return "bg-success"; // Success for 75 to 100
    }
  }

  updateProfileData(data: any) {
    this.dataTransferService.updateEmployee(data.employee);
    this.dataTransferService.updateDashboard(data.dashboard);
  }

  openCommentsPopup() {
    // this.$empId = this.employeeId;
    const dialogRef = this.dialogService.openDialog({
      component: ViewCommentComponent,
      componentProps: { employeeId: this.employeeId },
      title: "Comment History",
      panelClass: "my-custom-panel", // Add a custom panel class
    });

    dialogRef.afterClosed().subscribe((result) => {
      console.log("The dialog was closed", result);
    });

    // this.dialog.open(ModalDialogComponent, {
    //   panelClass: "full-width-dialog",
    //   data: {
    //     employeeId: this.employeeId,
    //   },
    // });
  }

  closeCommentsPopup() {
    this.$empId = 0;
  }

  openUpdateDialog() {
    this.dialog.open(UpdateAbilityKnowledgeDialogComponent, {
      height: "600px",
      width: "2000px",
      data: {
        id: this.employeeId,
      } as DialogData,
    });
  }
  showDojoLeaderChangeForm = false;
  dojoGexChangeText = "Edit";
  dojoGexChange() {
    this.showDojoLeaderChangeForm = !this.showDojoLeaderChangeForm;
    this.dojoGexChangeText = !this.showDojoLeaderChangeForm ? "Edit" : "Hide";
  }
openUploadCVOrProfilePopup(employee: any) {
    this.dialog.open(CvProfileUploadDialogComponent, {
      width: "800px",
      maxWidth: "90vw",
      height: "auto",
      maxHeight: "30vw",
      data: { employee: employee },
      panelClass: "full-width-dialog",
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });
  }
  dojoGexLeaderUpdate(event: any) {
    if (event !== "") {
      this.dojoGexChange();
      this.employee.dojoGexLeaderEmail = event;
    }
  }
}
