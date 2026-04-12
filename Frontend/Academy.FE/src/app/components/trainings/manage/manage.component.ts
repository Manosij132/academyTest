import { CommonModule } from "@angular/common";
import { Component, OnInit, inject } from "@angular/core";
import {
  AbstractControl,  FormArray,  FormBuilder,  FormControl,
  FormGroup,  FormsModule,  ReactiveFormsModule,  ValidationErrors,
  ValidatorFn,  Validators
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCard, MatCardModule } from "@angular/material/card";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { ActivatedRoute, Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { AcademyHttpService } from "@services/academy-http.service";
import { LoaderService } from "@services/loader.service";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { ExpectedProficiency } from "@shared/dto/manage-training-form-dto";
import { ManageTrainingRequest } from "@shared/dto/ManageTrainingRequest";
import { AddEcosystemDialogComponent } from "@components/trainings/add-ecosystem-dialog/add-ecosystem-dialog.component";
import { AddSkillDialogComponent } from "@components/trainings/add-skill-dialog/add-skill-dialog.component";
import { MatCheckboxModule } from "@angular/material/checkbox";

@Component({
  selector: "app-manage",
  standalone: true,
  imports: [
    CommonModule,
    MatInputModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    FormsModule,
    MatSelectModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCard,
    MatCardModule,
    MatCheckboxModule,
  ],
  templateUrl: "./manage.component.html",
  styleUrls: ["./manage.component.scss"],
})
export class ManageComponent implements OnInit {
  focused = false;
  trainingForm!: FormGroup;
  ecosystems: any[] = [];
  skills: any[] = [];
  request = new ManageTrainingRequest();
  transactionId: string = "";
  loadEcosystemModal = false;
  loadSkillModal = false;
  seniorities: any[] = [];
  proficiencyMaster: any[] = [];
  ecosystemSkillProficiency: any[] = [];
  readonly dialog = inject(MatDialog);

  addProficiencyMapping = false;
  viewProficiencyMapping = false;
  mappingEnumerable: any[] = [];
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    public readonly router: Router,
    private fb: FormBuilder,
    private loaderService: LoaderService
  ) {}

  ngOnInit(): void {
    this.loadEcosystems();
    this.loadSeniorities();
    this.loadProficiencyMaster();
    this.loadSkills();
    this.initForm();
  }

  initForm() {
    this.trainingForm = this.fb.group({
      ecosystemId: new FormControl(null, [Validators.required]),
      skillId: new FormControl(null, [Validators.required]),
      trainingId: new FormControl(0, [Validators.required]),
      trainingName: new FormControl("", [Validators.required]),
      trainingDescription: new FormControl("", [Validators.required]),
      ismvp: new FormControl(false),
      isPriortize: new FormControl(false),
      trainingUrl: new FormControl("", [
        Validators.required,
        Validators.pattern("https?://.+"),
      ]),
      trainingCompletionHours: new FormControl(0, [
        Validators.required,
        Validators.min(0),
      ]),
      expectedProficiency: this.fb.array([]),
    });
    // this.createProficiencyFormGroup();
  }

  get expectedProficiencyArray(): FormArray {
    return this.trainingForm.get("expectedProficiency") as FormArray;
  }

  private createFormGroupFromProficiency(
    proficiency: ExpectedProficiency
  ): FormGroup {
    return this.fb.group({
      seniorityId: [
        proficiency.seniorityId,
        [Validators.required, notZeroValidator()],
      ],
      proficiencyValue: [
        proficiency.proficiencyId,
        [Validators.required, notZeroValidator()],
      ],
    });
  }

  private bindProficiencyToFormArray(): void {
    this.ecosystemSkillProficiency.forEach((proficiency) => {
      // Create a FormGroup for the current task object
      const taskGroup = this.createFormGroupFromProficiency(proficiency);

      // Push the FormGroup into the FormArray
      this.expectedProficiencyArray.push(taskGroup);
    });
  }

  loadEcosystemSkillProficiencies() {
    const selectedEcosystemId =
      this.trainingForm.get("ecosystemId")?.value || 0;
    const selectedSkillId = this.trainingForm.get("skillId")?.value || 0;
    this.loaderService.start();
    this.academyHttpService
      .fetchProficiencyByEcosystemSkill(selectedEcosystemId, selectedSkillId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.ecosystemSkillProficiency = response.data;
            if (this.ecosystemSkillProficiency.length <= 0) {
              this.createProficiencyFormGroup();
            } else {
              this.bindProficiencyToFormArray();
            }
            this.addProficiencyMapping = true;
            //   this.ecosystemSkillProficiency.length === 0;
            this.viewProficiencyMapping = true;
            //   this.ecosystemSkillProficiency.length > 0;
          } else {
            this.toastr.error(
              response.errorMessage,
              "Ecosystem Skill Proficiencies Load Error"
            );
          }
        },
      });
  }

  loadProficiencyMaster() {
    this.loaderService.start();
    this.academyHttpService
      .fetchProficiencyMaster()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.proficiencyMaster = response.data;
          } else {
            this.toastr.error(
              response.errorMessage,
              "Proficiency Master Load Error"
            );
          }
        },
      });
  }

  loadSeniorities() {
    this.loaderService.start();
    this.academyHttpService
      .fetchSeniorities()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.seniorities = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  onMvpChanged(event: any) {
    this.trainingForm.get("ismvp")?.setValue(event.target.checked);
  }

  createProficiencyFormGroup() {
    const taskGroup = this.createFormGroupFromProficiency({
      proficiencyId: 0,
      seniorityId: 0,
    });
    this.expectedProficiencyArray.push(taskGroup);
  }

  get expectedProficiency(): FormArray {
    return this.trainingForm.get("expectedProficiency") as FormArray;
  }

  addProficiency(): void {
    // this.expectedProficiency.push(this.createProficiencyFormGroup());
    const taskGroup = this.createFormGroupFromProficiency({
      proficiencyId: 0,
      seniorityId: 0,
    });
    this.expectedProficiencyArray.push(taskGroup);
  }

  removeProficiency(index: number): void {
    this.expectedProficiency.removeAt(index);
  }

  onSubmit(): void {
    if (this.trainingForm.valid) {
      this.loaderService.start();
      this.academyHttpService
        .insertTraining(this.trainingForm.value)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (response: any) => {
            if (response.status === 200) {
              this.toastr.success(TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
              this.trainingForm.reset();
              this.initForm();
              this.addProficiencyMapping = false;
              this.viewProficiencyMapping = false;
            } else {
              this.toastr.error(TOASTER_MESSAGES.ERROR, "Error");
            }
          },
        });
    }
  }

  closeEcosystemModal() {
    this.loadEcosystemModal = false;
    this.loadEcosystems();
  }

  closeSkillModal() {
    this.loadSkillModal = false;
    this.loadSkills();
  }

  onEcosystemSkillChange() {
    if (
      this.trainingForm.get("ecosystemId")?.value !== 0 &&
      this.trainingForm.get("skillId")?.value !== 0
    ) {
      this.expectedProficiencyArray.clear();
      this.loadEcosystemSkillProficiencies();
    }
  }

  loadEcosystems() {
    if (this.request.ecosystem !== 0) return;
    this.loaderService.start();
    this.academyHttpService
      .fetchPrimaryEcosystems()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.ecosystems = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  loadSkills() {
    this.loaderService.start();
    this.academyHttpService
      .fetchSkills()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.skills = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  openEcosystemPopup() {
    this.loadEcosystemModal = true;
    const dialogRef = this.dialog.open(AddEcosystemDialogComponent, {
      // width: '800px',
      width: "600px",
      disableClose: true, // Disable closing when clicking outside the dialog
    });

    dialogRef.afterClosed().subscribe((result) => {
      console.log("The dialog was closed");
    });
  }

  openSkillPopup() {
    this.loadSkillModal = true;
    const dialogRef = this.dialog.open(AddSkillDialogComponent, {
      // width: '800px',
      width: "600px",
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result === true) {
        this.loadSkills();
      }
      console.log("The dialog was closed");
    });
  }
}

export function notZeroValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    // Check if the value is strictly 0 and not null or undefined
    if (
      control.value === 0 &&
      control.value !== null &&
      control.value !== undefined
    ) {
      // Return an error object if the value is 0
      return { zeroError: { value: control.value } };
    }

    // Return null if the value is not 0 (meaning it is valid)
    return null;
  };
}
