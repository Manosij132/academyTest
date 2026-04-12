import {
  Component,
  OnInit,
  AfterViewInit,
  TemplateRef,
  ViewChild,
  ViewChildren,
  QueryList,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialog } from "@angular/material/dialog";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatTableModule, MatTableDataSource } from "@angular/material/table";
import { MatSortModule, MatSort } from "@angular/material/sort";
import {
  MatPaginatorModule,
  MatPaginator,
  PageEvent,
} from "@angular/material/paginator";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import {
  SenioritiesService,
  Seniority,
} from "../../../../../services/seniorities.service";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { DialogData } from "../../common-dialog/models/dialog-data.model";
import { LoaderService } from "../../../../../services/loader.service";
import { ToastrService } from "ngx-toastr";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: "app-seniorities",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    FormsModule,
    MatPaginatorModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./seniorities.component.html",
  styleUrl: "./seniorities.component.css",
})
export class SenioritiesComponent implements OnInit, AfterViewInit {
  @ViewChild("addSeniorityTemplate") addSeniorityTemplate!: TemplateRef<any>;
  @ViewChild("editSeniorityTemplate") editSeniorityTemplate!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

  form: FormGroup;
  editForm: FormGroup;
  dialogError: string | null = null;

  seniorities: Seniority[] = [];
  dataSource = new MatTableDataSource<Seniority>([]);
  displayedColumns: string[] = [
    "id",
    "name",
    "createdAt",
    "updatedAt",
    "actions",
  ];
  loading = false;
  error: string | null = null;

  // Pagination properties
  pageSize = 5;
  totalItems = 0;
  showAddSeniority = false;
  showEditSeniority = false;
  showDeleteSeniority = false;
  seniorityToEdit: Seniority | null = null;
  seniorityToDelete: Seniority | null = null;
  searchText = "";
  constructor(
    private senioritiesService: SenioritiesService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private loaderService: LoaderService,
    private toastr: ToastrService
  ) {
    this.form = this.fb.group({
      name: ["", [Validators.required, Validators.maxLength(100)]],
    });
    this.editForm = this.fb.group({
      name: ["", [Validators.required, Validators.maxLength(100)]],
    });
  }

  ngOnInit() {
    this.fetchSeniorities();
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

  fetchSeniorities() {
    this.loaderService.start();
    this.senioritiesService.getAll().subscribe({
      next: (data) => {
        this.seniorities = data;
        this.dataSource.data = data;

        // Connect paginator and sort after data is loaded
        if (this.paginator) {
          this.dataSource.paginator = this.paginator;
        }
        if (this.sort) {
          this.dataSource.sort = this.sort;
        }

        this.totalItems = data.length;
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load seniorities";
        this.loaderService.stop();
      },
    });
  }

  applyFilter() {
    this.dataSource.filter = this.searchText.trim().toLowerCase();
    this.totalItems = this.dataSource.filteredData.length;

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  onPageChanged(event: PageEvent) {
    this.pageSize = event.pageSize;
    // Handle pagination logic here if needed
  }

  onCreateSeniority() {
    this.form.reset();
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Add Seniority",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addSeniorityTemplate,
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

    this.loaderService.start();
    this.dialogError = null;

    this.senioritiesService.create(this.form.value).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchSeniorities();
      },
      error: () => {
        this.dialogError = "Failed to add seniority";
        this.loaderService.stop();
      },
    });
  }

  onSubmitEditDialog(): void {
    if (
      this.editForm.invalid ||
      !this.seniorityToEdit ||
      !this.seniorityToEdit.id
    )
      return;

    this.loaderService.start();
    this.dialogError = null;

    this.senioritiesService
      .update(this.seniorityToEdit.id, this.editForm.value)
      .subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
          this.loaderService.stop();
          this.dialog.closeAll();
          this.fetchSeniorities();
        },
        error: () => {
          this.dialogError = "Failed to update seniority";
          this.loaderService.stop();
        },
      });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
  }

  onEditSeniority(seniority: Seniority) {
    this.seniorityToEdit = seniority;
    this.editForm.reset();
    this.editForm.patchValue({ name: seniority.name });
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit Seniority",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editSeniorityTemplate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "600px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.onSubmitEditDialog();
      }
      this.onCancelDialog();
    });
  }

  onDeleteSeniority(seniority: Seniority) {
    const dialogData: DialogData = {
      title: "Delete Seniority",
      message: `Are you sure you want to delete the seniority "<strong>${seniority.name}</strong>"? This action cannot be undone.`,
      confirmText: "Delete",
      cancelText: "Cancel",
      confirmButtonColor: "warn",
      showActions: true,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "500px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loading = true;
        this.senioritiesService.delete(seniority.id!).subscribe({
          next: (res: any) => {
            this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
            this.fetchSeniorities();
            this.loading = false;
          },
          error: () => {
            this.loading = false;
            this.error = "Failed to delete seniority.";
          },
        });
      }
    });
  }

  onBackToList() {
    this.showAddSeniority = false;
    this.showEditSeniority = false;
    this.showDeleteSeniority = false;
    this.seniorityToEdit = null;
    this.seniorityToDelete = null;
    this.fetchSeniorities();
  }
}
