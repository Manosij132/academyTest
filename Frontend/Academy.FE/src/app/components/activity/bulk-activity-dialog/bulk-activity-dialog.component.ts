import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogContent } from "@angular/material/dialog";
import { CommonModule } from "@angular/common";
import { finalize } from "rxjs/operators";
import { LoaderService } from "@services/loader.service";
import { AcademyHttpService } from "@services/academy-http.service";
import { ToastrService } from "ngx-toastr";
import { ActivityFormComponent } from "../activity-form/activity-form.component";

@Component({
  selector: "app-bulk-activity-dialog",
  standalone: true,
  imports: [
    CommonModule,
    ActivityFormComponent,
    MatDialogContent
  ],
  templateUrl: "./bulk-activity-dialog.component.html",
  styleUrls: ["./bulk-activity-dialog.component.css"],
})
export class BulkActivityDialogComponent {
  employeeId: number = 0;

  activityOptions: { id: number; name: string }[] = [];
  accountDetailsOptions: { name: string }[] = [];

  constructor(
    public dialogRef: MatDialogRef<BulkActivityDialogComponent>,
    private loaderService: LoaderService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private readonly academyHttpService: AcademyHttpService,
    private toastr: ToastrService
  ) {
    this.employeeId = data?.id;
  }

  ngOnInit() {
    this.loadActivityOptions();
    this.loadAccountDetailsOptions();
  }

  // HANDLE BULK SUBMIT
  handleBulkSubmit(value: any): void {
    const startDate = new Date(value.startDate);
    startDate.setHours(12, 0, 0, 0);

    let endDate: Date | null = null;
    if (value.endDate) {
      endDate = new Date(value.endDate);
      endDate.setHours(12, 0, 0, 0);
    }

    // Construct the payload for the API
    const payload = {
      activityId: value.activityId,
      activitySource: value.activitySource,
      activityDetail: value.activityDetail,
      comments: value.comments,
      startDate: startDate.toISOString(),
      endDate: endDate ? endDate.toISOString() : null,
      account: value.account?.join("#|"),
    };

    // Close the dialog and pass the value back to the parent component
    this.dialogRef.close(payload);
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
            this.toastr.error("Failed to load activity list");
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
            this.toastr.error("Failed to load account details");
          }
        },
      });
  }

  onCloseClick() {
    this.dialogRef.close();
  }
}