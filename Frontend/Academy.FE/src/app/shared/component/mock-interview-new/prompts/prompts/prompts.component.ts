import {
  Component,
  OnInit,
  QueryList,
  TemplateRef,
  ViewChild,
  ViewChildren,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatTableModule } from "@angular/material/table";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSort, MatSortModule } from "@angular/material/sort";
import {
  MatPaginator,
  MatPaginatorModule,
  PageEvent,
} from "@angular/material/paginator";
import { MatDialog } from "@angular/material/dialog";
import {
  FormBuilder,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import {
  Prompt,
  PromptsService,
} from "../../../../../services/prompts.service";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { DialogData } from "../../common-dialog/models/dialog-data.model";
import { map, Observable, of, startWith } from "rxjs";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatIconModule } from "@angular/material/icon";
import { LoaderService } from "../../../../../services/loader.service";
import { ToastrService } from "ngx-toastr";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: "app-prompts",
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatTableModule,
    MatAutocompleteModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSortModule,
    MatPaginatorModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./prompts.component.html",
  styleUrl: "./prompts.component.css",
})
export class PromptsComponent implements OnInit {
  @ViewChild("addPromptTemplate") addPromptTemplate!: TemplateRef<any>;
  @ViewChild("editPromptTemplate") editPromptTemplate!: TemplateRef<any>;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  form: FormGroup;
  editForm: FormGroup;
  dialogError: string | null = null;
  activePromptID: number | undefined = 0;
  prompts: Prompt[] = [];
  dataSource = new MatTableDataSource<Prompt>([]);
  displayedColumns: string[] = [
    "id",
    "prompt",
    "version",
    "usage",
    "preferredModel",
    "createdAt",
    "updatedAt",
    "actions",
  ];
  error: string | null = null;
  usageList = [
    "analyzingAnswers",
    "analyzingInterview",
    "suggestingImprovements",
    "AnalysingAnswerAndGenerateScore",
    "AnalysingSummaryAndScore",
  ];

  usageControl = new FormControl("");
  editUsageControl = new FormControl("");
  filteredUsages$: Observable<string[]> = of([]);
  filteredEditUsages$: Observable<string[]> = of([]);
  searchText = "";
  pageSize = 5;
  totalItems = 0;
  promptToEdit: Prompt | null = null;

  constructor(
    private promptsService: PromptsService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    public loaderService: LoaderService,
    private toastr: ToastrService
  ) {
    this.form = this.fb.group({
      prompt: ["", [Validators.required]],
      version: [null, [Validators.required]],
      usage: [""],
      preferredModel: [""],
      reasonForChange: [""],
    });
    this.editForm = this.fb.group({
      prompt: ["", [Validators.required]],
      version: [null, [Validators.required]],
      usage: [""],
      preferredModel: [""],
      reasonForChange: [""],
    });
  }

  ngOnInit() {
    this.fetchPrompts();
    // Autocomplete filter for "Add"
    this.filteredUsages$ = this.usageControl.valueChanges.pipe(
      startWith(""),
      map((value: any) => this._filterUsage(value))
    );

    // Autocomplete filter for "Edit"
    this.filteredEditUsages$ = this.editUsageControl.valueChanges.pipe(
      startWith(""),
      map((value: any) => this._filterUsage(value || ""))
    );
  }
  private _filterUsage(value: string): string[] {
    const filterValue = value?.toLowerCase() || "";
    return this.usageList.filter((option) =>
      option.toLowerCase().includes(filterValue)
    );
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
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

  fetchPrompts() {
    this.loaderService.start();
    this.promptsService.getAll().subscribe({
      next: (data) => {
        this.prompts = data;
        this.dataSource.data = data;
        this.totalItems = data.length;
        this.loaderService.stop();
      },
      error: () => {
        this.error = "Failed to load prompts";
        this.loaderService.stop();
      },
    });
  }

  applyFilter() {
    this.dataSource.filter = this.searchText.trim().toLowerCase();
    this.totalItems = this.dataSource.filteredData.length;
  }

  onPageChanged(event: PageEvent) {
    this.pageSize = event.pageSize;
    // Handle pagination logic here if needed
  }

  onCreateAction() {
    this.form.reset();
    this.dialogError = null;
    this.usageControl.setValue(this.form.get("usage")?.value || "");

    this.usageControl.valueChanges.subscribe((val) => {
      this.form.get("usage")?.setValue(val);
    });

    const dialogData: DialogData = {
      title: "Add Prompt",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addPromptTemplate,
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

  onEditAction(prompt: Prompt) {
    this.promptToEdit = prompt;
    this.editUsageControl.setValue(prompt.usage || "");

    this.editUsageControl.valueChanges.subscribe((val) => {
      this.editForm.get("usage")?.setValue(val);
    });

    this.editForm.patchValue({
      prompt: prompt.prompt,
      version: prompt.version,
      usage: prompt.usage || "",
      preferredModel: prompt.preferredModel || "",
      reasonForChange: prompt.reasonForChange || "",
    });
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit Prompt",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editPromptTemplate,
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

  onSubmitDialog(): void {
    if (this.form.invalid) return;

    this.loaderService.start();
    this.dialogError = null;

    this.promptsService.create(this.form.value).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchPrompts();
      },
      error: () => {
        this.dialogError = "Failed to add prompt";
        this.loaderService.stop();
      },
    });
  }

  onSubmitEditDialog(): void {
    if (this.editForm.invalid || !this.promptToEdit) return;

    this.loaderService.start();
    this.dialogError = null;

    this.promptsService
      .update(this.promptToEdit.id!, this.editForm.value)
      .subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
          this.loaderService.stop();
          this.dialog.closeAll();
          this.fetchPrompts();
        },
        error: () => {
          this.dialogError = "Failed to update prompt";
          this.loaderService.stop();
        },
      });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
  }
  onToggle(prompt: Prompt) {
    this.activePromptID = prompt.id;
    const dialogData: DialogData = {
      title: "Prompt",
      message: `${prompt.prompt}`,
      showActions: false,
    };
    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "500px",
      data: dialogData,
    });
    dialogRef.afterClosed().subscribe(() => {
      this.activePromptID = 0;
    });
  }
  onSubmitDeleteDialog(id: number): void {

    this.loaderService.start();
    this.dialogError = null;

    this.promptsService.delete(id).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchPrompts();
      },
      error: () => {
        this.dialogError = "Failed to delete prompt";
        this.loaderService.stop();
      },
    });
  }
  onDeleteAction(prompt: Prompt) {

    const dialogData: DialogData = {
      title: "Delete Prompt",
      message: `Are you sure you want to delete the Prompt? This action cannot be undone.`,
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
        this.onSubmitDeleteDialog(prompt.id!);
      }
      this.onCancelDialog();
    });
  }
}
