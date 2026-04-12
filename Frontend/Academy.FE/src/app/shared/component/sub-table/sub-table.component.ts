import { CommonModule } from "@angular/common";
import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  inject,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import { MatBadgeModule } from "@angular/material/badge";
import { MatCardModule } from "@angular/material/card";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatTableModule } from "@angular/material/table";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { DataTransferService } from "../../../services/data-transfer.service";
import { LoaderService } from "../../../services/loader.service";
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  CompletedTrainingStatus,
  TOASTER_MESSAGES,
  TrainingStatus,
} from "../../../shared/constants/app.constants";
import { ChangeStatusRequest } from "../../dto/change-status-request";
import { DialogData, UpdateAbilityKnowledgeDialogComponent } from "../update-ability-knowledge-dialog/update-ability-knowledge-dialog.component";

@Component({
  selector: "app-sub-table",
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    MatIconModule,
    MatTableModule,
    MatInputModule,
    MatBadgeModule,
    MatCardModule,
    MatProgressBarModule,
    MatDialogModule,
    MatTooltipModule,
    
  ],
  templateUrl: "./sub-table.component.html",
  styleUrl: "./sub-table.component.scss",
})
export class SubTableComponent implements OnChanges {
  footerButton = [];
  @Input() canChangeStatus: boolean = false;
  @Input() id: number = 0;
  @Input() show: boolean = false;
  public trainingStatus = TrainingStatus;
  public readonly completedTrainingStatus = TrainingStatus.find(
    (status) => status.Key === CompletedTrainingStatus
  );
  employee: any;

  readonly dialog = inject(MatDialog);

  displayedColumns: string[] = [
    "trainingUrl",
    "skillName",
    "startDate",
    "expectedEndDate",
    "status",
    "trainingScore",
  ];

  public header = [
    {
      colName: "Traning Name",
    },
    {
      colName: "MVP Skill",
    },
    {
      colName: "Start Date",
    },
    {
      colName: "End Date",
    },
    {
      colName: "Status",
    },
  ];

  columnTitles: { [key: string]: string } = {    
    trainingUrl: "Traning Name",
    skillName: "MVP Skill",
    startDate: "Start Date",
    expectedEndDate: "End Date",
    status: "Status",
    trainingScore:"Training Percent",
  };

  public rows = [];

  columns = [
    { id: "trainingUrl", label: "Training Name" },
    { id: "skillName", label: "Skill Name" },
    { id: "startDate", label: "Start Date", pipe: "date:'dd-MMM-yyyy'" },
    {
      id: "expectedEndDate",
      label: "Expected End Date",
      pipe: "date:'dd-MMM-yyyy'",
    },
    { id: "status", label: "Status" },
    { id: "trainingScore", label: "Training Percent" },
  ];

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private dataTransferService: DataTransferService,
    private toastr: ToastrService,
    private loaderService: LoaderService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.show) {
      this.loadEmployeeData();
    }
  }

  ngOnInit() {}

  updateProfileData(data: any) {
    this.dataTransferService.updateEmployee(data.employee);
    this.dataTransferService.updateDashboard(data.dashboard);
  }

  private loadEmployeeData() {
    this.loaderService.start();
    this.academyHttpService
      .fetchEmployeeDashboard(this.id)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.employee = response.data.employee;
            // console.log(this.employee);
            this.updateProfileData(response.data);
            this.rows = response.data.trainings;
            // for (let x = 0; x <= this.rows.length - 1; x++) {
            //   this.rows[x]["changedTrainingId"] = 0
            // }
          }
        },
      });
  }

  openProficiencyPopup() {
    this.dialog.open(UpdateAbilityKnowledgeDialogComponent, {
      // width: '800px',
      height: "600px",
      width: "2000px",
      data: {
              id: this.id,
            } as DialogData,
    });
  }

  onStatusChanged(row: any) {
    let request = new ChangeStatusRequest();
    if (row.changedTrainingId == 0) return;
    if (row.trainingStatusId != row.changedTrainingId) {
      request.EmployeeId = this.id;
      request.EmployeeTrainingId = row.employeeTrainingMapId;
      request.TrainingStatusId = row.changedTrainingId;
      this.loaderService.start();
      this.academyHttpService
        .changeTrainingStatus(request)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (response: any) => {
            if (response.status === 200) {
              this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
              this.loadEmployeeData();
            } else {
              this.toastr.error(response.errorMessage, "Error");
            }
          },
        });
    }
  }

  setChangedStatus(event: any, row: any) {
    row.changedTrainingId = event.target.value;
  }
}
