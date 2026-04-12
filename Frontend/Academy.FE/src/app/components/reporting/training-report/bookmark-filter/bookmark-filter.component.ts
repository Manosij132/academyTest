import {
  Component, DestroyRef, EventEmitter, inject, OnDestroy,
  OnInit, Output
} from '@angular/core';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxChange, MatCheckboxModule } from '@angular/material/checkbox';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import {
  Observable, debounceTime, distinctUntilChanged, startWith,
  map, finalize, Subscription
} from 'rxjs';
import { AcademyHttpService } from '@services/academy-http.service';
import { LoaderService } from '@services/loader.service';
import { BookmarkForms, EmailColumnsModel } from '@shared/dto/bookmark-form.dto';
import { ExportDetailReportMetadata } from '@shared/dto/ExportReportMetadata';
import { EnumReportType, MY_UTC_FORMATS } from '@shared/constants/reporting.constants';
import { CommonModule } from '@angular/common';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatMultiSelectComponent } from '@shared/component/mat-multi-select/mat-multi-select.component';
import { BookmarkFilterData, User, ApiResponse } from '@shared/Interface/bookmark';
import { forkJoin } from 'rxjs';
import { MatDatepickerModule } from "@angular/material/datepicker";
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MAT_MOMENT_DATE_ADAPTER_OPTIONS, MomentDateAdapter } from '@angular/material-moment-adapter';
import moment, { Moment } from 'moment';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-bookmark-filter',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatSelectModule,
    MatButtonModule,
    MatExpansionModule,
    MatCheckboxModule,
    MatMultiSelectComponent,
    MatDatepickerModule
  ],
  providers: [
    { provide: DateAdapter, useClass: MomentDateAdapter },
    { provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: true } },
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    { provide: MAT_DATE_FORMATS, useValue: MY_UTC_FORMATS },
  ],

  templateUrl: './bookmark-filter.component.html',
  styleUrl: './bookmark-filter.component.css'
})

export class BookmarkFilterComponent implements OnInit, OnDestroy {
  BookmarkRequest = new BookmarkForms();
  myTrainingReportForm!: FormGroup; // Form group for the training report
  reportTypes: any[] = []; // Report types
  tdcOptions: string[] = []; // TDC options
  communityOptions: string[] = []; // Community options
  clients: string[] = [];
  trainingOptions: any[] = []; // Training options
  seniorityOptions: any[] = []; // Seniority options
  projectOptions: string[] = []; // Project options
  statusOptions: any[] = []; // Status options
  selectColumnsOptions: any[] = []; // Select columns options
  groupByColumnsOptions: any[] = []; // Group by columns options
  areaPathOptions: any[] = []; // AreaPath options
  activityOptions: any[] = [];
  priActivityOptions: any[] = [];
  selectedAreaPath = [];
  dateTypeFiltersForActivityTypeTraining: any[] = ["Start Date", "Actual End Date", "Expected End Date", "Start Date & Actual End Date", "Start Date & Expected End Date"];
  dateTypeFiltersForActivityTypePrimary: any[] = ["Start Date", "End Date", "Start Date & End Date"];
  showDatePickers: boolean = false;
  dateTypeFilters: any[] = [];
  employeeControl = new FormControl<string | User>("");
  users: User[] = [];// Define the filteredUsers$ observable with the correct type
  filteredUsers$: Observable<User[]>;// Selected user object, initially null
  selectedUser: User | null = null;

  submitted: boolean = false; // Flag to track if the form has been submitted
  selectAllStates: { [key: string]: boolean } = {}; // Track the state of "Select All" checkboxes
  selectedCommunities: any[] = [];
  selectedPActivity: number = 1;
  showTrainingdropdown: boolean = true;
  disbaleAcitivtyTypedropdown: boolean = false;
  firstValue: number = 0;
  activitytype: string = '';
  selectColumns: any[] = [];
  groupByColumns: any[] = [];
  exportUrl: string | null = null;
  $window: Window = window;
  bookMarkId = 0;
  isSavedBookMark: boolean = false;
  sendEmailFields!: EmailColumnsModel;
  private formSubscription: Subscription;
  private destroyRef = inject(DestroyRef);
  @Output() generateDataEvent = new EventEmitter<BookmarkForms>();
  @Output() updateBookmarkId = new EventEmitter<number>();
  @Output() updateEmailFields = new EventEmitter<EmailColumnsModel>();

  constructor(private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService, private fb: FormBuilder, private route: ActivatedRoute) {

    // Initialize the form with FormBuilder
    this.myTrainingReportForm = this.fb.group({
      reportTypeSelect: ['', Validators.required],
      tdcMultiSelect: [[],],
      communityMultiSelect: [[],],
      clientMultiSelect: [[]],
      trainingsMultiSelect: [[],],
      seniorityMultiSelect: [[],],
      projectMultiSelect: [[],],
      statusMultiSelect: [[],],
      selectColumnsMultiSelect: [[], Validators.required],
      groupByColumnsMultiSelect: [[]],
      areaPathMultiSelect: [[]],
      activityTypeSelect: [[]],
      primaryactivityMultiSelect: [[],],
      employeeControl: ['',], // Employee control
      bookmarkControl: ['', Validators.required], // Bookmark control
      bookmarkIdControl: 0, // Add the bookmarkId field here
      dateTypeFilterSelect: [''],
      fromDate: [null],
      toDate: [null]
    });

    // Initialize filteredUsers$ with type-safe logic
    this.filteredUsers$ = this.employeeControl!.valueChanges.pipe(
      debounceTime(300), // Wait for 300ms between keystrokes
      distinctUntilChanged(), // Only proceed if the input has changed
      startWith(""),
      map((value: any) => (typeof value === "string" ? value : value?.employeeName)),
      map((employeeName) =>
        employeeName ? this._filterUsers(employeeName) : this.users.slice()
      )
    );
  }
  request = new ExportDetailReportMetadata();
  ngOnInit(): void {
    const bookmarkFilterData = this.route.snapshot.data['bookmarkFilterData'] as BookmarkFilterData;
    this.reportTypes = bookmarkFilterData.AllReportTypes;
    this.tdcOptions = bookmarkFilterData.AllTdc;
    this.communityOptions = bookmarkFilterData.AllCommunitySettings;
    this.clients = bookmarkFilterData.AllClients;
    this.trainingOptions = bookmarkFilterData.AllTrainings;
    this.seniorityOptions = bookmarkFilterData.Seniorities;
    this.projectOptions = bookmarkFilterData.AllProjects;
    this.statusOptions = bookmarkFilterData.AllTrainingStatus;
    this.selectColumnsOptions = bookmarkFilterData.AllSelectColumns;
    this.groupByColumnsOptions = bookmarkFilterData.AllGroupByColumns;
    this.areaPathOptions = bookmarkFilterData.AllAreaPaths;
    this.priActivityOptions = bookmarkFilterData.AllActivitiesType;

    if (this.priActivityOptions.length > 0) {
      this.firstValue = this.priActivityOptions[0].primaryActivityId;
      this.myTrainingReportForm.get('activityTypeSelect')?.setValue(this.firstValue);
    }

    // Initial DateTypeFilter Value,i.e. training related values
    if (this.firstValue == 1) {
      this.onActivityTypeChange("training")
    }

    this.route.params.subscribe(params => {
      const bookmarkId = params['id']; // Replace 'id' with the actual query parameter name
      if (bookmarkId) {
        this.isSavedBookMark = true; //
        this.myTrainingReportForm.get('bookmarkIdControl')?.setValue(bookmarkId);
        this.bookMarkId = bookmarkId;
        this.editBookmark(bookmarkId); // Call a method to load the bookmark data
      }
    });

    // Subscribe to Areapath dropdown changes
    this.myTrainingReportForm.get('areaPathMultiSelect')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(selectedAreaPaths => {
        if (selectedAreaPaths?.length > 0) {
          this.selectedAreaPath = selectedAreaPaths;
          this.showTrainingdropdown = true;
          this.disbaleAcitivtyTypedropdown = true;
          this.myTrainingReportForm.get('trainingsMultiSelect')?.setValue([]);
          this.myTrainingReportForm.get('selectColumnsMultiSelect')?.setValue([]);
          // this.myTrainingReportForm.get('groupByColumnsMultiSelect')?.setValue([]);
          this.myTrainingReportForm.get('activityTypeSelect')?.setValue(this.firstValue, { emitEvent: false });
          //this.FetchTrainingByAreapathAndCommunity(selectedAreaPaths);
          this.evaluateSelection();
        }
        else {
          this.selectedAreaPath = []
          this.disbaleAcitivtyTypedropdown = false;
          this.myTrainingReportForm.get('trainingsMultiSelect')?.setValue([]);
          this.myTrainingReportForm.get('selectColumnsMultiSelect')?.setValue([]);
          // this.myTrainingReportForm.get('groupByColumnsMultiSelect')?.setValue([]);
          this.evaluateSelection();
          this.onChangeReportType(this.myTrainingReportForm.value.reportTypeSelect);
        }
      });

    // Subscribe to community dropdown changes
    this.myTrainingReportForm.get('communityMultiSelect')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(selectedCommunity => {
        this.selectedCommunities = selectedCommunity;
        this.myTrainingReportForm.get('primaryactivityMultiSelect')?.setValue([]);
        this.evaluateSelection();
      });

    // Subscribe to community dropdown changes
    this.myTrainingReportForm.get('clientMultiSelect')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(selectedClient => {
        this.fetchClientProjects(selectedClient);
      });

    // Subscribe to ActivityType changes
    this.myTrainingReportForm.get('activityTypeSelect')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(selectedActivitytype => {
        // this.myTrainingReportForm.get('groupByColumnsMultiSelect')?.reset();
        this.myTrainingReportForm.get('selectColumnsMultiSelect')?.reset();
        this.selectedPActivity = selectedActivitytype;
        this.showTrainingdropdown = selectedActivitytype == 1 ? true : false;
        this.evaluateSelection();
        if (this.showTrainingdropdown) {
          this.myTrainingReportForm.get('primaryactivityMultiSelect')?.setValue([]);
        }
        else {
          this.myTrainingReportForm.get('trainingsMultiSelect')?.setValue([]);
        }
      });
    // Subscribe to ActivityType changes disbalbe in areapath
    this.myTrainingReportForm.get('activityTypeSelect')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(value => {
        const areaPathControl = this.myTrainingReportForm.get('areaPathMultiSelect');
        if (value == 2) {
          areaPathControl?.reset();
          areaPathControl?.disable();
        } else {
          areaPathControl?.enable();
        }
      });

    this.myTrainingReportForm.get('dateTypeFilterSelect')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(value => {
        console.log('date picker subscribe')
        if (!value) {
          console.log('date picker subscribe resetting value')
          this.myTrainingReportForm.get('fromDate')?.setValue(null);
          this.myTrainingReportForm.get('toDate')?.setValue(null);
        }
      });

    this.formSubscription = this.myTrainingReportForm?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        // Reset exportUrl on any form change
        this.exportUrl = null;
      });
  }

  ngOnDestroy() {
    // Clean up subscription
    if (this.formSubscription) {
      this.formSubscription.unsubscribe();
    }
  }
  // Save bookamrks data
  onSaveBookmark(): void {
    this.submitted = true; // Set submitted flag to true
    // Check if the form is valid
    if (this.myTrainingReportForm.valid) {

      this.BookmarkRequest.BookMarkName = this.myTrainingReportForm.value.bookmarkControl;
      this.BookmarkRequest.TDC = this.myTrainingReportForm.value.tdcMultiSelect;
      this.BookmarkRequest.Community = this.myTrainingReportForm.value.communityMultiSelect;
      this.BookmarkRequest.Trainings = this.myTrainingReportForm.value.trainingsMultiSelect;
      this.BookmarkRequest.Seniorities = this.myTrainingReportForm.value.seniorityMultiSelect;
      this.BookmarkRequest.Projects = this.myTrainingReportForm.value.projectMultiSelect;
      this.BookmarkRequest.Statuses = this.myTrainingReportForm.value.statusMultiSelect;
      this.BookmarkRequest.ReportType = this.myTrainingReportForm.value.reportTypeSelect;
      this.BookmarkRequest.SelectColumns = this.myTrainingReportForm.value.selectColumnsMultiSelect;
      this.BookmarkRequest.DateTypeFilter = this.myTrainingReportForm.value.dateTypeFilterSelect;
      this.BookmarkRequest.ActivityType = this.myTrainingReportForm.value.activityTypeSelect;
      this.BookmarkRequest.FromDate = this.formatDate(this.myTrainingReportForm.value.fromDate);
      this.BookmarkRequest.ToDate = this.formatDate(this.myTrainingReportForm.value.toDate);
      this.BookmarkRequest.Client = this.myTrainingReportForm.value.clientMultiSelect
      //this.BookmarkRequest.FromDate = new Date(this.myTrainingReportForm.value.fromDate.setDate(this.myTrainingReportForm.value.fromDate.getDate() + 1)).toISOString().split('T')[0];
      //this.BookmarkRequest.ToDate = new Date(this.myTrainingReportForm.value.toDate.setDate(this.myTrainingReportForm.value.toDate.getDate() + 1)).toISOString().split('T')[0];          

      //fetch values of from date, to date and date type and attach it to object of DateTypeFilter
      //then assign it to this.BookmarkRequest.DateTypeFilter

      if (this.BookmarkRequest.ReportType != 1) {
        this.BookmarkRequest.GroupByColumns = this.myTrainingReportForm.value.selectColumnsMultiSelect;
      }
      else {
        this.BookmarkRequest.GroupByColumns = [];
      }

      this.BookmarkRequest.AreaPaths = this.myTrainingReportForm.value.areaPathMultiSelect;
      this.BookmarkRequest.PrimaryActivities = this.myTrainingReportForm.value.primaryactivityMultiSelect;
      this.BookmarkRequest.activityOptions = Array.isArray(this.myTrainingReportForm.value.activityTypeSelect) ? this.myTrainingReportForm.value.activityTypeSelect : (this.myTrainingReportForm.value.activityTypeSelect) ? [this.myTrainingReportForm.value.activityTypeSelect] : [];
      this.BookmarkRequest.EmployeeId = this.selectedUser?.employeeId ? [this.selectedUser.employeeId] : [];

      if (this.sendEmailFields) {
        this.BookmarkRequest.emailTo = this.sendEmailFields.emailTo;
        this.BookmarkRequest.emailCC = this.sendEmailFields.emailCC;
        this.BookmarkRequest.emailSubject = this.sendEmailFields.emailSubject;
      }

      // Check if bookmarkControl is valid
      if (this.BookmarkRequest.BookMarkName.trim() === '') {
        // If bookmarkControl is empty, show an error message or return
        console.error('Bookmark is required'); // Log error if bookmark is empty
        this.toastr.error('Bookmark name cannot be empty.');
        return;// Prevent adding the bookmark
      }

      if (this.BookmarkRequest.ReportType != 1 && this.BookmarkRequest.SelectColumns != null && !this.arraysEqual(this.BookmarkRequest.SelectColumns, this.BookmarkRequest.GroupByColumns)) {
        this.toastr.error("Select values in the Group By Columns field in the same way as you do for the Select Columns field.");
        return;
      }

      // if (this.BookmarkRequest.ReportType == 3 && (this.BookmarkRequest.Statuses == null || this.BookmarkRequest.Statuses.length == 0 || this.BookmarkRequest.Statuses.length > 1)) {
      //   this.toastr.error("For Compliance Report, Please Select one status from status field.");
      //   return;
      // }

      // If DateTypeFilter selected then formDate & toDate are Mandatory
      if (this.showDatePickers) {
        if (!this.BookmarkRequest.FromDate) {
          this.toastr.error("Please select FromDate");
          return;
        }
        else if (!this.BookmarkRequest.ToDate) {
          this.toastr.error("Please select ToDate");
          return;
        }
      }

      this.route.params.subscribe(params => {
        const bookmarkId = params['id']; // Replace 'id' with the actual query parameter name

        if (bookmarkId) {
          this.BookmarkRequest.BookMarkId = bookmarkId;
        }
        else {
          this.BookmarkRequest.BookMarkId = this.myTrainingReportForm.value.bookmarkIdControl;
          //this.BookmarkRequest.BookMarkId =0;
        }
      });

      //Save data from here
      this.loaderService.start();
      this.academyHttpService
        .addBookmark(this.BookmarkRequest)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (response: any) => {
            if (response.status === 200) {
              this.toastr.success(response.message, "Success");
              this.bookMarkId = response.data.bookMarkId;
              this.updateBookmarkId.emit(this.bookMarkId);
              this.myTrainingReportForm.get('bookmarkIdControl')?.setValue(response.data.bookMarkId);
              this.isSavedBookMark = true;
              //console.log(response.data.bookMarkId);
            } else {
              this.toastr.error(response.errorMessage, "Training Save Error");
            }
          },
        });

      //this.resetForm(); // Reset the form after saving
      this.submitted = false; // Reset submitted flag

    } else {
      console.error('Form is invalid');
    }
  }

  // Method to reset the form
  resetForm(): void {
    this.submitted = false;
    this.myTrainingReportForm.reset({
      reportTypeSelect: '',
      tdcMultiSelect: [],
      communityMultiSelect: [],
      clientMultiSelect: [],
      trainingsMultiSelect: [],
      seniorityMultiSelect: [],
      projectMultiSelect: [],
      statusMultiSelect: [],
      selectColumnsMultiSelect: [],
      // groupByColumnsMultiSelect: [],
      employeeControl: '',
      bookmarkControl: '',
      areaPathMultiSelect: []
    });
    this.selectedUser = null;

    // Reset the "Select All" checkbox state
    this.selectAllStates['tdcMultiSelect'] = false;
    this.selectAllStates['communityMultiSelect'] = false;
    this.selectAllStates['seniorityMultiSelect'] = false;
    this.selectAllStates['statusMultiSelect'] = false;
    this.selectAllStates['projectMultiSelect'] = false;
    this.selectAllStates['trainingsMultiSelect'] = false;
    this.selectAllStates['selectColumnsMultiSelect'] = false;
    // this.selectAllStates['groupByColumnsMultiSelect'] = false;
    this.selectAllStates['AreaPathMultiSelect'] = false;
    // Add other controls as needed
  }

  // Method to filter users by employeeName
  private _filterUsers(employeeName: string): User[] {
    const filterValue = employeeName.toLowerCase();
    return this.users.filter((user) =>
      user.employeeName.toLowerCase().includes(filterValue)
    );
  }

  //Search user
  getUserName() {
    const searchTerm = this.employeeControl.value;
    if (typeof searchTerm === "string" && searchTerm.trim()) {
      this.academyHttpService
        .searchEmployee(searchTerm)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (response: any) => {
            if (response.status === 200) {
              this.users = response.data;
            } else {
              this.toastr.error(
                response.errorMessage,
                "Search Employees Error"
              );
            }
          },
        });
    }
    if (!searchTerm || searchTerm.toString().trim().length === 0) {
      this.selectedUser = null;
    }
  }

  // Function to display the employeeName in the input field after selection
  displayFn(user: User): string {
    return user && user.employeeName ? user.employeeName : "";
  }


  // Method to handle user selection from autocomplete
  onSelectUser(user: User): void {
    this.selectedUser = user;
  }

  // Method to toggle select all
  toggleSelectAll(event: MatCheckboxChange, controlName: string, options: string[]): void {
    this.selectAllStates[controlName] = event.checked; // Update the state based on the checkbox
    if (event.checked) {
      this.myTrainingReportForm.get(controlName)?.setValue(options);
    } else {
      this.myTrainingReportForm.get(controlName)?.setValue([]);
    }
  }
  // Method to toggle select all any data type
  toggleSelectAllWithAny(event: any, controlName: string, options: any[], valueKey: string = 'id') {
    const isChecked = event.checked;
    const formControl = this.myTrainingReportForm.get(controlName);

    if (!formControl) return;

    if (isChecked) {
      // Select all option values using the provided key
      const allValues = options.map(option => option[valueKey]);
      formControl.setValue(allValues);
    } else {
      // Clear selection
      formControl.setValue([]);
    }

    // Update Select All state
    this.selectAllStates[controlName] = isChecked;
  }

  // Check array are equal or not
  arraysEqual(arr1: number[], arr2: number[]): boolean {
    // Check if lengths are the same
    if (arr1.length !== arr2.length) {
      return false;
    }

    // Sort both arrays
    const sortedArr1 = arr1.slice().sort((a, b) => a - b);
    const sortedArr2 = arr2.slice().sort((a, b) => a - b);

    // Compare elements
    for (let i = 0; i < sortedArr1.length; i++) {
      if (sortedArr1[i] !== sortedArr2[i]) {
        return false;
      }
    }
    return true; // Arrays are equal
  }

  // Edit bookmark data and bind to data to all fields
  editBookmark(bookmarkId: number): void {
    this.loaderService.start();
    this.academyHttpService.fetchBookmarkById(bookmarkId).pipe(
      finalize(() => this.loaderService.stop())
    ).subscribe({
      next: (response: any) => {
        if (response.status === 200) {
          if (response.data.activityOptions.length > 0) {
            const activityType = this.priActivityOptions.find(i => i.primaryActivityId == response.data.activityOptions[0]);
            this.onActivityTypeChange(activityType.primaryActivityName, response);
            if (activityType.primaryActivityName.toLowerCase() == "activity") {
              this.showTrainingdropdown = false;
            }
          }
          // const reportTypeName = this.reportTypes.find(r => r.reportId === response.data.reportType)?.reportName;
          // if (reportTypeName.toLowerCase() == "detailed report") {
          //     this.myTrainingReportForm.controls["groupByColumnsMultiSelect"].disable();
          // }
          this.myTrainingReportForm.patchValue({
            reportTypeSelect: response.data.reportType,
            tdcMultiSelect: response.data.tdc,
            projectMultiSelect: response.data.projects,
            communityMultiSelect: response.data.communities,
            seniorityMultiSelect: response.data.seniorities,
            statusMultiSelect: response.data.statuses,
            employeeControl: response.data.employeeControl,
            bookmarkControl: response.data.bookMarkName,
            areaPathMultiSelect: response.data.areaPaths,
            clientMultiSelect: response.data.client,
          });
          this.sendEmailFields = {
            emailCC: response.data.emailCC,
            emailSubject: response.data.emailSubject,
            emailTo: response.data.emailTo,
            emailBody: response.data.emailBody
          } as EmailColumnsModel;

          // Set the datetypefilter related value if  filter selected.          
          if (response.data?.dateTypeFilter?.trim()) {
            this.showDatePickers = true;
            this.myTrainingReportForm.get('dateTypeFilterSelect')?.setValue(response.data.dateTypeFilter, { emitEvent: false });
            this.myTrainingReportForm.get('fromDate')?.setValue(response.data.fromDate, { emitEvent: false });
            this.myTrainingReportForm.get('toDate')?.setValue(response.data.toDate, { emitEvent: false });
          }

          this.updateEmailFields.emit(this.sendEmailFields);
          // Set the selected user if applicable
          if (response.data.employees != null) {
            this.selectedUser = response.data.employees[0];
          } // Adjust based on your data structure
          // this.myTrainingReportForm.get('trainingsMultiSelect')?.setValue(response.data.trainings, { emitEvent: false });
          this.myTrainingReportForm.get('selectColumnsMultiSelect')?.setValue(response.data.configureColumns, { emitEvent: false });
          // this.myTrainingReportForm.get('groupByColumnsMultiSelect')?.setValue(response.data.groupByColumns, { emitEvent: false });
          this.myTrainingReportForm.get('activityTypeSelect')?.setValue(response.data.activityOptions[0], { emitEvent: true });
          // this.myTrainingReportForm.get('primaryactivityMultiSelect')?.setValue(response.data.primaryActivities, { emitEvent: false });

          if (this.myTrainingReportForm.get('activityTypeSelect')?.value !== 2) {
            this.myTrainingReportForm.get('areaPathMultiSelect')
              ?.setValue(response.data.areaPaths, { emitEvent: false });
          } else {
            this.myTrainingReportForm.get('areaPathMultiSelect')
              ?.setValue([], { emitEvent: false });
            this.myTrainingReportForm.get('areaPathMultiSelect')
              ?.disable({ emitEvent: false });
          }

          this.onChangeReportType(response.data.reportType);

        } else {
          this.toastr.error(response.errorMessage, "Fetch Bookmark Error");
        }
      },
      error: (err) => {
        this.toastr.error("An error occurred while fetching the bookmark.", "Error");
      }
    });
  }

  onExportReport() {
    this.request = new ExportDetailReportMetadata();
    var ReportTypes = this.myTrainingReportForm.value.reportTypeSelect;
    if (ReportTypes > 0) {
      this.request.Type = EnumReportType[ReportTypes];
    }

    this.request.BookMarkId = this.bookMarkId;

    this.loaderService.start();
    this.academyHttpService
      .requestDetailedReport(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          console.clear();
          if (response.status === 200) {
            this.toastr.success("A bookmark entry has been added to the background service for sending mail", "Success");
            // this.router.navigate(["/track/worker-request", response.data]);
          } else {
            this.toastr.error(response.errorMessage, "Export Error");
          }
        },
      });
  }

  // Method to handle form submission
  setBookmarkPayload(): void {
    if (this.myTrainingReportForm.valid) {
      this.BookmarkRequest.BookMarkName = this.myTrainingReportForm.value.bookmarkControl;
      this.BookmarkRequest.TDC = this.myTrainingReportForm.value.tdcMultiSelect;
      this.BookmarkRequest.Community = this.myTrainingReportForm.value.communityMultiSelect;
      this.BookmarkRequest.Trainings = this.myTrainingReportForm.value.trainingsMultiSelect;
      this.BookmarkRequest.Seniorities = this.myTrainingReportForm.value.seniorityMultiSelect;
      this.BookmarkRequest.Projects = this.myTrainingReportForm.value.projectMultiSelect;
      this.BookmarkRequest.Statuses = this.myTrainingReportForm.value.statusMultiSelect;
      this.BookmarkRequest.ReportType = this.myTrainingReportForm.value.reportTypeSelect;
      this.BookmarkRequest.SelectColumns = this.myTrainingReportForm.value.selectColumnsMultiSelect;
      this.BookmarkRequest.Client = this.myTrainingReportForm.value.clientMultiSelect;
      if (this.BookmarkRequest.ReportType != 1) {
        this.BookmarkRequest.GroupByColumns = this.myTrainingReportForm.value.selectColumnsMultiSelect;
      }
      else {
        this.BookmarkRequest.GroupByColumns = []
      }
      this.BookmarkRequest.EmployeeId = this.selectedUser?.employeeId ? [this.selectedUser.employeeId] : [];
      this.BookmarkRequest.BookMarkId = this.myTrainingReportForm.value.bookmarkIdControl;
      this.BookmarkRequest.AreaPaths = this.myTrainingReportForm.value.areaPathMultiSelect;
      this.BookmarkRequest.PrimaryActivities = this.myTrainingReportForm.value.primaryactivityMultiSelect;
      this.BookmarkRequest.activityOptions = Array.isArray(this.myTrainingReportForm.value.activityTypeSelect) ? this.myTrainingReportForm.value.activityTypeSelect : (this.myTrainingReportForm.value.activityTypeSelect) ? [this.myTrainingReportForm.value.activityTypeSelect] : [];

      if (this.BookmarkRequest.ReportType != 1 && this.BookmarkRequest.SelectColumns != null && !this.arraysEqual(this.BookmarkRequest.SelectColumns, this.BookmarkRequest.GroupByColumns)) {
        this.toastr.error("Select values in the Group By Columns field in the same way as you do for the Select Columns field.");
        return;
      }
    }
  }

  onGenrateReport() {
    if (this.myTrainingReportForm.get('reportTypeSelect')?.value == 5 &&
      !this.myTrainingReportForm.get('communityMultiSelect')?.value.length &&
      !this.myTrainingReportForm.get('clientMultiSelect')?.value?.length
    ) {
      this.toastr.error("Please select any one of Community or Clients")
      return;
    }
    this.setBookmarkPayload();
    // if (this.BookmarkRequest.ReportType == 3 && (this.BookmarkRequest.Statuses == null || this.BookmarkRequest.Statuses.length == 0 || this.BookmarkRequest.Statuses.length > 1)) {
    //   this.toastr.error("For Compliance Report, Please Select one status from status field.");
    //   return;
    // }

    // this.fetchData(this.BookmarkRequest); // Fetch data from API on initialization
    this.generateDataEvent.emit(this.BookmarkRequest);
  }

  exportReport() {
    if (this.myTrainingReportForm.get('reportTypeSelect')?.value == 5 &&
      !this.myTrainingReportForm.get('communityMultiSelect')?.value.length &&
      !this.myTrainingReportForm.get('clientMultiSelect')?.value?.length
    ) {
      this.toastr.error("Please select any one of Community or Clients")
      return;
    }
    this.setBookmarkPayload();
    this.loaderService.start();
    this.academyHttpService
      .exportReportData(this.BookmarkRequest)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response) => {
          const apiResponse = response as ApiResponse;
          if (apiResponse.data.includes('http')) {
            this.exportUrl = apiResponse.data;
            this.toastr.success("Export Successful, please click on view report to access the report", "Success");
          } else {
            this.toastr.error(
              apiResponse.data || "Unknown error",
              "Error"
            );
          }
        },
      });
  }
  get isValidGroupByColumnsMultiSelectField() {
    return (this.submitted || this.myTrainingReportForm.get('groupByColumnsMultiSelect')?.touched) &&
      this.myTrainingReportForm
        .get('groupByColumnsMultiSelect')
        ?.hasError('required')
  }

  get isValidSelectColumnsMultiSelectField() {
    return (this.submitted || this.myTrainingReportForm.get('selectColumnsMultiSelect')?.touched) &&
      this.myTrainingReportForm
        .get('selectColumnsMultiSelect')
        ?.hasError('required')
  }

  get groupByColumnLabel() {
    return this.myTrainingReportForm.get('groupByColumnsMultiSelect')?.hasValidator(Validators.required) ? "Group By Columns*" : "Group By Columns";
  }
  // onCommunitySelection(selectedCommunity: any) {
  //     if (selectedCommunity.length > 0) {
  //         this.academyHttpService.fetchTrainingsByCommunity(selectedCommunity as string[]).subscribe({
  //             next: (response: any) => {
  //                 if (response.status === 200) {
  //                     this.trainingOptions = response.data;
  //                 } else {
  //                     this.toastr.error(response.errorMessage, "Training data get Error");
  //                 }
  //             },
  //         });
  //     }
  // }

  onChangeReportType(event: MatSelectChange) {
    const selectedReport = event?.value || event;
    console.log('Selected values:', selectedReport);
    if ([3, 4].includes(selectedReport)) {
      this.myTrainingReportForm.get('seniorityMultiSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('trainingsMultiSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('statusMultiSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('selectColumnsMultiSelect')?.disable({ emitEvent: false });
      this.disbaleAcitivtyTypedropdown = true;
      this.myTrainingReportForm.get('dateTypeFilterSelect')?.disable({ emitEvent: false });
    }
    else if ([5].includes(selectedReport)) {
      this.myTrainingReportForm.get('seniorityMultiSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('trainingsMultiSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('statusMultiSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('selectColumnsMultiSelect')?.disable({ emitEvent: false });
      this.disbaleAcitivtyTypedropdown = true;
      this.myTrainingReportForm.get('dateTypeFilterSelect')?.disable({ emitEvent: false });
      this.myTrainingReportForm.get('areaPathMultiSelect')?.disable({ emitEvent: false });
    }
    else {
      this.myTrainingReportForm.get('seniorityMultiSelect')?.enable();
      this.myTrainingReportForm.get('trainingsMultiSelect')?.enable();
      this.myTrainingReportForm.get('statusMultiSelect')?.enable();
      this.myTrainingReportForm.get('selectColumnsMultiSelect')?.enable();
      this.disbaleAcitivtyTypedropdown = false;
      this.myTrainingReportForm.get('dateTypeFilterSelect')?.enable();
      this.myTrainingReportForm.get('areaPathMultiSelect')?.enable({ emitEvent: false });
    }
  }

  evaluateSelection(): void {
    // No API calls until both are valid
    if (!this.selectedPActivity) return;

    if (this.selectedPActivity == 1) {
      let communities = [];
      if (this.selectedCommunities.length) {
        communities = this.selectedCommunities;
      }
      let areaPath: any[] = [];
      if (this.areaPathOptions.length) {
        areaPath = this.selectedAreaPath;
      }
      this.academyHttpService.fetchAllTrainings(communities, areaPath).subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.trainingOptions = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Training data get Error");
          }
        },
      });

    } else if (this.selectedPActivity == 2) {
      let communities = [];
      if (this.selectedCommunities.length) {
        communities = this.selectedCommunities;
      }
      this.academyHttpService.fetchPrimaryActivityByCommunity(communities).subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.activityOptions = response.data;
          } else {

            this.toastr.error(response.errorMessage, "Training data get Error");
          }
        },
      });
    }

  }
  fetchClientProjects(clients: any) {
    this.academyHttpService.fetchAllProjects(clients).subscribe({
      next: (response: any) => {
        if (response.status === 200) {
          this.projectOptions = response.data;
        } else {
          this.toastr.error(response.errorMessage, "Clients data get Error");
        }
      },
    });
  }
  onActivityTypeChange(selectedValue: string, response: any = null) {
    //change the options in the dropdown of date type filter
    if (selectedValue.toLowerCase() == "activity") {
      this.dateTypeFilters = this.dateTypeFiltersForActivityTypePrimary;
    }
    else if (selectedValue.toLowerCase() == "training") {
      this.dateTypeFilters = this.dateTypeFiltersForActivityTypeTraining;
    }

    this.activitytype = selectedValue;
    forkJoin({
      AllSelectColumns: this.academyHttpService.fetchAllSelectColumns(this.activitytype).pipe(map((res: any) => res.data)),
      AllGroupByColumns: this.academyHttpService.fetchAllGroupByColumns(this.activitytype).pipe(map((res: any) => res.data)),
    }).subscribe(result => {
      this.selectColumnsOptions = result.AllSelectColumns;
      this.groupByColumnsOptions = result.AllGroupByColumns;
      if (response) {
        this.myTrainingReportForm.patchValue({
          selectColumnsMultiSelect: response.data.configureColumns,
          // groupByColumnsMultiSelect: response.data.groupByColumns,
          primaryactivityMultiSelect: response.data.primaryActivities,
          trainingsMultiSelect: response.data.trainings
        });
      }
    });
  }

  onDateTypeFilterChange(selectedValue: any) {
    console.log("selected value of date type ", selectedValue)

    if (selectedValue) {
      this.showDatePickers = true;
    }
    else {
      this.showDatePickers = false;
    }
  }

  private formatDate(date: Moment | null): string | null {
    if (!date) return null;
    //return date.format('YYYY-MM-DD'); // Returns "2025-12-22"
    return moment(date).format('YYYY-MM-DD');
  }
}



