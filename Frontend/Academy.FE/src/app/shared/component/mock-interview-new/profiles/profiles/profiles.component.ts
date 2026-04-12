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
  ReactiveFormsModule,
  Validators,
  FormsModule,
  FormControl,
} from "@angular/forms";
import { MatTableModule } from "@angular/material/table";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatTableDataSource } from "@angular/material/table";
import { PageEvent } from "@angular/material/paginator";
import {
  Profile,
  ProfileService,
  ProfileRequest,
} from "../../../../../services/profile.service";
import {
  Skills,
  SkillsServiceService,
} from "../../../../../services/skills.service";
import {
  SenioritiesService,
  Seniority,
} from "../../../../../services/seniorities.service";
import { DialogData } from "../../common-dialog/models/dialog-data.model";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { Observable } from "rxjs";
import { AutocompleteService } from "../../../../../services/autocomplete.service";
import { MatIconModule } from "@angular/material/icon";
import { LoaderService } from "../../../../../services/loader.service";
import { ToastrService } from 'ngx-toastr';
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { MatTooltipModule } from "@angular/material/tooltip";
import { DataService, FitmentType, Client } from "@services/data.service";
import { Section, SectionsService } from "@services/sections.service";
import { MatSnackBar } from "@angular/material/snack-bar";

@Component({
  selector: "app-profiles",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatAutocompleteModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSortModule,
    MatPaginatorModule,
    MatCheckboxModule,
    ReactiveFormsModule,
    FormsModule,
    MatTooltipModule
  ],
  templateUrl: "./profiles.component.html",
  styleUrl: "./profiles.component.css",
})
export class ProfilesComponent implements OnInit {
  @ViewChild("addProfileTemplate") addProfileTemplate!: TemplateRef<any>;
  @ViewChild("editProfileTemplate") editProfileTemplate!: TemplateRef<any>;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  seniorityControl = new FormControl<Seniority | null>(null);
  filteredSeniorities!: Observable<any[]>;
  skillControl = new FormControl<Skills | null>(null);
  filteredSkills!: Observable<any[]>;
  form: FormGroup;
  editForm: FormGroup;
  dialogError: string | null = null;

  profiles: Profile[] = [];
  dataSource = new MatTableDataSource<Profile>([]);
  displayedColumns: string[] = [
    "profileId",
    "profileName",
    "seniority",
    "skills",
    "createdAt",
    "updatedAt",
    "actions",
  ];
  error: string | null = null;

  // Pagination properties
  pageSize = 5;
  totalItems = 0;
  showAddProfile = false;
  showEditProfile = false;
  showDeleteProfile = false;
  profileToEdit: Profile | null = null;
  profileToDelete: Profile | null = null;

  // Properties from addprofile component
  seniorities: Seniority[] = [];
  skills: Skills[] = [];
  selectedSkillId: number | null = null;
  selectedTopics: string[] = [];
  sections: { [skillId: string]: string[] } = {};
  skillTopicMap: { [skillId: number]: string[] } = {
    1: [
      "OOPS concepts",
      "Java basic concepts",
      "Java RDBMS",
      "Java NoSQL",
      "Java deployments",
      "cloud",
    ],
    2: ["Python basics", "Data Science", "Web Development", "Scripting"],
    3: ["SQL Joins", "Indexes", "Transactions"],
    4: ["JavaScript ES6", "DOM", "Async"],
    5: ["Types", "Interfaces", "Generics"],
    6: [".NET Core", "LINQ", "Entity Framework"],
    7: [
      "microservices pattern",
      "microservices deployment",
      "microservices problems",
    ],
    8: ["Go routines", "Channels", "Web APIs"],
  }; //api data to be mapped
  searchText = "";
  fitmentType: FitmentType[] = [];
  clients: Client[] = [];
  primarySkillId: number | null = null;
  isPrimarySkillSelected: boolean = false;
  sectionsCache: { [skillId: number]: Section[] } = {};
  constructor(
    private profileService: ProfileService,
    private dataService: DataService,
    private sectionsService: SectionsService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private senioritiesService: SenioritiesService,
    private skillsService: SkillsServiceService,
    private autoCompleteService: AutocompleteService,
    public loaderService: LoaderService,
    private snackBar: MatSnackBar,
    private toastr: ToastrService,
  ) {
    this.form = this.fb.group({
      fitmentType: [null, [Validators.required]],
      clients: [null],
      position: ["", [Validators.required]],
      seniorityId: [null, [Validators.required]],
      primarySkillValidator: [null, Validators.required]
    });
    this.editForm = this.fb.group({
      fitmentType: [null, [Validators.required]],
      clients: [null],
      position: ["", [Validators.required]],
      name: [""],
      seniorityId: [null, [Validators.required]],
      primarySkillValidator: [null, Validators.required]
    });
  }

  ngOnInit() {
    this.fetchProfiles();
    this.loadSeniorities();
    this.loadSkills();
    this.validateFitmentContext();
    this.fetchFitmentType();
    this.fetchClients();
    this.editForm.valueChanges.subscribe(() => {
      const name = this.getGeneratedProfileName();
      this.editForm.patchValue({ name }, { emitEvent: false });
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


  loadSeniorities() {
    this.loaderService.start();
    this.senioritiesService.getAll().subscribe({
      next: (data) => {
        this.seniorities = data || [];
        this.filteredSeniorities = this.autoCompleteService.setupFilter(
          this.seniorityControl,
          this.seniorities,
          "name",
        );
        this.loaderService.stop();
      },
      error: () => {
        this.error = "Failed to load seniorities.";
        this.loaderService.stop();
      },
    });
  }

  loadSkills() {
    this.loaderService.start();
    this.skillsService.getAll().subscribe({
      next: (data) => {
        this.skills = data || [];
        this.filteredSkills = this.autoCompleteService.setupFilter(
          this.skillControl,
          this.skills,
          "name",
        );
        this.loaderService.stop();
      },
      error: () => {
        this.error = "Failed to load skills.";
        this.loaderService.stop();
      },
    });
  }

  fetchProfiles() {
    this.loaderService.start();
    this.profileService.getAll().subscribe({
      next: (data) => {
        const transformed = data.map(p => ({
          ...p,
          skills: p.skillsAndSections?.map(s => ({
            id: s.skillId,
            name: s.skillName
          })) || []
        }));
        this.profiles = transformed;
        this.dataSource.data = transformed;
        this.totalItems = transformed.length;
        if (this.paginator) {
          this.dataSource.paginator = this.paginator;
        }
        if (this.sort) {
          this.dataSource.sort = this.sort;
        }
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load profiles";
        this.loaderService.stop();
      },
    });
  }

  fetchFitmentType() {
    this.loaderService.start();
    this.dataService.getAllFitmentTypes().subscribe({
      next: (data) => {
        this.fitmentType = data;

        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load fitment types";
        this.loaderService.stop();
      },
    });
  }

  fetchClients() {
    this.loaderService.start();
    this.dataService.getAllClients().subscribe({
      next: (data) => {
        this.clients = data;
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = "Failed to load client";
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

  openAddProfile() {
    this.form.reset();
    this.dialogError = null;
    this.selectedSkillId = null;
    this.selectedTopics = [];
    this.sections = {};
    this.seniorityControl.reset();
    this.skillControl.reset();

    const dialogData: DialogData = {
      title: "Create Profile",
      message: "",
      confirmText: "Create",
      cancelText: "Cancel",
      showActions: true,
      form: this.form,
      template: this.addProfileTemplate,
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

  openEditProfile(profile: Profile) {
    this.profileToEdit = profile;
    this.sections = {};
    this.selectedTopics = [];
    this.selectedSkillId = null;
    this.primarySkillId = profile.primarySkillId;
    this.dialogError = null;
    this.editForm.patchValue({
      fitmentType: profile.fitmentTypeId,
      clients: profile.clientId,
      position: profile.position,
      seniorityId: profile.seniorityId,
      name: profile.profileName
    });

    profile.skillsAndSections?.forEach(skill => {
      this.sections[skill.skillId] =
        skill.sections
          ?.map(sec => sec.name)
          .filter((name): name is string => !!name) || [];
    });

    profile.skillsAndSections?.forEach(skill => {
      const skillId = skill.skillId;
      this.sectionsService.getBySkillId(skillId).subscribe({
        next: (data) => {
          this.sectionsCache[skillId] = data;
        },
        error: () => {
          this.sectionsCache[skillId] = [];
        }
      });
    });

    const selectedSeniority =
      this.seniorities.find((p) => p.id == profile.seniorityId) || null;
    this.updatePrimarySkillValidator();
    const dialogData: DialogData = {
      title: "Edit Profile",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editProfileTemplate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "800px",
      data: dialogData,
    });

    dialogRef.afterOpened().subscribe(() => {
      this.seniorityControl.setValue(selectedSeniority);
    })

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.onSubmitEditDialog();
      }
      this.onCancelDialog();
    });
  }

  closeDialogs() {
    this.showAddProfile = false;
    this.showEditProfile = false;
    this.showDeleteProfile = false;
    this.profileToEdit = null;
    this.profileToDelete = null;
    this.fetchProfiles();
  }

  private getActiveForm(): FormGroup {
    return this.profileToEdit ? this.editForm : this.form;
  }

  // Helper methods from addprofile component
  getSkillName(skillId: number): string {
    const skill = this.skills.find((s: any) => s.id === +skillId);
    return skill ? skill.name : String(skillId);
  }

  getSectionSkillIds(): number[] {
    return Object.keys(this.sections).map((id) => +id);
  }

  isChecked(topic: string) {
    return (
      this.selectedSkillId &&
      this.sections[this.selectedSkillId] &&
      this.sections[this.selectedSkillId].includes(topic)
    );
  }

  removeSkill(skillId: number) {
    delete this.sections[skillId];
    if (this.selectedSkillId === skillId) {
      this.selectedTopics = [];
    }
    if (this.primarySkillId === skillId) {
      this.primarySkillId = null;
    }
    this.updatePrimarySkillValidator();
  }

  updatePrimarySkillValidator() {
    const activeForm = this.getActiveForm();
    if (
      this.primarySkillId &&
      this.sections[this.primarySkillId] &&
      this.sections[this.primarySkillId].length
    ) {
      activeForm.patchValue({ primarySkillValidator: true });
    } else {
      activeForm.patchValue({ primarySkillValidator: null });
    }
  }

  // Dialog handling methods
  onSubmitDialog(): void {
    if (this.form.invalid || !this.getSectionSkillIds().length) {
      this.dialogError =
        "Name, seniority, and at least one skill with topics are required.";
      return;
    }
    if (!this.primarySkillId) {
      this.dialogError = "Primary skill must be selected.";
      return;
    }

    this.dialogError = null;
    const skillsAndSections = Object.entries(this.sections)
      .reduce((acc: { [key: string]: number[] }, [skillId, sectionNames]) => {
        const numericSkillId = Number(skillId);
        const sectionIds = this.sectionsCache[numericSkillId]
          ?.filter(section => sectionNames.includes(section.name))
          .map(section => section.id) || [];
        acc[skillId] = sectionIds;
        return acc;
      }, {});

    const profile: ProfileRequest = {
      "fitmentType": this.form.get("fitmentType")?.value,
      "clientId": this.form.get("clients")?.value ?? undefined,
      "position": this.form.get("position")?.value,
      "seniority": this.form.value.seniorityId.id,
      "primarySkillId": this.primarySkillId,
      "skillsAndSections": skillsAndSections,
      "profileName": this.getGeneratedProfileName()
    };
    this.loaderService.start();
    this.profileService.create(profile).subscribe({
      next: (res: any) => {
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.loaderService.stop();
        this.dialog.closeAll();
        this.fetchProfiles();
      },
      error: () => {
        this.dialogError = "Failed to add profile";
        this.loaderService.stop();
      },
    });
    this.skillControl.reset();
  }

  onSubmitEditDialog(): void {
    if (
      this.editForm.invalid ||
      !this.getSectionSkillIds().length ||
      !this.profileToEdit ||
      !this.profileToEdit.profileId
    ) {
      this.dialogError =
        "Name, seniority, and at least one skill with topics are required.";
      return;
    }

    this.dialogError = null;

    const skillsAndSections = Object.entries(this.sections)
      .reduce((acc: { [key: string]: number[] }, [skillId, sectionNames]) => {

        const numericSkillId = Number(skillId);
        const sectionIds = this.sectionsCache[numericSkillId]
          ?.filter(section => sectionNames.includes(section.name))
          .map(section => section.id) || [];

        acc[skillId] = sectionIds;

        return acc;
      }, {});

    const updatedProfile: ProfileRequest = {
      fitmentType: this.editForm.value.fitmentType,
      position: this.editForm.value.position,
      seniority: this.editForm.value.seniorityId.id ?? this.profileToEdit.seniorityId,
      primarySkillId: this.primarySkillId ?? this.profileToEdit.primarySkillId,
      skillsAndSections: skillsAndSections,
      profileName: this.editForm.value.name,
      clientId: this.editForm.value.clients
    };

    this.loaderService.start();
    this.profileService
      .update(this.profileToEdit.profileId, updatedProfile)
      .subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
          this.loaderService.stop();
          this.dialog.closeAll();
          this.fetchProfiles();
        },
        error: () => {
          this.dialogError = "Failed to update profile";
          this.loaderService.stop();
        },
      });
  }

  onCancelDialog(): void {
    this.dialog.closeAll();
    this.skillControl.reset();
    this.primarySkillId = null;
  }

  onSenioritySelectionChange(seniority: Seniority): void {
    const activeForm = this.profileToEdit ? this.editForm : this.form;
    let seniorityId = seniority.id;
    const newSeniorityId = seniorityId
      ? this.seniorities.find((c) => c.id == seniorityId) || null
      : null;
    activeForm.patchValue({
      seniorityId: newSeniorityId
    });
    this.selectedTopics =
      this.selectedSkillId && this.sections[this.selectedSkillId]
        ? [...this.sections[this.selectedSkillId]]
        : [];
  }

  displaySeniority(seniority: Seniority): string {
    return seniority && seniority.name ? seniority.name : "";
  }
  onSkillSelectionChange(skill: Skills): void {
    if (!skill || skill.id == null) {
      this.selectedSkillId = null;
      return;
    }
    let skillId = skill.id;
    const newSkillId = skillId
      ? this.skills.find((c) => c.id == skillId) || null
      : null;
    this.selectedSkillId = newSkillId?.id || null;
    this.form.patchValue({ skillId: newSkillId });
    if (!this.primarySkillId && skillId) {
      this.primarySkillId = skillId;
      this.form.patchValue({ name: this.getGeneratedProfileName() });
    }
    this.sectionsService.getBySkillId(skillId).subscribe({
      next: (data) => {
        this.sectionsCache[skillId] = data;
        this.skillTopicMap[skillId] = data.map(section => section.name);
      },
      error: () => {
        this.skillTopicMap[skillId] = [];
      }
    });

  }
  displaySkill(skill: Skills): string {
    return skill && skill.name ? skill.name : "";
  }

  addSkillWithTopics() {
    if (this.selectedSkillId && this.selectedTopics.length) {
      this.sections[this.selectedSkillId] = [...this.selectedTopics];
    } else if (
      this.selectedSkillId &&
      this.sections[this.selectedSkillId] &&
      !this.selectedTopics.length
    ) {
      delete this.sections[this.selectedSkillId];
    }
    this.updatePrimarySkillValidator();
    this.selectedSkillId = null;
    this.skillControl.reset();
    this.selectedTopics = [];
  }

  isAddSkillButtonDisabled() {
    let savedTopics = "";
    if (this.selectedSkillId && this.sections[this.selectedSkillId]) {
      savedTopics = this.sections[this.selectedSkillId]
        .slice()
        .sort()
        .toString();
    }
    const newSelectedTopics = this.selectedTopics.slice().sort().toString();
    return savedTopics === newSelectedTopics;
  }



  isAccountFit(): boolean {
    const selectedFitmentTypeId = this.getActiveForm().get("fitmentType")?.value;
    if (!selectedFitmentTypeId) return false;

    const selectedFitmentType = this.fitmentType.find(
      (type) => type.id === selectedFitmentTypeId,
    );
    return selectedFitmentType?.name === "Account Fit";
  }

  onPrimarySkillChange(): void {
    if (!this.selectedSkillId) return;

    if (this.primarySkillId === this.selectedSkillId) {
      this.primarySkillId = null;
    } else {
      this.primarySkillId = this.selectedSkillId;
    }
    this.updatePrimarySkillValidator();
    const activeForm = this.getActiveForm();
    activeForm.patchValue(
      { name: this.getGeneratedProfileName() },
      { emitEvent: false }
    );
  }

  getGeneratedProfileName(): string {
    const activeForm = this.getActiveForm();
    const fitmentTypeId = activeForm.get("fitmentType")?.value;
    const clients = activeForm.get("clients")?.value;
    const position = activeForm.get("position")?.value;
    if (!fitmentTypeId || !position || !this.primarySkillId) {
      return "";
    }
    const fitmentTypeName = this.fitmentType
      .find(type => type.id === fitmentTypeId)
      ?.name.replace(/\s+/g, "");
    const clientName = this.clients
      .find((type) => type.id === clients)
      ?.name;
    const primarySkillName = this.getSkillName(this.primarySkillId);
    if (!fitmentTypeName || !primarySkillName) {
      return "";
    }
    if (fitmentTypeName === 'AccountFit' && clientName) {
      return `${fitmentTypeName}_${clientName}_${position}_${primarySkillName}`;
    }
    return `${fitmentTypeName}_${position}_${primarySkillName}`;
  }

  validateFitmentContext() {
    [this.form, this.editForm].forEach(form => {
      form.get("fitmentType")?.valueChanges.subscribe(value => {
        const contextControl = form.get("clients");
        const selectedFitment = this.fitmentType.find(
          type => type.id === value
        );
        if (selectedFitment?.name === "Account Fit") {
          contextControl?.setValidators([Validators.required]);
        } else {
          contextControl?.clearValidators();
        }
        contextControl?.updateValueAndValidity();
      });
    });
  }

  openDeletePopup(profile: Profile) {
    const dialogData: DialogData = {
      title: "Delete Profile",
      message: `Are you sure you want to delete this profile? This action cannot be undone.`,
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
        this.loaderService.start();
        this.dialogError = null;
        this.profileService.delete(profile.profileId || 0).subscribe({
          next: (res: any) => {
            this.fetchProfiles();
            this.loaderService.stop();
            this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
            this.dialog.closeAll();
          },
          error: (err) => {
            console.error('Error while deleting profile:', err);
            this.toastr.error('Failed to delete profile', 'Error');
            this.loaderService.stop();
          },
        });
      }
      this.dialog.closeAll();
    });
  }

}
