import { animate, state, style, transition, trigger } from "@angular/animations";
import { SelectionModel } from "@angular/cdk/collections";
import { Overlay } from "@angular/cdk/overlay";
import { CommonModule } from "@angular/common";
import { Component, inject, OnInit, ViewChild } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatChipsModule } from "@angular/material/chips";
import { MatDialog } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatMenuModule } from "@angular/material/menu";
import { MatPaginator, MatPaginatorModule, PageEvent } from "@angular/material/paginator";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSelectModule, MatSelect } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { ActivatedRoute, Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { finalize, forkJoin } from "rxjs";
import { AcademyHttpService } from "@services/academy-http.service";
import { LoaderService } from "@services/loader.service";
import { UpdateEndDateDialogComponent } from "../update-end-date-dialog/update-end-date-dialog.component";
import { UpdateTrainingImpactDialogComponent } from "@components/training-impact/update-training-impact-dialog/update-training-impact-dialog.component";
import { ScheduleMockInterviewDialogComponent } from "@components/interview/schedule-mock-interview-dialog/schedule-mock-interview-dialog.component";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { PrivilegedUserDirective } from "@shared/directives/privileged-user.directive";
import { FilterDataDto } from "@shared/dto/filter-data-dto";
import { MatTooltip } from "@angular/material/tooltip";
import { FormsModule } from "@angular/forms";
import { MatOptionSelectionChange } from "@angular/material/core";

export interface PeriodicElement {
  arrow: string;
  employeeName: string;
  globantEmailAddress: string;
  employeeEmail: string;
  dojoDetailId: number;
  workingEcosystem: string;
  seniority: string;
  community: string;
  tdc: string;
  account: string;
  trainingScore: number;
  joiningDate: string;
  nestedData?: NestedElement[];
}

export interface SelectedUser {
  id: number;
  email: string;
}
export interface NestedElement {
  detail: string;
  value: string;
}

@Component({
  selector: "app-training-impact",
  templateUrl: "./training-impact.component.html",
  styleUrls: ["./training-impact.component.css"],
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatMenuModule,
    MatSlideToggleModule,
    PrivilegedUserDirective,
    MatCheckboxModule,
    MatChipsModule,
    MatTooltip,
    FormsModule, // <-- Add FormsModule to imports array
  ],
  animations: [
    trigger("detailExpand", [
      state("collapsed, void", style({ height: "0px", minHeight: "0" })),
      state("expanded", style({ height: "*" })),
      transition(
        "expanded <=> collapsed",
        animate("225ms cubic-bezier(0.4, 0.0, 0.2, 1)")
      ),
    ]),
  ],
})
export class TrainingImpactComponent implements OnInit {
  public totalPages = 0;
  public totalItems = 0;
  public pageSize = 20;
  public pageIndex = 0;
  
  dataSource = new MatTableDataSource<PeriodicElement>();
  selectedUsers: Set<any> = new Set();
  selection = new SelectionModel<any>(true, []);
  data: any[] = [];
  displayedColumns: string[] = [
    "select",
    "employeeName",
    "globantEmailAddress",
    "aiStudio",
    "account",
    "community",
    "dojoStartDate",
    "dojoEndDate",
    "assignedThroughTraining",
    "comments",
    "ticketNumber",
  ];

  color = '#bfd732';
  request: any = {
    pageIndex: 1,
    pageSize: 20,
    searchText: "",
    community: [],
    aiStudio:[],
    account:[],
    tdc:[],
    SortBy: "",
    SortByDescending: "",
  };
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('communitySelect') communitySelect!: MatSelect;
  @ViewChild('countrySelect') countrySelect!: MatSelect;
  @ViewChild('aiStudioSelect') aiStudioSelect!: MatSelect;
  @ViewChild('accountSelect') accountSelect!: MatSelect;

  filterValues: FilterDataDto[] = [];
  public columns = [
    {
      colName: "select",
      sortAllowed: false,
      filterAllowed: false,
      defaultSort: false,
      defaultFilter: false,
    },
    {
      colName: "Glober",
      sortAllowed: false,
      filterAllowed: false,
      defaultSort: false,
      defaultFilter: false,
    },
    {
      colName: "Email",
      sortAllowed: true,
      filterAllowed: false,
      defaultSort: true,
      defaultFilter: false,
      value: "globantEmailAddress",
    },
    {
      colName: "StartDate",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "dojoStartDate",
    },
    {
      colName: "EndDate",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "dojoEndDate",
    },
    {
      colName: "Community",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "community",
    },
    {
      colName: "AssignedThroughTraining",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "assignedThroughTraining",
    },
    {
      colName: "Comments",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: true,
      defaultFilter: true,
      value: "comments",
    },
    {
      colName: "Ticket",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: true,
      defaultFilter: true,
      value: "ticketNumber",
    },
  ];
  selectedSort: string | undefined = "";
  selectedRowIndex: number | null = null;
  selectedFilterColumn: any = "";
  activityList: any;
  communities: string[] = [];
  selectedCommunity: string[] = [];
  tdc: any[] = [];
  accounts: string[] = [];
  allAccounts: { aiStudio: string; account: string }[] = [];
  aiStudios: any[] = [];
  selectedAccount: string[] = [];
  selectedAiStudio: string[] = [];
  selectedTdc: string[] = [];
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];
  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly router: Router,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService,
    private dialog: MatDialog,
    private overlay: Overlay
  ) {
    this.selectedSort = this.columns.filter((el) => el.defaultSort)[0].value;
    this.selectedFilterColumn = this.columns.find(
      (column) => column.defaultFilter === true
    )?.value;
  }
  columnTitles: { [key: string]: string } = {
    employeeName: "Glober",
    globantEmailAddress: "Email",
    aiStudio:"AI Studio",
    account:"Account",
    dojoStartDate: "Start Date",
    dojoEndDate: "End Date",
    community: "Community",
    assignedThroughTraining: "Assigned Through Training",
    comments: "Comments",
    ticketNumber: "Ticket",
    select: "",
  };

  ngOnInit(): void {
    this.initialSetup();
  }

  ngOnDestroy(): void {
    // Clean up subscriptions
  }

  reset(): void {
    this.request = {
      pageIndex: 1,
      pageSize: 20,
      searchText: "",
      community: [],
      aiStudio:[],
      account:[],
      tdc:[],
      SortBy: "",
      SortByDescending: "",
    };
    this.selectedCommunity = [];
    this.selectedTdc = [];
    this.selectedAiStudio = [];
    this.selectedAccount = [];
    this.loadTrackerList();
  }

  initialSetup() {
    this.request.pageIndex = 1;
    this.request.pageSize = 20;
    this.initializeMain();
  }

  loadTrackerList() {
    this.loaderService.start();
    this.academyHttpService
      .fetchDojoMemberList(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response) => {
          interface ApiResponse {
            success: boolean;
            data: {
              items: any[];
              totalPages: number;
              pageSize: number;
              pageIndex: number;
              totalCount: number;
            };
            errorMessage?: string;
          }

          const apiResponse = response as ApiResponse;
          if (apiResponse.success) {
            this.configurePaginator(apiResponse.data);
            this.dataBind(apiResponse.data.items);
          } else {
            this.toastr.error(
              apiResponse.errorMessage || "Unknown error",
              "Error"
            );
          }
        },
      });
  }

  private configurePaginator(data: any) {
    this.totalPages = data.totalPages;
    this.pageSize = data.pageSize;
    this.pageIndex = data.pageIndex;
    this.totalItems = data.totalCount;
  }

  private dataBind(items: any[]): void {
    this.data = items;
    this.setDefaultIcon(-1);
  }

  private setDefaultIcon(index: number) {
    for (let i = 0; i < this.dataSource.data.length; i++) { }
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.communitySelect.optionSelectionChanges.subscribe(
      (event: MatOptionSelectionChange) => this.onCommunityChanged(event)
    );

    this.countrySelect.optionSelectionChanges.subscribe(
      (event: MatOptionSelectionChange) => this.onCountryChanged(event)
    );
    this.aiStudioSelect.optionSelectionChanges.subscribe(
      (event: MatOptionSelectionChange) => this.onAiStudioChanged(event)
    );

    this.accountSelect.optionSelectionChanges.subscribe(
      (event: MatOptionSelectionChange) => this.onAccountChanged(event)
    );
    this.dataSource.filterPredicate = (
      data: PeriodicElement,
      filter: string
    ) => {
      const dataStr = Object.values(data)
        .reduce((acc, value) => {
          return (
            acc +
            " " +
            (typeof value === "string" ? value : JSON.stringify(value))
          );
        }, "")
        .toLowerCase();
      return dataStr.includes(filter.trim().toLowerCase());
    };
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;

    this.request.searchText = filterValue;
    this.request.pageIndex = 1;
    this.loadTrackerList();
  }

  filterTracker() {
    this.request.PageIndex = 1;
    this.loadTrackerList();
  }

  onSortByOptionChanged(ev: any) {
    this.request.SortBy = ev.value;
  }

  onSortDirOptionChanged(ev: any) {
    this.request.SortByDescending = ev.value == 1;
  }

  onPageChanged(e: PageEvent) {
    this.setDefaultIcon(-1);
    this.selectedRowIndex = null;
    this.request.PageIndex = e.pageIndex + 1;
    this.request.PageSize = e.pageSize;
    this.loadTrackerList();
  }

  initializeMain() {
    const trackerList$ = this.academyHttpService.fetchDojoMemberList(
      this.request
    );
    const community$ = this.academyHttpService.fetchAllCommunity();
    const country$ = this.academyHttpService.fetchAllTdc();
    const aiStudio$ = this.academyHttpService.fetchAllAiStudio();
    const account$ = this.academyHttpService.fetchAllAiStudioAndAccount();
    this.loaderService.start();
    forkJoin([trackerList$, community$, country$, aiStudio$, account$])
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe((responses: any) => {
        if (responses[0].status === 200) {
          this.configurePaginator(responses[0].data);
          this.dataBind(responses[0].data.items);
        }
        if (responses[1].status === 200) {
          this.communities = responses[1].data;
          this.tdc = responses[2].data;
          this.aiStudios = responses[3].data;
          this.allAccounts = responses[4].data;
          if (!this.request.aiStudio || this.request.aiStudio.length === 0) {
            this.accounts = [
              ...new Set(this.allAccounts.map((a: any) => a.account))
            ];
          }
        }
      });
  }

  clickItem(event: MouseEvent) {
    event.preventDefault();
  }

  openMockInterviewDialog(employeeId: number): void {
    const dialogRef = this.dialog.open(ScheduleMockInterviewDialogComponent, {
      data: { employeeId: employeeId },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
      }
    });
  }

  view(employeeId: number) {
    this.router.navigate(["/mock-interview-details", employeeId]);
    // this.router.navigate(["/dashboard", employeeId]);
  }
  isSelected(row: PeriodicElement): boolean {
    const existing = Array.from(this.selectedUsers).find(
      (x) => x.dojoDetailId === row.dojoDetailId
    );
    var ispresent = existing !== undefined;
    return ispresent;
  }

  // Called when individual checkbox is toggled
  onCheckboxChange(user: PeriodicElement): void {
    if (this.selectedUsers.has(user)) {
      this.selectedUsers.delete(user);
    } else {
      this.selectedUsers.add(user);
    }
  }
  // Returns whether all rows on the current page are selected
  isAllSelected(): boolean {
    if (!this.dataSource || !this.dataSource.data || !this.paginator) {
      return false;
    }

    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    const pageData = this.dataSource.data.slice(startIndex, endIndex);

    return pageData.every((row) => this.selection.isSelected(row));
  }

  isIndeterminate(): boolean {
    if (!this.dataSource || !this.dataSource.data || !this.paginator) {
      return false;
    }

    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    const pageData = this.dataSource.data.slice(startIndex, endIndex);

    const selectedCount = pageData.filter((row) =>
      this.selection.isSelected(row)
    ).length;
    return selectedCount > 0 && selectedCount < pageData.length;
  }

  // Selects all rows if not all selected; otherwise clear selection
  masterToggle(): void {
    if (!this.dataSource || !this.paginator) return;

    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    const pageData = this.dataSource.data.slice(startIndex, endIndex);

    this.isAllSelected()
      ? pageData.forEach((row) => this.selection.deselect(row))
      : pageData.forEach((row) => this.selection.select(row));
  }
  openDojoEndDateDialog(): void {
    const dialogRef = this.dialog.open(UpdateEndDateDialogComponent, {
      //width: '100%',
      width: "400px",
      maxWidth: "60vw",
      height: "auto",
      data: {},
      panelClass: "",
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });

    dialogRef.afterClosed().subscribe((activity) => {
      console.log("dialog was closed", activity);
      if (activity === "") return;

      const activityEndDate = new Date(activity.endDate);
      activityEndDate.setHours(12, 0, 0, 0);
      const utcEndDateString = activityEndDate.toISOString();

      var activities = Array.from(this.selectedUsers).map((i) => {
        return {
          dojoDetailId: i.dojoDetailId,
          dojoEndDate: utcEndDateString,
        };
      });
      console.log("activities to save", activities);
      this.loaderService.start();
      this.academyHttpService
        .updateDojoEndDate(activities)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (res: any) => {
            if (res.status === 200) {
              this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
              this.selectedUsers = new Set();
              this.initializeMain();
            } else {
              this.toastr.error(res?.message || "Error", "Error");
            }
          },
          error: (err) => {
            const validationErrors = err?.error?.errors;
            if (validationErrors) {
              console.error("Validation errors:", validationErrors);
              this.toastr.error(
                "Validation error: " + JSON.stringify(validationErrors),
                "Error"
              );
            } else {
              const errMsg =
                err?.error?.message || "Unexpected error during save.";
              this.toastr.error(errMsg, "Error");
            }
          },
        });
    });
  }

checkTicketNumbers() {
    const userArray = Array.from(this.selectedUsers);
    const hasInvalidTicketNumber = userArray.some(user => 
        !user.ticketNumber || user.ticketNumber === ''
    );

    if (hasInvalidTicketNumber) {
        return false
    }   

    return true;
}

  openDojoTrainingInfoDialog(): void {
     if(!this.checkTicketNumbers()){
     this.toastr.error("One or more users have an invalid ticket number.", "Error");
     return;
     }

              
    const empEmails = Array.from(this.selectedUsers).map((user) => user.globantEmailAddress);
    const dialogRef = this.dialog.open(UpdateTrainingImpactDialogComponent, {
      //width: '100%',
      width: "80vw",
      maxWidth: "80vw",
      height: "auto",
      data: empEmails,
      panelClass: "",
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });

    dialogRef.afterClosed().subscribe((activitiesToUpdate) => {
      console.log("dialog was closed", activitiesToUpdate);
      if (!activitiesToUpdate) return;
  
      this.loaderService.start();
      this.academyHttpService
        .updateDojoTrainingIfno(activitiesToUpdate)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (res: any) => {
            if (res.status === 200) {
              this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
              this.selectedUsers = new Set();
              this.initializeMain();
            } else {
              this.toastr.error(res?.message || "Error", "Error");
            }
          },
          error: (err) => {
            const validationErrors = err?.error?.errors;
            if (validationErrors) {
              console.error("Validation errors:", validationErrors);
              this.toastr.error(
                "Validation error: " + JSON.stringify(validationErrors),
                "Error"
              );
            } else {
              const errMsg =
                err?.error?.message || "Unexpected error during save.";
              this.toastr.error(errMsg, "Error");
            }
          },
        });
    });
  }

  onRemoveUser(data: any, user: any) {
    for (const item of this.selectedUsers) {
      if (item.dojoDetailId === user.dojoDetailId) {
        this.selectedUsers.delete(item);
      }
    }
  }

  trimText(text: any) {
    const maxLength = 30;
    if (text.length > maxLength) {
      return text.substring(0, maxLength) + "...";
    }
    return text;
  }

  onCommunityChanged(option: MatOptionSelectionChange) {
    if (!option.isUserInput) return;

    const value = option.source.value;
    const isChecked = option.source.selected;
    const allValues = this.communities;

    if (value === 'ALL') {
      this.selectedCommunity = isChecked ? ['ALL', ...allValues] : [];
    } else {
      if (isChecked) {
        this.selectedCommunity = [...new Set([...this.selectedCommunity, value])];
      } else {
        this.selectedCommunity =
          this.selectedCommunity.filter(v => v !== value);
      }

      const allSelected = allValues.every(v =>
        this.selectedCommunity.includes(v)
      );

      this.selectedCommunity = allSelected
        ? ['ALL', ...allValues]
        : this.selectedCommunity.filter(v => v !== 'ALL');
    }    
    this.request.community =
      this.selectedCommunity.includes('ALL')
        ? []
        : this.selectedCommunity;
  }

  onCountryChanged(option: MatOptionSelectionChange) {
    if (!option.isUserInput) {
      return;
    }
    const value = option.source.value;
    const isChecked = option.source.selected;
    const allValues = this.tdc;
    if (value === 'ALL') {
      if (isChecked) {
        this.selectedTdc = ['ALL', ...allValues];
      } else {
        this.selectedTdc = [];
      }
    } else {
      if (isChecked) {
        this.selectedTdc = [...new Set([...this.selectedTdc, value])];
      } else {
        this.selectedTdc = this.selectedTdc.filter(v => v !== value);
      }
      const allSelected = allValues.every(v =>
        this.selectedTdc.includes(v)
      );
      this.selectedTdc = allSelected
        ? ['ALL', ...allValues]
        : this.selectedTdc.filter(v => v !== 'ALL');
    }
    this.request.country = this.selectedTdc.includes('ALL') ? [] : this.selectedTdc;    
  }

  onAiStudioChanged(option: MatOptionSelectionChange) {
    if (!option.isUserInput) return;

    const value = option.source.value;
    const isChecked = option.source.selected;
    const allValues = this.aiStudios;

    if (value === 'ALL') {
      this.selectedAiStudio = isChecked ? ['ALL', ...allValues] : [];
    } else {
      if (isChecked) {
        this.selectedAiStudio = [...new Set([...this.selectedAiStudio, value])];
      } else {
        this.selectedAiStudio = this.selectedAiStudio.filter(v => v !== value);
      }

      const allSelected = allValues.every(v =>
        this.selectedAiStudio.includes(v)
      );

      this.selectedAiStudio = allSelected
        ? ['ALL', ...allValues]
        : this.selectedAiStudio.filter(v => v !== 'ALL');
    }

    this.request.aiStudio = this.selectedAiStudio.includes('ALL')
      ? []
      : this.selectedAiStudio;
    if (this.request.aiStudio.length > 0) {
      const filtered = this.allAccounts.filter(acc =>
        this.request.aiStudio.includes(acc.aiStudio)
      );
      this.accounts = [
        ...new Set(filtered.map(a => a.account))
      ];
    } else {
      this.accounts = [
        ...new Set(this.allAccounts.map(a => a.account))
      ];
    }
    this.selectedAccount = [];
    this.request.account = [];
  }

  onAccountChanged(option: MatOptionSelectionChange) {
    if (!option.isUserInput) return;

    const value = option.source.value;
    const isChecked = option.source.selected;
    const allValues = this.accounts;

    if (value === 'ALL') {
      this.selectedAccount = isChecked ? ['ALL', ...allValues] : [];
    } else {
      if (isChecked) {
        this.selectedAccount = [...new Set([...this.selectedAccount, value])];
      } else {
        this.selectedAccount = this.selectedAccount.filter(v => v !== value);
      }

      const allSelected = allValues.every(v =>
        this.selectedAccount.includes(v)
      );

      this.selectedAccount = allSelected
        ? ['ALL', ...allValues]
        : this.selectedAccount.filter(v => v !== 'ALL');
    }

    this.request.account = this.selectedAccount.includes('ALL')
      ? []
      : this.selectedAccount;
  }

  search() {
    this.request.pageIndex = 1;
    this.loadTrackerList();
  }
}
