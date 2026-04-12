import { ChangeDetectorRef, Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { MatIconModule } from "@angular/material/icon";
import { MatButtonModule } from "@angular/material/button";
import { MatTableModule } from "@angular/material/table";
import { MatDialogModule } from "@angular/material/dialog";
import { finalize } from "rxjs";
import { LoaderService } from "@services/loader.service";
import { AcademyHttpService } from "@services/academy-http.service";
import { ToastrService } from "ngx-toastr";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { ActivityFormComponent } from "../activity-form/activity-form.component";
@Component({
  selector: "app-activity-detail-dialog",
  standalone: true,
  templateUrl: "./activity-detail-dialog.component.html",
  styleUrls: ["./activity-detail-dialog.component.css"],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatDialogModule,
    ActivityFormComponent,
  ],
})
export class ActivityDetailDialogComponent {

  employeeId: number = 0;
  employeeName: string = "";

  activityList: any[] = [];

  displayedColumns: string[] = [
    "activityName",
    "activityDetail",
    "comments",
    "startDate",
    "endDate",
    "status",
    "account",
    "actions",
  ];

  activityOptions: { id: number; name: string }[] = [];
  accountDetailsOptions: { name: string }[] = [];

  showAddForm = false;
  editMode = false;
  selectedActivity: any = null;

  constructor(
    public dialogRef: MatDialogRef<ActivityDetailDialogComponent>,
    private loaderService: LoaderService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private cd: ChangeDetectorRef
  ) {
    this.employeeId = data?.id;
    this.employeeName = data.employeeName;
  }

  ngOnInit() {
    this.loadActivityDetails(this.employeeId);
    this.loadActivityOptions();
    this.loadAccountDetailsOptions();
  }

  // NEW METHOD (used by common form)
  handleFormSubmit(value: any): void {

  let payload: any;
  // DELETE FLOW
  if (value.Action === 'delete') {
     payload = {
      employeeActivityId: this.selectedActivity?.employeeActivityId,
      employeeId: this.employeeId,
      action: 'delete'
    };
  }

  else{

    const startDate = new Date(value.startDate);
    startDate.setHours(12, 0, 0, 0);

    let endDate: Date | null = null;
    if (value.endDate) {
      endDate = new Date(value.endDate);
      endDate.setHours(12, 0, 0, 0);
    }

    payload = {
      employeeActivityId: this.editMode
        ? this.selectedActivity?.employeeActivityId
        : null,
      employeeId: this.employeeId,
      activityId: value.activityId,
      activitySource: value.activitySource,
      activityDetail: value.activityDetail,
      comments: value.comments,
      startDate: startDate.toISOString(),
      endDate: endDate ? endDate.toISOString() : null,
      status: this.mapStatusToId(value.status),
      account: value.account?.join("#|"),
      action: value.action
    };
  }

    this.loaderService.start();

    this.academyHttpService
      .saveActivityDetails(payload)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res?.status === 200) {
            this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
            this.loadActivityDetails(this.employeeId);
            this.showAddForm = false;
          } else {
            this.toastr.error(res?.message || 'Error while saving activity', 'Error');
          }
        },
        error: (err) => {
          const validationErrors = err?.error?.errors;
          if (validationErrors) {
            this.toastr.error(
              'Validation error: ' + JSON.stringify(validationErrors),
              'Error'
            );
          } else {
            this.toastr.error(
              err?.error?.message || 'Unexpected error during save.',
              'Error'
            );
          }
        },
      });
  }

  cancelAdd() {
    this.showAddForm = false;
    this.editMode = false;
    this.selectedActivity = null;
  }

  editActivity(row: any) {
    this.showAddForm = true;
    this.editMode = true;
    this.selectedActivity = { ...row };
  }

  loadActivityDetails(employeeId: number) {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllActivities(employeeId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200 && Array.isArray(response.data)) {
            this.activityList = response.data.map((item: any) => ({
              employeeActivityId: item.employeeActivityId,
              activityId: item.activityId,
              activityName: item.activityName,
              activitySource: item.activitySource,
              activityDetail: item.activityDetail,
              comments: item.comments,
              startDate: item.startDate ? new Date(item.startDate) : null,
              endDate: item.endDate ? new Date(item.endDate) : null,
              status: this.mapStatusIdToText(item.statusId),
              account: item.account ? item.account.split("#|") : [],
            }));
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  loadActivityOptions(): void {
    this.loaderService.start();
    this.academyHttpService
      .getActivityMasterList()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res?.status === 200) {
            this.activityOptions = res.data.map((item: any) => ({
              id: item.activityId,
              name: item.activityName,
            }));
          } else {
            this.toastr.error("Failed to load activity list", "Error");
          }
        },
      });
  }

  loadAccountDetailsOptions(): void {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllAccount()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res.status === 200) {
            this.accountDetailsOptions = res.data.map((item: any) => ({
              name: item,
            }));
          } else {
            this.toastr.error("Failed to load account details", "Error");
          }
        },
      });
  }

  mapStatusIdToText(statusId: number): string {
    switch (statusId) {
      case 1:
        return "Pending";
      case 2:
        return "Completed";
      case 3:
        return "On going";
      default:
        return "Unknown";
    }
  }

  mapStatusToId(status: string): number {
    switch (status) {
      case "Pending":
        return 1;
      case "On going":
        return 3;
      case "Completed":
        return 2;
      default:
        return 0;
    }
  }
}
