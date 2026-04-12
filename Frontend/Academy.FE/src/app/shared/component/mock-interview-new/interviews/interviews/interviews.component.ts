import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  AfterViewInit,
  ViewChildren,
  QueryList,
} from "@angular/core";
import { DatePipe, CommonModule } from "@angular/common";
import { TitleCasePipe } from "@angular/common";
import { MatDialog } from "@angular/material/dialog";
import { TOASTER_MESSAGES } from '@shared/constants/app.constants';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  FormControl,
  FormsModule,
  FormArray,
  AbstractControl,
  ValidationErrors,
} from "@angular/forms";
import { DeleteinterviewComponent } from "../deleteinterview/deleteinterview.component";
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
import { MatSelectModule } from "@angular/material/select";
import { MatButtonToggleModule } from "@angular/material/button-toggle";
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import {
  Profile,
  ProfileService,
} from "../../../../../services/profile.service";
import {
  Interview,
  InterviewsService,
} from "../../../../../services/interviews.service";
import { DialogData } from "../../common-dialog/models/dialog-data.model";
import { CommonDialogComponent } from "../../common-dialog/common-dialog.component";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatIconModule } from "@angular/material/icon";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { AutocompleteService } from "../../../../../services/autocomplete.service";
import { environment } from '@environments/environment';
import { InterviewFilter } from "@shared/dto/interviewdetails-response";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from "@angular/material/core";
import { MAT_MOMENT_DATE_ADAPTER_OPTIONS, MomentDateAdapter } from "@angular/material-moment-adapter";
import { MY_UTC_FORMATS } from "@shared/constants/reporting.constants";

import {
  debounceTime,
  distinctUntilChanged,
  finalize,
  map,
  Observable,
  of,
  startWith,
  switchMap,
  tap,
} from "rxjs";
import { AcademyHttpService } from "../../../../../services/academy-http.service";
import { DataRequestOptions, FilterOption } from "../../../../dto/data-request-options.dto";
import { LoaderService } from "../../../../../services/loader.service";
import { Router } from "@angular/router";
import { MatMenuModule, MatMenuTrigger } from "@angular/material/menu";
import { FilterDataDto } from "@shared/dto/filter-data-dto";
import { ToastrService } from "ngx-toastr";
import { SkillsServiceService } from "@services/skills.service";
import { MatTooltipModule } from "@angular/material/tooltip";
import { ModalConfirmDialogComponent } from "@shared/component/modal-confirm-dialog/modal-confirm-dialog.component";
import { DialogService } from "@services/dialog.service";
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { DataService, FitmentType } from "@services/data.service";
import { FlatpickrDirective, provideFlatpickrDefaults } from 'angularx-flatpickr';

@Component({
  selector: "app-interviews",
  standalone: true,
  imports: [
    CommonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatAutocompleteModule,
    DeleteinterviewComponent,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatInputModule,
    MatSelectModule,
    MatButtonToggleModule,
    ReactiveFormsModule,
    FormsModule,
    MatMenuModule,
    MatChipsModule,
    MatDatepickerModule,
    MatSnackBarModule,
    FlatpickrDirective
  ],
  providers: [
    { provide: DateAdapter, useClass: MomentDateAdapter },
    { provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: true } },
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    { provide: MAT_DATE_FORMATS, useValue: MY_UTC_FORMATS },
    provideFlatpickrDefaults({
      enableTime: true,
      dateFormat: 'Y-m-d H:i',
    })
  ],
  templateUrl: "./interviews.component.html",
  styleUrl: "./interviews.component.scss",
})
export class InterviewsComponent implements OnInit, AfterViewInit {
  @ViewChild("addInterviewTemplate") addInterviewTemplate!: TemplateRef<any>;
  @ViewChild("editInterviewTemplate") editInterviewTemplate!: TemplateRef<any>;
  @ViewChild("shareInterviewTemplate")
  shareInterviewTemplate!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

  pagedInterviews: Interview[] = []; // Only the current page
  pageIndex = 0;

  form: FormGroup;
  editForm: FormGroup;
  shareForm: FormGroup;
  dialogError: string | null = null;

  // Data for form
  candidates: any[] = [];
  profiles: Profile[] = [];
  selectedCandidate: any | null = null;
  selectedProfile: Profile | null = null;
  interviewType: string = "ASSIGNED";

  interviews: Interview[] = [];
  dataSource = new MatTableDataSource<Interview>([]);
  noCandidateFound = false;
  displayedColumns: string[] = [
    "id",
    "glober",
    "profile",
    "assignedBy",
    "status",
    "actions",
  ];
  error: string | null = null;
  isDataLoaded = false;
  searchText = "";
  // Pagination properties
  isStartDateSelected: boolean = false;
  isDateFilterApplied: boolean = false;
  pageSize = 8;
  totalItems = 0;
  sortByStatusValue = "assigned";
  showAddInterview = false;
  showEditInterview = false;
  showDeleteInterview = false;
  interviewToEdit: Interview | null = null;
  interviewToDelete: Interview | null = null;
  candidateControl = new FormControl<any>(null, Validators.required);
  profileControl = new FormControl<Profile | null>(null, Validators.required);
  filteredCandidates!: Observable<any[]>;
  filteredProfiles!: Observable<any[]>;
  request = new DataRequestOptions();
  initialCandidates: any[] = [];
  isInvalidCandidate = false;
  lastFetchedCandidates: any[] = [];
  filteredInterviews: Interview[] = [];
  seniorities: any[] = [];
  communities: any[] = [];
  tdcs: any[] = [];
  skills: any[] = [];
  positions: any[] = [];
  evalutionTypes: any[] = [];
  filterValues: FilterDataDto[] = [];
  interviewFilter: InterviewFilter = {
    interviewStartDate: null,
    interviewEndDate: null
  }

  public columns = [
    {
      colName: "TDC",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "TDC",
    },
    {
      colName: "Community",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "Community",
    },
    {
      colName: "Evaluation types",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "EvalutionTypes",
    },
    {
      colName: "Position",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "Position",
    },
    {
      colName: "Seniority",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "Seniority",
    },
    {
      colName: "Skills",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "Skills",
    },
  ];
  compareFilters = (
    o1: { column: string; value: string },
    o2: { column: string; value: string },
  ): boolean => {
    if (!o1 || !o2) return false;

    return o1.column === o2.column && o1.value === o2.value;
  };

  selectedFilterColumns: string[] = [];
  groupedFilterValues: {
    [key: string]: { value: string }[];
  } = {};
  expandedGroups: { [key: string]: boolean } = {};
  selectedValues: { column: string; value: string }[] = [];
  showAccountDropdown = false;
  accountOptions: string[] = [];
  selectedAccounts: string[] = [];
  advanceFilterCriteria: any = [];
  fitmentType: FitmentType[] = [];
  evaluationTypeResponse: any[] = [];
  minDate: Date | string = 'today';
  constructor(
    private interviewsService: InterviewsService,
    private profileService: ProfileService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private autoCompleteService: AutocompleteService,
    private readonly academyHttpService: AcademyHttpService,
    public loaderService: LoaderService,
    private router: Router,
    private readonly toastr: ToastrService,
    private skillsService: SkillsServiceService,
    private dialogService: DialogService,
    private snackBar: MatSnackBar,
    private dataService: DataService,
  ) {
    this.form = this.fb.group({
      interviewType: [null, Validators.required],
      selectedCandidate: [null, Validators.required],
      selectedProfile: [null, Validators.required],
      interviewDateTime: [null, Validators.required],
      ccEmail: [null],
    });
    this.editForm = this.fb.group({
      interviewType: [null, Validators.required],
      selectedCandidate: this.candidateControl,
      selectedProfile: this.profileControl,
      status: [""],
      sectionStatus: [""],
      interviewCode: [""],
      interviewDateTime: [null, Validators.required],
      ccEmail: [null],
    });
    this.shareForm = new FormGroup({
      email: new FormArray([
        new FormControl("", [Validators.required, Validators.email, this.companyEmailValidator('globant.com')]),
      ]),
    });
  }

  ngOnInit() {
    this.fetchInterviews();
    this.loadInitialCandidates();
    this.setupAutocomplete();
    this.loadProfiles();
    this.fetchFitmentType();
    this.selectedFilterColumns.forEach((column) => {
      this.expandedGroups[column] = true;
    });
    this.getEvaluationTypesResponse();
  }

  ngAfterViewInit(): void {
    this.paginatorList.changes.subscribe((paginators) => {
      if (paginators.first) {
        this.dataSource.paginator = paginators.first;
      }
    });
  }

  getEvaluationTypesResponse() {
    this.loaderService.start();
    this.interviewsService
      .fetchEvalutionTypes()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response) {
            response.map((fitmentTypeObj: any) => {
              this.evaluationTypeResponse.push({ Id: fitmentTypeObj?.id, Text: fitmentTypeObj?.name });
            });
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  get Emails() {
    return this.shareForm.get("email") as FormArray;
  }
  addEmail() {
    this.Emails.push(
      new FormControl("", [Validators.required, Validators.email, this.companyEmailValidator('globant.com')]),
    );
  }
  removeEmail(index: number) {
    this.Emails.removeAt(index);
  }
  companyEmailValidator(domain: string) {
    return (control: AbstractControl): ValidationErrors | null => {
      const email = control.value;
      if (!email) return null;
      if (control.hasError('email')) return null;
      const domainPart = email.substring(email.lastIndexOf('@') + 1);
      return domainPart.toLowerCase() === domain.toLowerCase()
        ? null
        : { invalidCompanyEmail: true };
    };
  }

  ngAfterViewChecked() {
    if (this.sort && this.dataSource.sort !== this.sort) {
      this.dataSource.sort = this.sort;
    }
    if (this.paginator && this.dataSource.paginator !== this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    this.dataSource.sortingDataAccessor = (item: any, property: string) => {
      switch (property) {
        case "glober":
          return item.candidate?.name?.toLowerCase() || "";
        default:
          return item[property];
      }
    };
  }

  loadInitialCandidates() {
    this.academyHttpService.fetchTrackerList(this.request).subscribe({
      next: (res: any) => {
        this.initialCandidates = res.data?.items || [];
        this.candidateControl.setValue(this.candidateControl.value || "");
      },
      error: () => {
        this.initialCandidates = [];
      },
    });
  }

  setupAutocomplete() {
    this.filteredCandidates = this.candidateControl.valueChanges.pipe(
      startWith(""),
      debounceTime(500),
      distinctUntilChanged(),
      switchMap((value: any) => {
        // 🔹 Case 1: option selected (object)
        if (typeof value !== "string") {
          this.isInvalidCandidate = false;
          return of<any[]>([]);
        }

        const searchText = value.trim();

        // 🔹 Case 2: empty input
        if (!searchText) {
          this.isInvalidCandidate = false;
          return of(this.initialCandidates);
        }

        // 🔹 Case 3: API search
        return this.academyHttpService
          .fetchTrackerList({
            ...this.request,
            SearchText: searchText,
            PagingOptions: { PageIndex: 0, PageSize: 20 },
          })
          .pipe(
            map((res: any) => res.data?.items ?? []),
            tap((list: any[]) => {
              this.lastFetchedCandidates = list;
              this.isInvalidCandidate = !list.some(
                (c) =>
                  c.employeeName.toLowerCase() === searchText.toLowerCase(),
              );
            }),
          );
      }),
    );
  }

  loadProfiles() {
    this.profileService.getAll().subscribe({
      next: (data) => {
        this.profiles = data || [];

        this.filteredProfiles = this.autoCompleteService.setupFilter(
          this.profileControl,
          this.profiles,
          "profileName",
        );
      },
      error: (err) => {
        console.error("Failed to load profiles:", err);
        this.profiles = [];
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

  fetchInterviews() {
    this.loaderService.start();
    this.error = null;
    this.isDataLoaded = false;
    this.interviewsService.getAll().subscribe({
      next: (data) => {
        this.interviews = data.map((item) => ({
          ...item,
          skills: this.getSkillsByProfileId(item.profile?.id),
          profileImage:
            this.getImageByEmployeeId(item.candidate?.id) !== null
              ? this.getImageByEmployeeId(item.candidate?.id)
              : "/assets/images/default_avatar.png",
        }));
        this.totalItems = this.interviews.length;
        this.updatePagedInterviews();
        this.dataSource.data = data;
        this.dataSource.paginator = this.paginator;

        this.dataSource.sort = this.sort;

        this.totalItems = data.length;
        this.isDataLoaded = true;
        this.loaderService.stop();
        this.filterTracker();
      },
      error: (err) => {
        console.error("Error loading evaluations:", err);

        if (err.message && err.message.includes("parsing")) {
          this.error =
            "Backend data integrity issue detected. Please check the database for invalid candidate references and clean up the data.";
        } else {
          this.error = `Failed to load evaluations: ${err.message || err.status || "Unknown error"
            }`;
        }

        this.isDataLoaded = true;
        this.loaderService.stop();
        this.interviews = [];
        this.dataSource.data = [];
      },
    });
  }

  getSkillsByProfileId(profileId: number) {
    return (
      this.profiles
        .find((p) => p.profileId === profileId)
        ?.skillsAndSections?.map((skill) => ({
          id: skill.skillId,
          name: skill.skillName,
        })) || []
    );
  }

  getImageByEmployeeId(employeeId: number): string | null {
    const employee = this.initialCandidates.find(
      (e) => e.employeeId === employeeId,
    );
    return employee ? employee.image : null;
  }

  onCreateInterview() {
    this.form.reset();
    this.candidateControl.reset();
    this.profileControl.reset();
    this.form.patchValue({
      interviewType: null,
      selectedCandidate: null,
      selectedProfile: null,
      interviewDateTime: null,
      ccEmail: null,
    });

    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Schedule Evaluation",
      message: "",
      confirmText: "Schedule",
      cancelText: "Cancel",
      isInvalidCandidateRef: () => this.isInvalidCandidate,
      showActions: true,
      form: this.form,
      template: this.addInterviewTemplate,
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

  onEditInterview(interview: Interview) {
    this.interviewToEdit = interview;
    this.editForm.reset();

    // Populate the edit form with interview data

    const selectedFitment = this.fitmentType.find(
      f => f.id === interview.profile.fitmentTypeId
    );

    if (selectedFitment) {
      const prefix = selectedFitment.name.replace(/\s/g, '');

      const filtered = this.profiles.filter(profile => {
        const profilePrefix = profile.profileName?.split('_')[0];
        return profilePrefix === prefix;
      });

      this.filteredProfiles = this.autoCompleteService.setupFilter(
        this.profileControl,
        filtered,
        'profileName'
      );
    }

    const scheduledDate = interview?.scheduledAt ? new Date(interview?.scheduledAt) : null;
    this.minDate = scheduledDate && scheduledDate < new Date() ? scheduledDate : 'today';

    this.candidateControl.setValue(interview.candidate);
    this.profileControl.setValue(interview.profile);
    this.editForm.patchValue({
      interviewType: interview.profile.fitmentTypeName,
      status: interview.status || "",
      sectionStatus:
        typeof interview.sectionStatus === "string"
          ? interview.sectionStatus
          : JSON.stringify(interview.sectionStatus || "", null, 2),
      interviewCode: interview.interviewCode || "",
      selectedProfile: interview.profile,
      interviewDateTime: scheduledDate,
    });

    // OPEN dialog **after** candidate is loaded
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Edit Evaluation",
      message: "",
      confirmText: "Save",
      cancelText: "Cancel",
      showActions: true,
      form: this.editForm,
      template: this.editInterviewTemplate,
      isInvalidCandidateRef: () => this.isInvalidCandidate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "600px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.onSubmitEditDialog();
      this.onCancelDialog();
    });
  }

  onShareInterview(interview: Interview) {
    this.shareForm.reset();
    // OPEN dialog **after** candidate is loaded
    this.dialogError = null;

    const dialogData: DialogData = {
      title: "Share Evaluation",
      message: "",
      confirmText: "Share",
      cancelText: "Cancel",
      showActions: true,
      form: this.shareForm,
      template: this.shareInterviewTemplate,
      isInvalidCandidateRef: () => this.isInvalidCandidate,
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "600px",
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.onSubmitShareInterviewDialog(interview);
      this.onCancelShareInterviewDialog();
    });
  }

  onCancelShareInterviewDialog() {
    this.dialog.closeAll();
    const formArray = this.shareForm.get("email") as FormArray;
    const firstControl = formArray.at(0);
    formArray.clear();
    formArray.push(firstControl);
  }

  onSubmitShareInterviewDialog(interview: Interview) {
    const emails = this.shareForm.value.email.filter((e: string) => e);
    if (emails.length === 0) {
      console.warn("No valid email addresses entered.");
      return;
    }
    const payload = {
      toEmail: emails.join(","),
      name: interview.candidate.name,
      skills: this.getSkillNames(interview?.profile?.skillsAndSections),
      dateTime: interview?.createdAt,
      interviewUrl: interview?.interviewLink,
      evaluationType: interview?.profile?.fitmentTypeName,
      evaluationId: interview?.profile?.fitmentTypeId,
      comments: interview.comments,
      score: interview.score,
      outof: interview.totalScore,
      videoLink:
        environment.academyBaseUrl +
        "view-interview/" +
        interview?.interviewCode,
    };
    this.interviewsService.shareInterviewDetails(payload).subscribe({
      next: (res) => {
        if (res) {
          this.loaderService.stop();
          this.dialog.closeAll();
        }
      },
      error: (error) => {
        ((this.dialogError = "Failed to update evaluation"), error);
        this.loaderService.stop();
      },
    });
  }

  getSkillNames(skills: any[] | undefined): string {
    if (!skills || skills.length === 0) {
      return "";
    }
    return skills.map((skill) => skill.skillName).join(", ");
  }

  getCandidateDetails(candidateId: number) {
    return this.academyHttpService
      .fetchEmployeeDashboard(candidateId)
      .pipe(map((response: any) => response.data.employee));
  }

  onBackToList() {
    this.showAddInterview = false;
    this.showEditInterview = false;
    this.showDeleteInterview = false;
    this.interviewToEdit = null;
    this.interviewToDelete = null;
    this.fetchInterviews();
  }

  onSubmitDialog(): void {
    if (this.form.invalid) return;

    this.dialogError = null;

    const formValue = this.form.value;
    const date = new Date(formValue.interviewDateTime);

    const formattedDate =
      date.getFullYear() + "-" +
      String(date.getMonth() + 1).padStart(2, "0") + "-" +
      String(date.getDate()).padStart(2, "0") + "T" +
      String(date.getHours()).padStart(2, "0") + ":" +
      String(date.getMinutes()).padStart(2, "0") + ":" +
      String(date.getSeconds()).padStart(2, "0") +
      ".000Z";
    const interview = {
      profileId: formValue.selectedProfile?.profileId,
      candidateId: formValue.selectedCandidate?.id,
      candidate: {
        id: formValue.selectedCandidate?.id,
        name: formValue.selectedCandidate?.name,
        email: formValue.selectedCandidate?.email,
      },
      scheduleDateTime: formattedDate,
      ccEmailIds: formValue.ccEmail,
    };
    this.loaderService.start();

    this.interviewsService.create(interview).subscribe({
      next: (res: any) => {
        this.loaderService.stop();
        this.toastr.success(res?.message || TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
        this.dialog.closeAll();
        this.fetchInterviews();
      },
      error: (err) => {
        this.loaderService.stop();
        const status = err?.status;
        const msg = err?.error?.message || err?.message || '';
        if (status === 409 || msg.toLowerCase().includes('already scheduled') || msg.toLowerCase().includes('duplicate')) {
          this.dialogError = 'An evaluation is already scheduled for this profile. Please select a different profile or candidate.';
        } else {
          this.dialogError = 'Failed to schedule evaluation. Please try again.';
        }
      },
    });
  }

  onSubmitEditDialog(): void {
    if (this.editForm.invalid || !this.interviewToEdit) return;

    this.dialogError = null;

    const formValue = this.editForm.value;
    let sectionStatus = formValue.sectionStatus;

    if (typeof sectionStatus === "string" && sectionStatus.trim()) {
      try {
        sectionStatus = JSON.parse(sectionStatus);
      } catch (e) { }
    }

    const date = new Date(formValue.interviewDateTime);
    const formattedDate =
      date.getFullYear() + "-" +
      String(date.getMonth() + 1).padStart(2, "0") + "-" +
      String(date.getDate()).padStart(2, "0") + "T" +
      String(date.getHours()).padStart(2, "0") + ":" +
      String(date.getMinutes()).padStart(2, "0") + ":" +
      String(date.getSeconds()).padStart(2, "0") +
      ".000Z";

    const updatedInterview: Interview = {
      candidate: {
        id: formValue.selectedCandidate?.id,
        name: formValue.selectedCandidate?.name,
        email: formValue.selectedCandidate?.email,
      },
      profileId: formValue.selectedProfile.profileId,
      status: formValue.status,
      candidateId: this.interviewToEdit.candidate.id,
      interviewCode: formValue.interviewCode,
      scheduleDateTime: formattedDate,
    };

    this.loaderService.start();
    this.interviewsService
      .update(this.interviewToEdit.id!, updatedInterview)
      .subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message || TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
          this.loaderService.stop();
          this.dialog.closeAll();
          this.fetchInterviews();
        },
        error: () => {
          this.dialogError = "Failed to update evaluation";
          this.loaderService.stop();
        },
      });
  }

  onCancelDialog(): void {
    this.isInvalidCandidate = false;
    this.dialog.closeAll();
  }

  onInterviewTypeChange(val: any): void {
    const selectedFitment = this.fitmentType.find(f => f.id === val);

    if (!selectedFitment) {
      this.filteredProfiles = of([]);
      return;
    }
    const fitmentPrefix = selectedFitment.name.replace(/\s/g, '');

    const filtered = this.profiles.filter(profile => {
      if (!profile.profileName) return false;

      const prefix = profile.profileName.split('_')[0];
      return prefix === fitmentPrefix;
    });

    this.filteredProfiles = this.autoCompleteService.setupFilter(
      this.profileControl,
      filtered,
      'profileName'
    );

    // Reset selected profile
    this.profileControl.reset();
  }

  // New methods for Material Select components
  onCandidateSelectionChange(candidate: any): void {
    const payload = {
      email: candidate.employeeEmail,
      id: candidate.employeeId,
      name: candidate.employeeName,
    };
    this.isInvalidCandidate = false;
    this.selectedCandidate = payload;

    this.form.patchValue({ selectedCandidate: payload });
  }

  displayCandidate(candidate: any): string {
    return candidate && candidate.employeeName ? candidate.employeeName : "";
  }

  displayCandidateEdit(candidate: any): string {
    return candidate && candidate.name ? candidate.name : "";
  }

  onProfileSelectionChange(profile: Profile): void {
    const profileId = profile.profileId;
    const newProfile = profileId
      ? this.profiles.find((p) => p.profileId == profileId) || null
      : null;
    this.selectedProfile = newProfile;
    this.form.patchValue({ selectedProfile: newProfile });
  }

  displayProfile(profile: Profile): string {
    return profile && profile.profileName ? profile.profileName : "";
  }

  onEditProfileSelectionChange(profile: Profile): void {
    const profileId = profile.profileId;
    const newProfile = profileId
      ? this.profiles.find((p) => p.profileId == profileId) || null
      : null;
    this.selectedProfile = newProfile;
    this.editForm.patchValue({ selectedProfile: newProfile });
  }

  applyGlobalSearch() {
    this.filterTracker();
  }

  onPageChanged(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updatePagedInterviews();
  }

  updatePagedInterviews() {
    const startIndex = this.pageIndex * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.pagedInterviews = this.filteredInterviews.slice(startIndex, endIndex);
    this.totalItems = this.filteredInterviews.length; // update paginator
  }

  capitalizeFirst(value: string | null | undefined): string {
    if (!value || typeof value !== "string") return "";
    return value.charAt(0).toUpperCase() + value.slice(1).toLowerCase();
  }

  viewSummary(interviewCode: any) {
    this.router.navigate(["view-interview", interviewCode]);
  }

  onFilterOptionChanged(ev: any) {

    const newColumns: string[] = ev.value || [];
    this.selectedFilterColumns = newColumns;

    const hasEvaluationType = newColumns.includes('EvalutionTypes');

    // Remove filters for columns not selected anymore
    this.request.FilterOptions =
      (this.request.FilterOptions || [])
        .filter((f: any) => newColumns.includes(f.FilterBy));

    // If EvalutionTypes removed → also clear Account
    if (!hasEvaluationType) {

      this.showAccountDropdown = false;
      this.selectedAccounts = [];

      this.request.FilterOptions =
        this.request.FilterOptions
          .filter((f: any) => f.FilterBy !== 'Account');
    }

    // Sync selectedValues
    this.selectedValues =
      this.selectedValues
        .filter(sel => newColumns.includes(sel.column));

    // Reset group UI
    this.groupedFilterValues = {};
    this.expandedGroups = {};

    newColumns.forEach((col) => {
      this.expandedGroups[col] = true;
      this.fetchFilterValues(col);
    });
  }

  onFilterValueChanged(ev: any) {

    const selectedValues = ev?.value || [];

    if (!this.request.FilterOptions) {
      this.request.FilterOptions = [];
    }

    // Step 1: Group selected values by column
    const grouped: { [key: string]: string[] } = {};

    selectedValues.forEach((item: any) => {
      if (!grouped[item.column]) {
        grouped[item.column] = [];
      }
      grouped[item.column].push(item.value);
    });

    // Step 2: Remove old filters ONLY for affected columns
    const affectedColumns = Object.keys(grouped);

    this.request.FilterOptions =
      this.request.FilterOptions
        .filter((f: any) => !affectedColumns.includes(f.FilterBy));

    // Step 3: Add new filters
    Object.keys(grouped).forEach(column => {
      grouped[column].forEach(value => {
        const filterOption = new FilterOption();
        filterOption.FilterBy = column;
        filterOption.FilterValue = value;
        this.request.FilterOptions.push(filterOption);
      });
    });

    // Step 4: Handle Account dependency
    const selectedEvalTypes = grouped['EvalutionTypes'] || [];
    const hasAccountFit = selectedEvalTypes.includes('Account Fit');

    if (!hasAccountFit) {
      this.request.FilterOptions =
        this.request.FilterOptions
          .filter((f: any) => f.FilterBy !== 'Account');
      this.showAccountDropdown = false;
    } else {
      this.showAccountDropdown = true;
      this.fetchAllAccounts();
    }

  }

  private fetchMap: { [key: string]: () => void } = {
    Seniority: () => this.fetchSeniority(),
    Position: () => this.fetchPositions(),
    Community: () => this.fetchCommunity(),
    TDC: () => this.fetchTdc(),
    Skills: () => this.fetchSkills(),
    EvalutionTypes: () => this.fetchEvalutionTypes(),
  };

  fetchFilterValues(column: string) {
    this.fetchMap[column]?.();
  }

  fetchSeniority() {
    if (this.seniorities.length) {
      this.groupedFilterValues["Seniority"] = this.mapToFilterData(
        this.seniorities,
        "level",
        "name",
        "Seniority",
      );
      return;
    }

    this.loaderService.start();
    this.academyHttpService
      .fetchSeniorities()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res.status === 200) {
            this.seniorities = res.data;
            this.groupedFilterValues["Seniority"] = this.mapToFilterData(
              this.seniorities,
              "level",
              "name",
              "Seniority",
            );
          } else {
            this.toastr.error(res.errorMessage, "Error");
          }
        },
      });
  }

  toggleGroup(column: string, event: Event) {
    event.stopPropagation(); // keep dropdown open
    this.expandedGroups[column] = !this.expandedGroups[column];
  }

  fetchPositions() {
    this.positions = this.profiles || [];
    this.groupedFilterValues["Position"] = this.mapToFilterData(
      this.positions,
      "profileId",
      "position",
      "Position",
    );
  }

  fetchCommunity() {
    if (this.communities.length) {
      this.groupedFilterValues["Community"] = this.mapSimpleArray(
        this.communities,
        "Community",
      );
      return;
    }

    this.loaderService.start();
    this.academyHttpService
      .fetchAllCommunity()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res.status === 200) {
            this.communities = res.data;
            this.groupedFilterValues["Community"] = this.mapSimpleArray(
              this.communities,
              "Community",
            );
          } else {
            this.toastr.error(res.errorMessage, "Error");
          }
        },
      });
  }

  fetchTdc() {
    if (this.tdcs.length) {
      this.groupedFilterValues["TDC"] = this.mapSimpleArray(this.tdcs, "TDC");
      return;
    }

    this.loaderService.start();
    this.academyHttpService
      .fetchAllTdc()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res.status === 200) {
            this.tdcs = res.data;
            this.groupedFilterValues["TDC"] = this.mapSimpleArray(
              this.tdcs,
              "TDC",
            );
          } else {
            this.toastr.error(res.errorMessage, "Error");
          }
        },
      });
  }

  fetchSkills() {
    if (this.skills.length) {
      this.groupedFilterValues["Skills"] = this.mapToFilterData(
        this.skills,
        "id",
        "name",
        "Skills",
      );
      return;
    }

    this.loaderService.start();
    this.skillsService
      .getAll()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          this.skills = res;
          this.groupedFilterValues["Skills"] = this.mapToFilterData(
            this.skills,
            "id",
            "name",
            "Skills",
          );
        },
      });
  }

  fetchEvalutionTypes() {
    this.evalutionTypes = this.evaluationTypeResponse || [];
    this.groupedFilterValues["EvalutionTypes"] = this.mapToFilterData(
      this.evalutionTypes,
      "Id",
      "Text",
      "EvalutionTypes",
    );
  }

  private mapToFilterData(
    array: any[],
    idKey: string,
    valueKey: string,
    type: string,
  ): FilterDataDto[] {
    return array.map((item) => ({
      id: item[idKey],
      value: item[valueKey],
      type,
    }));
  }

  private mapSimpleArray(array: any[], type: string): FilterDataDto[] {
    return array.map((item) => ({
      id: item,
      value: item,
      type,
    }));
  }

  resetFilters() {
    // Clear UI selections
    this.selectedFilterColumns = [];
    this.selectedValues = [];

    // Clear backend request model
    if (this.request) {
      this.request.FilterOptions = [];
    }

    // Reset expansion state
    this.expandedGroups = {};
    this.filterTracker();
    this.showAccountDropdown = false;
    this.selectedAccounts = [];
  }

  private columnKeyMap: { [key: string]: string } = {
    Seniority: "profile.seniorityName", // profile -> seniorityname
    Position: "profile.position", // profile
    Community: "community", // to be added
    TDC: "tdc", //to be added 
    Account: "profile.clientName", //profile -> client
    Skills: "profile.skillsAndSections", //profile
    EvalutionTypes: "profile.fitmentTypeName", //profile
  };

  getNestedValue(obj: any, path: string) {
    return path.split('.').reduce((acc, part) => acc?.[part], obj);
  }

  setAppliedFilters() {
    let appliedFilters = [...this.request?.FilterOptions];
    let filterObj: { [key: string]: string[] } = {};
    appliedFilters.forEach((item) => {
      filterObj[item.FilterBy] ??= [];
      filterObj[item.FilterBy].push(item.FilterValue);
    });

    this.advanceFilterCriteria = Object.keys(filterObj).map(key => ({
      name: key,
      chips: filterObj[key]
    }));
  }


  onInterviewStartDateChange() {
    this.isStartDateSelected = true;
    this.filterTracker();
  }

  onInterviewEndDateChange() {
    this.filterTracker();
  }

  clearDateFilter() {
    this.interviewFilter = {
      interviewStartDate: null,
      interviewEndDate: null
    };

    this.isStartDateSelected = false;
    this.filterTracker();

    if (this.paginator) {
      this.paginator.firstPage();
    }
  }

  onAccountChanged(event: any) {
    this.selectedAccounts = event?.value || [];
    // Remove old Account entries
    this.request.FilterOptions = this.request.FilterOptions.filter(
      (f) => f.FilterBy !== "Account",
    );
    // Add new selected accounts
    this.selectedAccounts.forEach((acc) => {
      const filterOption = new FilterOption();
      filterOption.FilterBy = "Account";
      filterOption.FilterValue = acc;
      this.request.FilterOptions.push(filterOption);
    });

  }

  fetchAllAccounts() {
    this.loaderService.start();
    this.interviewsService
      .fetchAllAccounts()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response) {
            this.accountOptions = [];
            response.map((accountTypeObj: any) => {
              this.accountOptions.push(accountTypeObj.name);
            });
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  matchesFilterValue(itemValue: any, filterValue: any): boolean {
    if (!itemValue) return false;
    // Case 1: Array of objects (skillsAndSections)
    if (Array.isArray(itemValue)) {
      return itemValue.some((obj: any) => {

        // If skillName exists
        if (obj.skillName) {
          return obj.skillName
            .toLowerCase()
            .includes(filterValue.toLowerCase());
        }

        return false;

      });
    }
    // Case 2: Simple string
    return itemValue
      .toString()
      .toLowerCase()
      .includes(filterValue.toLowerCase());
  }

  filterCardsByStatus(status: string) {
    this.filteredInterviews = this.interviews.filter((interview) => {
      return interview.status?.toLowerCase() === status.toLowerCase();
    });

    const filter = this.searchText.trim().toLowerCase();
    if (!filter) {
      this.pageIndex = 0; // Reset pagination to first page
      this.updatePagedInterviews();
    } else {
      this.applyGlobalSearch(); //apply global search filter
    }
  }

  filterByStatus(data: any) {
    this.sortByStatusValue = data?.value;
    this.filterTracker();
  }

  clearSearch() {
    this.searchText = "";
    this.filterTracker();
  }

  onApplyFilters(trigger: MatMenuTrigger) {
    this.filterTracker();
    trigger.closeMenu();
  }

  filterTracker() {
    if (!this.interviews) return;
    if (!this.interviews.length) {
      this.filteredInterviews = [];
      this.updatePagedInterviews();
      return;
    }

    let data = [...this.interviews];

    /* =========================
       Advanced Filters
    ==========================*/
    if (this.request?.FilterOptions?.length) {

      this.setAppliedFilters();

      const groupedFilters: { [key: string]: any[] } = {};

      this.request.FilterOptions.forEach((filter: any) => {
        if (!groupedFilters[filter.FilterBy]) {
          groupedFilters[filter.FilterBy] = [];
        }
        groupedFilters[filter.FilterBy].push(filter.FilterValue);
      });

      data = data.filter((item: any) => {
        return Object.keys(groupedFilters).every(column => {
          const selectedValues = groupedFilters[column];
          const key = this.columnKeyMap[column];

          // SPECIAL HANDLING FOR ACCOUNT
          if (column === 'Account') {

            const isAccountFit =
              item.profile?.fitmentTypeName === 'Account Fit';

            // If this row is NOT Account Fit → ignore Account filter
            if (!isAccountFit) {
              return true;
            }

            // Now it MUST have clientName
            const accountValue =
              this.getNestedValue(item, key);

            if (!accountValue) return false;

            return selectedValues.some(filterValue =>
              this.matchesFilterValue(accountValue, filterValue)
            );
          }

          // 🔹 Normal filtering for other columns
          const itemValue =
            this.getNestedValue(item, key);

          if (itemValue == null) return false;

          return selectedValues.some(filterValue =>
            this.matchesFilterValue(itemValue, filterValue)
          );
        });
      });

    } else {
      this.advanceFilterCriteria = [];
    }

    // Status Filter
    if (this.sortByStatusValue) {
      data = data.filter(
        (interview) =>
          interview.status?.toLowerCase() ===
          this.sortByStatusValue.toLowerCase()
      );
    }

    // Date Range Filter
    const startDate = this.interviewFilter.interviewStartDate
      ? new Date(this.interviewFilter.interviewStartDate)
      : null;

    const endDate = this.interviewFilter.interviewEndDate
      ? new Date(this.interviewFilter.interviewEndDate)
      : null;

    if (endDate) {
      endDate.setHours(23, 59, 59, 999);
    }

    if (startDate || endDate) {
      data = data.filter((interview) => {
        if (!interview.createdAt) return false;

        const interviewDate = new Date(interview.createdAt);

        return (
          (!startDate || interviewDate >= startDate) &&
          (!endDate || interviewDate <= endDate)
        );
      });
    }

    //  Global Search
    const filterText = this.searchText?.trim().toLowerCase();

    if (filterText) {
      data = data.filter(interview => {
        const candidateName = interview.candidate?.name?.toLowerCase() || "";
        const status = interview.status?.toLowerCase() || "";
        const seniority = interview.seniority?.name?.toLowerCase() || "";
        const profileName = interview.profile?.profileName?.toLowerCase() || "";
        const skillNames = interview?.profile?.skillsAndSections?.some((skill: any) =>
          skill.skillName.toLowerCase().includes(filterText)
        ) || false;

        return (
          candidateName.includes(filterText) ||
          status.includes(filterText) ||
          seniority.includes(filterText) ||
          profileName.includes(filterText) ||
          skillNames
        );
      });
    }

    //  Final Assignment
    this.filteredInterviews = data;
    this.pageIndex = 0;
    this.updatePagedInterviews();
  }

  deleteInterview(interviewId: any) {
    const dialogData: DialogData = {
      title: "Delete Evaluation",
      message: `Are you sure you want to delete this evaluation? This action cannot be undone.`,
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
        this.interviewsService.delete(interviewId).subscribe({
          next: (res: any) => {
            this.interviews = this.interviews.filter(item => item.id !== interviewId);
            this.filterTracker();
            this.loaderService.stop();
            this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
            this.dialog.closeAll();
          },
          error: (err) => {
            console.error('Error while deleting evaluation:', err);
            this.toastr.error('Failed to delete evaluation', 'Error');
            this.loaderService.stop();
          },
        });
      }
      this.dialog.closeAll();
    });
  }

  profileName(profile: any) {
    return [
      profile?.clientName,
      profile?.position,
      profile?.primarySkillName
    ]
      .filter(Boolean)
      .join('_');
  }

  evaluationType(profile: any) {
    return profile?.fitmentTypeName ?? '';
  }

  getTopSkills(skills: any): any[] {
    if (!skills || skills.length > 3) {
      return skills?.slice(0, 3) || [];
    }
    else {
      return skills;
    }
  }

  getRemainingSkills(skills: { skillId: number, skillName: string, sections: any[] }[]): string {
    if (!skills || skills.length <= 3) {
      return '';
    }
    // Take skills after the first 3 and get their names
    const remainingSkillNames = skills.slice(3).map(skill => skill.skillName);

    return remainingSkillNames.join(', ');
  }
}