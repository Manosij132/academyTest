import { CommonModule, Location } from "@angular/common";
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
import { ActivatedRoute, Router } from "@angular/router";
import { finalize } from "rxjs";
import { AcademyHttpService } from "@services/academy-http.service";
import { DataTransferService } from "@services/data-transfer.service";
import { DialogService } from "@services/dialog.service";
import { LoaderService } from "@services/loader.service";
import { AddCommentComponent } from "../../../shared/component/add-comment/add-comment.component";
import { ConfirmDialogComponent } from "../../../shared/component/confirm-dialog/confirm-dialog.component";
import { SelectionControlComponent } from "../../../shared/component/selection-control/selection-control.component";
import { UpdateAbilityKnowledgeDialogComponent } from "../../../shared/component/update-ability-knowledge-dialog/update-ability-knowledge-dialog.component";
import { ViewCommentComponent } from "../../../shared/component/view-comment/view-comment.component";
import { ModalConfirmDialogComponent } from "../../../shared/component/modal-confirm-dialog/modal-confirm-dialog.component";
import { ChangeStatusRequest } from "../../../shared/dto/change-status-request";
import { MockInterviewServiceService, UserInterviewStatisticsDTO } from "@services/mock-interview-service.service";
import { InterviewData } from "../../../shared/Interface/mock-interview";
import { TableComponent } from "../../../shared/component/table/table.component";
import { DomSanitizer, SafeUrl } from "@angular/platform-browser";
import { CandidateInterviewsComponent } from "../../../shared/component/mock-interview-new/candidate-interviews/candidate-interviews.component";
@Component({
  selector: 'app-mock-interview-details',
  standalone: true,
  imports: [CommonModule,
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
    TableComponent,
    CandidateInterviewsComponent,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    SelectionControlComponent],
  templateUrl: './mock-interview-details.component.html',
  styleUrl: './mock-interview-details.component.css'
})

export class MockInterviewDetailsComponent {
  displayedColumns: string[] = [
    "trainingName",
    "mvpSkill",
    "startDate",
    "endDate",
    "status",
  ];
  trainingData: any[] = [];
  interviewStats: UserInterviewStatisticsDTO | null = null;
  candidate:any;
  $empId: number = 0;
  employeeId: number = 0;
  employee: any;
  dashboard: any;
  showCommentsPopup = false;
  isProfileOpen = false;
  color = "#eeeeee";
  showBackButton = true;

  @ViewChild("viewCommentsModal") viewCommentsModal!: ElementRef;
  safeProfileImageUrl: SafeUrl = "";
  
  constructor(
    private dialog: MatDialog,
    private readonly academyHttpService: AcademyHttpService,
    private readonly route: ActivatedRoute,
    private dataTransferService: DataTransferService,
    private loaderService: LoaderService,
    private dialogService: DialogService,
    private mockInterviewService: MockInterviewServiceService ,
    private router:Router,
    private sanitizer:DomSanitizer,
    private location: Location

  ) {
    this.employeeId = Number(this.route.snapshot.paramMap.get("id"));
  }
  onChildEvent(data: any) {
  }
  tableData!: InterviewData[];
  data: any;
  interviewData: any;
  interviewId='';
  
  

  ngOnInit() {
    this.loadEmployeeData();
  }

  goBack(): void {
    this.location.back();
  }

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
            },
            (error) => {
              element.trainingStatus = oldValue;
            }
          );
      }
    });
    console.log(`status selected for ${element.name}: ${newValue}`);
  }
  onImageError(event: Event): void {
    const imgElement = event.target as HTMLImageElement;
    imgElement.src = 'assets/img/user-icon.png';
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
            this.safeProfileImageUrl = this.sanitizer.bypassSecurityTrustUrl(this.employee.imageUrl);
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
      // width: '800px',
      height: "600px",
      width: "2000px",
    });
  }
  getRoutes() {
    let routes = new Map<string, string>();
    routes.set("skillSet", `/interview-details/${this.interviewId}`);
    return routes
  }
  onRowClick(index: any) {
    this.interviewId=this.interviewData[index].interviewId;
    this.router.navigate([`/interview-details/${this.interviewId}`], { state: { selectedIndex: index } });
  }
  toggleProfile() {
    this.isProfileOpen = !this.isProfileOpen;
  }
}
