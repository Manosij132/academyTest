import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  AfterViewInit,
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
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSortModule, MatSort } from "@angular/material/sort";
import {
  MatPaginatorModule,
  MatPaginator,
  PageEvent,
} from "@angular/material/paginator";
import {
  AIModel,
  AIModelsService,
} from "../../../../../services/aimodels.service";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { DialogData } from "../../common-dialog/models/dialog-data.model";
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatIconModule } from "@angular/material/icon";
import { LoaderService } from "../../../../../services/loader.service";
import { ToastrService } from "ngx-toastr";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: "app-aimodels",
  standalone: true,
  imports: [
    CommonModule,
    MatProgressSpinner,
    FormsModule,
    MatIconModule,
    MatTableModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSortModule,
    MatPaginatorModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./aimodels.component.html",
  styleUrl: "./aimodels.component.css",
})
export class AIModelsComponent implements OnInit, AfterViewInit {
  @ViewChild("addAIModelTemplate") addAIModelTemplate!: TemplateRef<any>;
  @ViewChild("editAIModelTemplate") editAIModelTemplate!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  form: FormGroup;
  editForm: FormGroup;
  dialogError: string | null = null;

  aimodels: AIModel[] = [];
  dataSource = new MatTableDataSource<AIModel>([]);
  displayedColumns: string[] = [
    "id",
    "modelName",
    "version",
    "usage",
    "createdAt",
    "updatedAt",
    "actions",
  ];
  error: string | null = null;
  showAddAIModel = false;
  showEditAIModel = false;
  showDeleteAIModel = false;
  aiModelToEdit: AIModel | null = null;
  aiModelToDelete: AIModel | null = null;
  searchText = "";

  // Pagination properties
  pageSize = 5;
  totalItems = 0;

  constructor(
    private aimodelsService: AIModelsService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    public loaderService: LoaderService,
    private toastr: ToastrService
  ) {
    this.form = this.fb.group({
      modelName: ["", [Validators.required, Validators.maxLength(100)]],
      version: ["", [Validators.required, Validators.maxLength(50)]],
      usage: [""],
    });
    this.editForm = this.fb.group({
      modelName: ["", [Validators.required, Validators.maxLength(100)]],
      version: ["", [Validators.required, Validators.maxLength(50)]],
      usage: [""],
    });
  }

  ngOnInit() {
    this.fetchAIModels();
  }

  ngAfterViewInit() {
    // Set initial page size
    if (this.paginator) {
      this.paginator.pageSize = 5;
    }
  }

  ngAfterViewChecked() {
    if (this.sort && this.dataSource.sort !== this.sort) {
      this.dataSource.sort = this.sort;
    }
    if (this.paginator && this.dataSource.paginator !== this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }

  fetchAIModels() {
    this.loaderService.start();
    this.aimodelsService.getAll().subscribe({
      next: (data) => {
        this.aimodels = data;
        this.dataSource.data = data;
        this.totalItems = data.length;
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load AI Models";
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
    // Note: For client-side pagination, MatTableDataSource handles this automatically
    // For server-side pagination, you would make an API call here with the new page parameters
  }

  onCreateAIModel() {
    this.form.reset();
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Add AI Model",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addAIModelTemplate,
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

  onEditAIModel(model: AIModel) {
    this.aiModelToEdit = model;
    this.editForm.reset();
    // Convert usage array to comma-separated string for editing
    const usageString = Array.isArray(model.usage)
      ? model.usage.join(", ")
      : model.usage || "";
    this.editForm.patchValue({
      modelName: model.modelName,
      version: model.version,
      usage: usageString,
    });
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit AI Model",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editAIModelTemplate,
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

  onBackToList() {
    this.showAddAIModel = false;
    this.showEditAIModel = false;
    this.showDeleteAIModel = false;
    this.aiModelToEdit = null;
    this.aiModelToDelete = null;
    this.fetchAIModels();
  }

  isArray(val: any): boolean {
    return Array.isArray(val);
  }

  onSubmitDialog(): void {
    if (this.form.invalid) return;

    this.dialogError = null;
    const formValue = this.form.value;
    let usageArray: string[] = [];
    if (formValue.usage) {
      usageArray = formValue.usage
        .split(",")
        .map((u: string) => u.trim())
        .filter((u: string) => u.length > 0);
    }

    const aiModelData = {
      modelName: formValue.modelName,
      version: formValue.version,
      usage: usageArray,
    };

    this.loaderService.start();
    this.aimodelsService.create(aiModelData).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchAIModels();
      },
      error: () => {
        this.dialogError = "Failed to add AI Model";
        this.loaderService.stop();
      },
    });
  }

  onSubmitEditDialog(): void {
    if (this.editForm.invalid || !this.aiModelToEdit) return;

    this.loaderService.start();
    this.dialogError = null;

    // Convert comma-separated usage string to array
    const formValue = this.editForm.value;
    let usageArray: string[] = [];
    if (formValue.usage) {
      usageArray = formValue.usage
        .split(",")
        .map((u: string) => u.trim())
        .filter((u: string) => u.length > 0);
    }

    const aiModelData = {
      modelName: formValue.modelName,
      version: formValue.version,
      usage: usageArray,
    };

    this.aimodelsService.update(this.aiModelToEdit.id!, aiModelData).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchAIModels();
      },
      error: () => {
        this.dialogError = "Failed to update AI Model";
        this.loaderService.stop();
      },
    });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
  }

  openDeletePopup(aiModelData: any) {

    const dialogData: DialogData = {
      title: "Delete AI model",
      message: `Are you sure you want to delete the AI model ? This action cannot be undone.`,
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
        this.onSubmitDeleteDialog(aiModelData.id!);
      }
      this.onCancelDialog();
    });
  }

  onSubmitDeleteDialog(id: number): void {

    this.loaderService.start();
    this.dialogError = null;

    this.aimodelsService.delete(id).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchAIModels();
      },
      error: () => {
        this.dialogError = "Failed to delete AI model";
        this.loaderService.stop();
      },
    });
  }
}
