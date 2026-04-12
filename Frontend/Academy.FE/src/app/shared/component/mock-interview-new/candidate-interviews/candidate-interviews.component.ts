import { CommonModule, JsonPipe } from "@angular/common";
import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  QueryList,
  TemplateRef,
  ViewChild,
  ViewChildren,
} from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatPaginatorModule, MatPaginator } from "@angular/material/paginator";
import { MatSortModule, MatSort } from "@angular/material/sort";
import { MatTableModule, MatTableDataSource } from "@angular/material/table";
import { CandidateInterviewDetailsComponent } from "../candidate-interview-details/candidate-interview-details.component";
import { InterviewsService } from "../../../../services/interviews.service";
import { ActivatedRoute, Router } from "@angular/router";
import { environment } from "../../../../../environments/environment";
import { CommonDialogComponent } from "../common-dialog/common-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { DialogData } from "../common-dialog/models/dialog-data.model";
import {
  FormArray,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatError, MatFormField, MatLabel } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { ViewVideoComponent } from "../../../../components/interview/view-video/view-video.component";
import { MatSnackBar } from "@angular/material/snack-bar";
import { LoaderService } from "../../../../services/loader.service";
import { finalize } from "rxjs";

@Component({
  selector: "app-candidate-interviews",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormField,
    MatLabel,
    MatError,
    MatIconModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    JsonPipe,
    CandidateInterviewDetailsComponent,
    ReactiveFormsModule,
    FormsModule,
  ],
  templateUrl: "./candidate-interviews.component.html",
  styleUrl: "./candidate-interviews.component.css",
})
export class CandidateInterviewsComponent implements OnInit {
  @Input() candidate: any;
  @Output() hideBackButton = new EventEmitter<void>();
  @Output() showBackButton = new EventEmitter<void>();
  @ViewChild("addEmailTemplate") addEmailTemplate!: TemplateRef<any>;
  interviewDetails: any[] = [];
  loading = false;
  error: string | null = null;
  addInterviewDetails: boolean = false;
  selectedInterviewDetails: any | null = null;
  interviewToBeDeleted: any | null = null;
  form: FormGroup;
  selectedInterview: any;
  employeeId: any;

  displayedColumns: string[] = [
    "id",
    "profile",
    "assignedBy",
    "status",
    "actions",
  ];
  dataSource = new MatTableDataSource<any>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

  constructor(
    private interviewService: InterviewsService,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    public loaderService: LoaderService
  ) {
    this.employeeId = Number(this.route.snapshot.paramMap.get("id"));
    this.form = new FormGroup({
      email: new FormArray([
        new FormControl("", [Validators.required, Validators.email]),
      ]),
    });
  }
  get Emails() {
    return this.form.get("email") as FormArray;
  }
  addEmail() {
    this.Emails.push(
      new FormControl("", [Validators.required, Validators.email])
    );
  }

  ngOnInit(): void {
    this.fetchCandidateInterviewDetails(this.employeeId);
  }

  ngAfterViewInit(): void {
    this.paginatorList.changes.subscribe((paginators) => {
      if (paginators.first) {
        this.dataSource.paginator = paginators.first;
      }
    });
  }

  ngAfterViewChecked() {
    if (this.sort && this.dataSource.sort !== this.sort) {
      this.dataSource.sort = this.sort;
    }
    if (this.paginator && this.dataSource.paginator !== this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }

  public fetchCandidateInterviewDetails(id: number) {
    // Start global/page loader before making API call
    this.loaderService.start();

    this.interviewService
      .fetchCandidateInterviewDetails(id)
      .pipe(
        // Ensure loader is stopped whether API succeeds or fails
        finalize(() => this.loaderService.stop())
      )
      .subscribe({
        next: (details: any) => {
          // If API returns data, flatten interview object into parent object
          // Otherwise, initialize as empty array
          const interviewDetails = details?.length
            ? structuredClone(
              details.map((detail: any) => ({
                ...detail,
                ...detail?.interview
              }))
            )
            : [];

          // Assign processed interview details to component state
          this.interviewDetails = interviewDetails;
          // Bind data to table data source
          this.dataSource.data = this.interviewDetails;
        },

        error: (err) => {
          // Handle "No Data Found" scenario (valid business case)
          if (err?.status === 404) {
            // Reset data source to show empty state in UI
            this.interviewDetails = [];
            this.dataSource.data = [];
          } else {
            // Handle unexpected or server-side errors
            // (Optional: show toast/snackbar or error banner)
          }
        }
      });
  }

  public onCreateInterviewDetails() {
    this.addInterviewDetails = true;
  }

  public backToList(callApi: boolean) {
    callApi && this.fetchCandidateInterviewDetails(this.candidate?.id);
    this.addInterviewDetails = false;
    this.selectedInterviewDetails = null;
  }

  public editInterviewDetails(interviewDetail: any) {
    this.selectedInterviewDetails = structuredClone(interviewDetail);
    this.addInterviewDetails = true;
  }

  public onDeleteInterviewDetails() {
    this.interviewService
      .deleteInterview(this.interviewToBeDeleted)
      .subscribe((details: any) => {
        this.interviewToBeDeleted = null;
        this.fetchCandidateInterviewDetails(this.candidate?.id);
      });
  }

  public viewInterview(row: any) {
    this.hideBackButton.emit();
    this.selectedInterview = structuredClone(row);
  }

  viewSummary(row: any) {
    this.router.navigate(['view-interview', row.interviewCode]);
  }
  goBack() {
    this.showBackButton.emit();
    this.selectedInterview = null;
  }

  public emailToCandidate(row: any) {
    this.form.reset();
    this.form.setControl(
      "email",
      new FormArray([
        new FormControl("", [Validators.required, Validators.email]),
      ])
    );

    const dialogData: DialogData = {
      title: "Forward Interview Invite",
      message: "",
      confirmText: "Send",
      cancelText: "Cancel",
      showActions: true,
      template: this.addEmailTemplate,
      form: this.form,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "600px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        const emails = this.form.value.email.filter((e: string) => e); // clean up empty emails

        if (emails.length === 0) {
          console.warn("No valid email addresses entered.");
          return;
        }

        const payload = {
          toEmail: emails.join(","),
          name: this.candidate?.employeeName,
          skills: row.skills,
          dateTime: new Date().toLocaleString(),
          interviewUrl: `${environment.academyBaseUrl}interview/${row.interviewCode}`,
          evaluationType: row?.profile?.fitmentTypeName,
          evaluationId: row?.profile?.fitmentTypeId

        };
        this.loaderService.start();
        this.interviewService.sendInterviewEmail(payload).subscribe({
          next: (res) => {
            this.loaderService.stop();
          },
          error: (err) => {
            console.error("Error sending email:", err);
            this.loaderService.stop();
          },
        });
      }
    });
  }

  public emailSummary(row: any) {
    this.form.reset();
    this.form.setControl(
      "email",
      new FormArray([
        new FormControl("", [Validators.required, Validators.email]),
      ])
    );

    const dialogData: DialogData = {
      title: "Forward evaluation Summary",
      message: "",
      confirmText: "Send",
      cancelText: "Cancel",
      showActions: true,
      template: this.addEmailTemplate,
      form: this.form,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "600px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        const emails = this.form.value.email.filter((e: string) => e); // clean up empty emails

        if (emails.length === 0) {
          console.warn("No valid email addresses entered.");
          return;
        }

        const payload = {
          toEmail: emails.join(","),
          name: row.name,
          skills: row.skills,
          comments: row.comments,
          score: row.score,
          outof: row.outof,
          videoLink:
            environment.academyBaseUrl + "view-interview/" + row?.interviewCode,
        };
        this.loaderService.start();
        this.interviewService.sendInterrviewSummaryEmail(payload).subscribe({
          next: (res) => this.loaderService.stop(),
          error: (err) => {
            console.error("Error sending email:", err);
            this.loaderService.stop();
          },
        });
      }
    });
  }
  removeEmail(index: number) {
    this.Emails.removeAt(index);
  }

  viewVideo(row: any) {
    if (row?.interview?.interviewLink) {
      const url = row?.interview?.interviewLink.replace("view", "preview");
      const dialogRef = this.dialog.open(ViewVideoComponent, {
        width: "50%",
        height: "60%",
        disableClose: true,
        data: { url: url },
      });
    } else {
      this.snackBar.open("Video Not Available", "Close", {
        duration: 3000,
        panelClass: ["error-snackbar"],
      });
    }
  }

  capitalizeFirst(value: string | null | undefined): string {
    if (!value || typeof value !== "string") return "";
    return value.charAt(0).toUpperCase() + value.slice(1).toLowerCase();
  }
}
