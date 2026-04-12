import {
  Component,
  OnInit,
  QueryList,
  TemplateRef,
  ViewChild,
  ViewChildren,
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
import {
  Skills,
  SkillsServiceService,
} from "../../../../services/skills.service";
import { CommonDialogComponent } from "../common-dialog/common-dialog.component";
import { DialogData } from "../common-dialog/models/dialog-data.model";
import { MatIconModule } from "@angular/material/icon";
import { LoaderService } from "../../../../services/loader.service";
import { ToastrService } from "ngx-toastr";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: "app-skills",
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatTableModule,
    FormsModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./skills.component.html",
  styleUrl: "./skills.component.scss",
})
export class SkillsComponent implements OnInit {
  @ViewChild("addSkillTemplate") addSkillTemplate!: TemplateRef<any>;
  @ViewChild("editSkillTemplate") editSkillTemplate!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

  form: FormGroup;
  editForm: FormGroup;
  dialogError: string | null = null;

  skills: Skills[] = [];
  dataSource = new MatTableDataSource<Skills>([]);
  displayedColumns: string[] = [
    "id",
    "name",
    "createdAt",
    "updatedAt",
    "actions",
  ];
  error: string | null = null;

  // Pagination properties
  pageSize = 20;
  totalItems = 0;
  showAddSkill = false;
  showEditSkill = false;
  skillToEdit: Skills | null = null;
  searchText = "";
  constructor(
    private skillsService: SkillsServiceService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    public loaderService: LoaderService,
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
    this.fetchSkills();
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

  fetchSkills() {
    this.loaderService.start();
    this.skillsService.getAll().subscribe({
      next: (data) => {
        this.skills = data;
        this.dataSource.data = data;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.totalItems = data.length;
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load skills";
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

  onCreateSkill() {
    this.form.reset();
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Add Skill",
      message: "",
      confirmText: "Add",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addSkillTemplate,
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

  onEditSkill(skill: Skills) {
    this.skillToEdit = skill;
    this.editForm.reset();
    this.editForm.patchValue({ name: skill.name });
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit Skill",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editSkillTemplate,
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

  onDeleteSkill(skill: Skills) {

    const dialogData: DialogData = {
      title: "Delete Skill",
      message: `Are you sure you want to delete the Skill "<strong>${skill.name}</strong>"? This action cannot be undone.`,
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
        this.onSubmitDeleteDialog(skill.id!);
      }
      this.onCancelDialog();
    });
  }

  onBackToList() {
    this.showAddSkill = false;
    this.showEditSkill = false;
    this.skillToEdit = null;
    this.fetchSkills();
  }

  onSubmitDialog(): void {
    if (this.form.invalid) return;

    this.loaderService.start();
    this.dialogError = null;

    this.skillsService.create(this.form.value).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchSkills();
      },
      error: () => {
        this.dialogError = "Failed to add skill";
        this.loaderService.stop();
      },
    });
  }

  onSubmitEditDialog(): void {
    if (this.editForm.invalid || !this.skillToEdit) return;

    this.loaderService.start();
    this.dialogError = null;

    this.skillsService
      .update(this.skillToEdit.id!, this.editForm.value)
      .subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
          this.loaderService.stop();
          this.dialog.closeAll();
          this.fetchSkills();
        },
        error: () => {
          this.dialogError = "Failed to update skill";
          this.loaderService.stop();
        },
      });
  }

  onSubmitDeleteDialog(id: number): void {

    this.loaderService.start();
    this.dialogError = null;

    this.skillsService.delete(id).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchSkills();
      },
      error: () => {
        this.dialogError = "Failed to delete skill";
        this.loaderService.stop();
      },
    });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
  }
}
