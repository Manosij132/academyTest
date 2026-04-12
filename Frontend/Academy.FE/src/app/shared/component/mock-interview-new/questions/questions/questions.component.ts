import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  AfterViewInit,
  ViewChildren,
  QueryList,
} from "@angular/core";
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
import { CommonModule } from "@angular/common";
import { FormControl, FormsModule } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import {
  Skills,
  SkillsServiceService,
} from "../../../../../services/skills.service";
import { QuestionsService } from "../../../../../services/questions.service";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { DialogData } from "../../common-dialog/models/dialog-data.model";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { Observable } from "rxjs";
import { AutocompleteService } from "../../../../../services/autocomplete.service";
import { LoaderService } from "../../../../../services/loader.service";
import { ToastrService } from "ngx-toastr";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: "app-questions",
  standalone: true,
  templateUrl: "./questions.component.html",
  styleUrls: ["./questions.component.css"],
  imports: [
    MatTableModule,
    MatButtonModule,
    MatFormFieldModule,
    MatAutocompleteModule,
    MatInputModule,
    MatSelectModule,
    MatSortModule,
    MatPaginatorModule,
    MatIconModule,
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
  ],
})
export class QuestionsComponent implements OnInit, AfterViewInit {
  @ViewChild("addQuestionTemplate") addQuestionTemplate!: TemplateRef<any>;
  @ViewChild("editQuestionTemplate") editQuestionTemplate!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;
  skillControl = new FormControl<Skills | null>(null);
  filteredSkills!: Observable<any[]>;
  questions: any[] = [];
  dataSource = new MatTableDataSource<any>([]);
  displayedColumns: string[] = [
    "id",
    "skill",
    "section",
    "question",
    // "actions",
  ];
  error: string | null = null;

  // Pagination properties
  pageSize = 25;
  totalItems = 0;
  showScoreGuideline = false;
  showAddQuestion = false;
  showEditQuestion = false;
  showDeleteQuestion = false;
  questionToEdit: any = null;
  questionToDelete: any = null;

  // Form and dialog properties
  form: FormGroup;
  editForm: FormGroup;
  skills: Skills[] = [];
  dialogError: string | null = null;
  searchText = "";
  // Score guideline interface
  scoreGuideline: Record<string, Array<{ topic: string; score: number }>> = {};

  constructor(
    private questionsService: QuestionsService,
    private skillsService: SkillsServiceService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private autoCompleteService: AutocompleteService,
    public loaderService: LoaderService,
    private toastr: ToastrService
  ) {
    this.form = this.fb.group({
      questionText: ["", [Validators.required]],
      section: [""],
      skillId: [null],
      status: ["active"],
      coachGuideline: [""],
      scoreGuideline: [{}],
    });
    this.editForm = this.fb.group({
      questionText: ["", [Validators.required]],
      section: [""],
      skillId: this.skillControl,
      status: ["active"],
      coachGuideline: [""],
      scoreGuideline: [{}],
    });
  }

  ngOnInit(): void {
    this.loadQuestions();
    this.loadSkills();
  }

  ngAfterViewInit(): void {
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'skill':
          return item.skill?.name?.toLowerCase() || '';
        case 'section':
          return item.section?.name?.toLowerCase() || '';
        default:
          return item[property];
      }
    };

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

  loadSkills() {
    this.loaderService.start();
    this.skillsService.getAll().subscribe({
      next: (data) => {
        this.skills = data || [];
        this.filteredSkills = this.autoCompleteService.setupFilter(
          this.skillControl,
          this.skills,
          "name"
        );
        this.loaderService.stop();
      },
      error: () => {
        (this.skills = []), this.loaderService.stop();
      },
    });
  }

  loadQuestions() {
    this.loaderService.start();
    this.error = null;
    this.questionsService.getAll().subscribe({
      next: (data) => {
        this.questions = data;
        this.dataSource.data = data;
        this.totalItems = data.length;
        this.loaderService.stop();

      },
      error: (err) => {
        this.error = "Failed to load questions.";
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

  onCreateQuestion() {
    this.form.reset();
    this.skillControl.reset();
    this.form.patchValue({
      status: "active",
    });
    this.scoreGuideline = {
      must: [],
      optional: [],
      mustNot: [],
    };

    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Add Question",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addQuestionTemplate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "800px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.onSubmitDialog();
      }
      this.onCancelDialog();
    });
  }

  onEditQuestion(question: any) {
    this.questionToEdit = question;
    const selectedSkill =
      this.skills.find((p) => p.id == question.skill.id) || null;
    this.skillControl.setValue(selectedSkill);
    this.editForm.patchValue({
      questionText: question.question || "",
      section: question.section || "",
      status: question.status || "active",
      coachGuideline: question.coachGuideline || "",
    });
    try {
      const parsed = question.scoreGuideline
        ? typeof question.scoreGuideline === "string"
          ? JSON.parse(question.scoreGuideline)
          : question.scoreGuideline
        : {};

      this.scoreGuideline = {
        must: [],
        mustNot: [],
        optional: [],
      };
      for (const section of ["must", "mustNot", "optional"]) {
        if (parsed[section] && parsed[section][0]) {
          const obj = parsed[section][0];
          this.scoreGuideline[section] = Object.keys(obj).map((key) => ({
            topic: key,
            score: obj[key as keyof typeof obj],
          }));
        }
      }
    } catch {
      this.scoreGuideline = {
        must: [],
        optional: [],
        mustNot: [],
      };
    }

    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit Question",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editQuestionTemplate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "800px",
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
    this.showAddQuestion = false;
    this.showEditQuestion = false;
    this.showDeleteQuestion = false;
    this.questionToEdit = null;
    this.questionToDelete = null;
    this.loadQuestions();
  }

  onSubmitDialog(): void {
    if (this.form.invalid) return;
    this.dialogError = null;

    const formValue = this.form.value;
    const question = {
      question: formValue.questionText,
      section: formValue.section,
      skill: formValue.skillId ? { id: formValue.skillId.id } : null,
      status: formValue.status,
      coachGuideline: formValue.coachGuideline,
      scoreGuideline: this.transformScoreGuideline(),
    };

    this.loaderService.start();
    this.questionsService.create(question).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.showScoreGuideline = false;
        this.loadQuestions();
      },
      error: () => {
        this.dialogError = "Failed to add question";
        this.loaderService.stop();
      },
    });
  }

  onSubmitEditDialog(): void {
    if (this.editForm.invalid || !this.questionToEdit) return;

    this.dialogError = null;

    const formValue = this.editForm.value;
    const updatedQuestion = {
      ...this.questionToEdit,
      question: formValue.questionText,
      section: formValue.section,
      skill: formValue.skillId ? { id: formValue.skillId.id } : null,
      status: formValue.status,
      coachGuideline: formValue.coachGuideline,
      scoreGuideline: this.transformScoreGuideline(),
    };

    this.loaderService.start();
    this.questionsService
      .update(this.questionToEdit.id, updatedQuestion)
      .subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
          this.loaderService.stop();
          this.dialog.closeAll();
          this.showScoreGuideline = false;
          this.loadQuestions();
        },
        error: () => {
          this.dialogError = "Failed to update question";
          this.loaderService.stop();
        },
      });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
    this.showScoreGuideline = false;
  }

  // Score guideline management methods
  addSection(): void {
    const newSectionName = this.getUniqueSectionName();

    // Add the new section safely
    this.scoreGuideline[newSectionName] = [];

    // Trigger view update if necessary
    this.scoreGuideline = { ...this.scoreGuideline };
  }

  removeSection(sectionName: string): void {
    if (this.scoreGuideline && this.scoreGuideline[sectionName]) {
      delete this.scoreGuideline[sectionName];
    }
  }

  addTopic(sectionName: string): void {
    if (!this.scoreGuideline[sectionName]) {
      // Initialize the section if it doesn't exist
      this.scoreGuideline[sectionName] = [];
      // Trigger Angular change detection (optional but safer)
      this.scoreGuideline = { ...this.scoreGuideline };
    }

    this.scoreGuideline[sectionName].push({ topic: "", score: 1 });
  }

  removeTopic(sectionName: string, idx: number) {
    this.scoreGuideline[sectionName].splice(idx, 1);
  }

  getSectionNames(): string[] {
    return ["must", "optional", "mustNot"];
  }

  getUniqueSectionName(): string {
    let base = "must";
    let idx = 1;
    while (this.scoreGuideline[`${base} ${idx}`]) idx++;
    return `${base} `;
  }

  renameSection(oldName: string, newName: string) {
    newName = newName.trim();
    if (!newName || oldName === newName || this.scoreGuideline[newName]) {
      return;
    }
    this.scoreGuideline[newName] = this.scoreGuideline[oldName];
    delete this.scoreGuideline[oldName];
  }
  enableScoreGuideline() {
    this.showScoreGuideline = true;

    // If no sections exist, initialize
    if (!this.scoreGuideline || Object.keys(this.scoreGuideline).length === 0) {
      this.scoreGuideline = {
        must: [],
        optional: [],
        mustNot: [],
      };
    }
  }
  getSectionLabel(section: string): string {
    switch (section) {
      case "must":
        return "Must Have Sections";
      case "optional":
        return "Optional Sections";
      case "mustNot":
        return "Must NOT Include Sections";
      default:
        return section;
    }
  }
  hasAllRequiredSections(): boolean {
    const requiredSections = ["must", "optional", "mustNot"];
    return requiredSections.every((section) => section in this.scoreGuideline);
  }
  onSkillSelectionChange(skill: Skills): void {
    let skillId = skill.id;
    const newSkill = skillId
      ? this.skills.find((c) => c.id == skillId) || null
      : null;
    this.form.patchValue({ skillId: newSkill });
  }
  displaySkill(skill: Skills): string {
    return skill && skill.name ? skill.name : "";
  }

  transformScoreGuideline(): Record<string, any> {
    const transformed: Record<string, any> = {};

    for (const section of Object.keys(this.scoreGuideline)) {
      const sectionObject: Record<string, number> = {};

      for (const item of this.scoreGuideline[section]) {
        if (item.topic && item.score !== undefined) {
          sectionObject[item.topic] = item.score;
        }
      }

      // Wrap object inside an array to match your expected output
      transformed[section] =
        Object.keys(sectionObject).length > 0 ? [sectionObject] : [];
    }

    return transformed;
  }
  onDeleteQuestion(question: any) {

    const dialogData: DialogData = {
      title: "Delete Question",
      message: `Are you sure you want to delete the Question ? This action cannot be undone.`,
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
        this.onSubmitDeleteDialog(question.id!);
      }
      this.onCancelDialog();
    });
  }

  onSubmitDeleteDialog(id: number): void {

    this.loaderService.start();
    this.dialogError = null;

    this.questionsService.delete(id).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.loadQuestions();
      },
      error: () => {
        this.dialogError = "Failed to delete question";
        this.loaderService.stop();
      },
    });
  }
}
