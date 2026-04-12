import {
  AfterViewInit,
  Component,
  OnDestroy,
  OnInit,
  QueryList,
  TemplateRef,
  ViewChild,
  ViewChildren,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatTableModule, MatTableDataSource } from "@angular/material/table";
import { MatSortModule, MatSort, Sort } from "@angular/material/sort";
import {
  MatPaginatorModule,
  MatPaginator,
  PageEvent,
} from "@angular/material/paginator";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import {
  Candidate,
  CandidatesService,
} from "../../../../../services/candidates.service";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { DialogData } from "../../common-dialog/models/dialog-data.model";

import { Router } from "@angular/router";
import { ScheduleMockInterviewDialogComponent } from "../../../../../components/interview/schedule-mock-interview-dialog/schedule-mock-interview-dialog.component";
import { DataRequestOptions } from "../../../../dto/data-request-options.dto";
import { AcademyHttpService } from "../../../../../services/academy-http.service";
import { debounceTime, distinctUntilChanged, Subject } from "rxjs";
import { LoaderService } from "../../../../../services/loader.service";

@Component({
  selector: "app-candidates",
  standalone: true,
  templateUrl: "./candidates.component.html",
  styleUrl: "./candidates.component.css",
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    FormsModule,
    MatPaginatorModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatDialogModule,
  ],
})
export class CandidatesComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild("addCandidateTemplate") addCandidateTemplate!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

  form: FormGroup;
  dialogLoading = false;
  dialogError: string | null = null;

  candidates: Candidate[] = [];
  dataSource = new MatTableDataSource<Candidate>([]);
  displayedColumns: string[] = [
    "employeeId",
    "employeeName",
    "employeeEmail",
    "actions",
  ];
  error: string | null = null;
  showAddCandidate = false;
  searchText = "";
  // Pagination properties
  public totalPages = 0;
  public totalItems = 0;
  public pageSize = 20;
  public pageIndex = 0;
  color = "#eeeeee";
  request = new DataRequestOptions();

  private searchSubject = new Subject<string>();

  constructor(
    private candidatesService: CandidatesService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private router: Router,
    private readonly academyHttpService: AcademyHttpService,
    private loaderService: LoaderService
  ) {
    this.form = this.fb.group({
      name: ["", [Validators.required, Validators.maxLength(100)]],
    });
  }

  ngOnInit() {
    this.searchSubject
      .pipe(
        debounceTime(500), // Wait 500ms after user stops typing
        distinctUntilChanged() // Call only if value actually changed
      )
      .subscribe((searchTerm) => {
        this.applyFilter(searchTerm);
      });
    this.fetchCandidates();
  }

  ngAfterViewInit() {
    this.sort.sortChange.subscribe((sort: Sort) => {
      this.onSortChanged(sort);
    });
  }

  fetchCandidates() {
    this.loaderService.start();
    this.academyHttpService.fetchTrackerList(this.request).subscribe({
      next: (response: any) => {
        if (response.success) {
          const data = response.data;
          this.totalItems = data.totalCount;
          this.pageSize = data.pageSize;
          this.pageIndex = data.pageIndex;

          this.dataSource.data = data.items;
          this.loaderService.stop();
          setTimeout(() => {
            this.sort.active = this.request.SortOptions.SortBy;
            this.sort.direction = this.request.SortOptions.SortByDescending
              ? "desc"
              : "asc";

            this.dataSource.sort = this.sort;
          });
        }
      },
      error: () => {
        this.error = "Failed to load candidates";
        this.loaderService.stop();
      },
    });
  }

  onSearchChange(event: any) {
    const value = event.target.value.trim();
    this.searchSubject.next(value);
  }

  applyFilter(searchValue: string) {
    this.searchText = searchValue;

    // Call your API or filtering logic
    this.request.PagingOptions.PageIndex = 0;
    this.request.SearchText = searchValue;
    this.fetchCandidates();
  }

  onPageChanged(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.request.PagingOptions.PageIndex = this.pageIndex;
    this.request.PagingOptions.PageSize = this.pageSize;
    this.fetchCandidates();
  }

  onSortChanged(sort: Sort) {
    this.pageIndex = 0;
    this.request.PagingOptions.PageIndex = 0;

    if (!sort.active || sort.direction === "") {
      this.request.SortOptions = {
        SortBy: "",
        SortByDescending: false,
      };
    } else {
      this.request.SortOptions = {
        SortBy: sort.active,
        SortByDescending: sort.direction === "desc",
      };
    }
    this.fetchCandidates();
  }

  onCreateCandidate() {
    this.showAddCandidate = true;
  }

  openMockInterviewDialog(
    candidateId: number,
    candidateName: string,
    candidateEmail: string
  ): void {
    const dialogRef = this.dialog.open(ScheduleMockInterviewDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      height: 'auto',
      panelClass: 'custom-dialog',
      data: {
        employeeId: candidateId,
        name: candidateName,
        email: candidateEmail,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        // Handle any post-dialog actions if needed
      }
    });
  }

  view(candidateId: number): void {
    this.router.navigate(["/mock-interview-details", candidateId]);
  }

  onBackToList() {
    this.showAddCandidate = false;
    this.fetchCandidates();
  }

  createCandidate(): void {
    this.form.reset();
    this.dialogError = null;
    this.dialogLoading = false;

    const dialogData: DialogData = {
      title: "Add Candidate",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addCandidateTemplate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "600px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.onSubmitDialog();
      }
      this.onCancelDialog();
    });
  }

  onSubmitDialog(): void {
    if (this.form.invalid) return;

    this.dialogLoading = true;
    this.dialogError = null;

    this.candidatesService.add(this.form.value).subscribe({
      next: () => {
        this.dialogLoading = false;
        this.dialog.closeAll();
        this.fetchCandidates();
      },
      error: () => {
        this.dialogError = "Failed to add candidate";
        this.dialogLoading = false;
      },
    });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
  }

  ngOnDestroy() {
    this.searchSubject.unsubscribe();
  }
}
