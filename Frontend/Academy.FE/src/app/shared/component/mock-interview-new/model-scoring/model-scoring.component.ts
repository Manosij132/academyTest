import {
  Component,
  ViewChild,
  AfterViewInit,
  OnInit,
  TemplateRef,
  ViewChildren,
  QueryList,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatTableModule, MatTableDataSource } from "@angular/material/table";
import { MatSortModule, MatSort } from "@angular/material/sort";
import {
  MatPaginatorModule,
  MatPaginator,
  PageEvent,
} from "@angular/material/paginator";
import { MatButtonModule } from "@angular/material/button";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { InterviewsService } from "../../../../services/interviews.service";
import { CommonDialogComponent } from "../common-dialog/common-dialog.component";
import { DialogData } from "../common-dialog/models/dialog-data.model";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatIconModule } from "@angular/material/icon";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { AIModelsService } from "../../../../services/aimodels.service";
import { MatOptionModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { LoaderService } from "../../../../services/loader.service";

@Component({
  selector: "app-model-scoring",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatSelectModule,
    MatOptionModule,
    MatDialogModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./model-scoring.component.html",
  styleUrl: "./model-scoring.component.css",
})
export class ModelScoringComponent implements OnInit, AfterViewInit {
  modelScoringDetails: any[] = [];
  error: string | null = null;
  addInterviewDetails: boolean = false;
  selectedInterviewDetails: any | null = null;
  selectedModelScoring: any | null = null;
  interviewToBeDeleted: any | null = null;
  activePromptID: number | undefined = 0;
  displayedColumns: string[] = [
    "interviewDetailId",
    "modelId",
    "prompt",
    "score",
    "comments",
    "actions",
  ];
  dataSource = new MatTableDataSource<any>();
  searchText = "";

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild("addModelScoringTemplate")
  addModelScoringTemplate!: TemplateRef<any>;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

  form: FormGroup;
  dialogError: string | null = null;
  pageSize = 5;
  totalItems = 0;
  aimodels: any;

  constructor(
    private interviewService: InterviewsService,
    private aimodelsService: AIModelsService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private loaderService: LoaderService
  ) {
    this.form = this.fb.group({
      model: [null],
      prompt: [""],
      modelScore: [""],
      modelComments: ["", []],
      manualOverrideScore: [90, [Validators.required]],
      manualOverrideComments: ["", [Validators.required]],
      interviewCode: ["", [Validators.required]],
      id: [],
    });
  }

  ngOnInit() {
    this.fetchAIModels();
    setTimeout(() => this.fetchModelScoringDetailsDetails(), 500);
  }
  fetchAIModels() {
    this.loaderService.start();
    this.aimodelsService.getAll().subscribe({
      next: (data) => {
        this.aimodels = data;
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load AI Models";
        this.loaderService.stop();
      },
    });
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

  applyFilter() {
    this.dataSource.filter = this.searchText.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  public fetchModelScoringDetailsDetails() {
    this.loaderService.start();
    this.interviewService
      .fetchModelScoringDetailsDetails("1")
      .subscribe((details: any) => {
        let modelScoringDetails = details?.length
          ? structuredClone(details)
          : [];
        modelScoringDetails = modelScoringDetails?.map((model: any) => {
          const aiModel = this.aimodels?.find(
            (aiModel: any) => aiModel.id == model.model
          );
          this.loaderService.stop();

          return {
            ...model,
            aiModelName: aiModel?.modelName ?? "-",
          };
        });

        this.modelScoringDetails = modelScoringDetails?.length
          ? structuredClone(modelScoringDetails)
          : [];
        this.dataSource.data = this.modelScoringDetails;
      });
  }

  public onCreateInterviewModelScore() {
    // Reset form and selected items for new entry
    this.selectedModelScoring = null;
    this.selectedInterviewDetails = null;
    this.form.reset();
    this.form.patchValue({
      model: null,
      prompt: "",
      modelScore: "",
      modelComments: "",
      manualOverrideScore: "",
      manualOverrideComments: "",
      interviewCode: "",
      id: null,
    });

    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Add Model Scoring",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: false,
      form: this.form,
      template: this.addModelScoringTemplate,
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

  public backToList(callApi: boolean) {
    callApi && this.fetchModelScoringDetailsDetails();
    this.addInterviewDetails = false;
    this.selectedInterviewDetails = null;
  }

  public editInterviewDetails(modelScoring: any) {
    this.selectedModelScoring = structuredClone(modelScoring);
    this.selectedInterviewDetails = structuredClone(modelScoring);

    // Populate form with existing data
    this.form.patchValue({
      model: modelScoring.model || 101,
      prompt: modelScoring.prompt || "",
      modelScore: modelScoring.modelScore || "",
      modelComments: modelScoring.modelComments || "",
      manualOverrideScore: modelScoring.manualOverrideScore || 90,
      manualOverrideComments: modelScoring.manualOverrideComments || "",
      interviewCode: modelScoring.interviewCode || "",
      id: modelScoring.id,
    });
    this.form.get("interviewCode")?.disable();
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit Model Scoring",
      message: "",
      confirmText: "Update",
      cancelText: "Cancel",
      showActions: false,
      form: this.form,
      template: this.addModelScoringTemplate,
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

    (this.selectedInterviewDetails
      ? this.interviewService.updateInterviewScoring(this.form.value)
      : this.interviewService.createInterviewScoring(this.form.value)
    ).subscribe({
      next: () => {
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchModelScoringDetailsDetails();
      },
      error: () => {
        this.dialogError = "Failed to add evaluation details";
        this.loaderService.stop();
      },
    });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
  }
  onPageChanged(event: PageEvent) {
    this.pageSize = event.pageSize;
    // Note: For client-side pagination, MatTableDataSource handles this automatically
    // For server-side pagination, you would make an API call here with the new page parameters
  }
  onToggle(prompt: any) {
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
}
