import { animate, state, style, transition, trigger } from "@angular/animations";
import { SelectionModel } from "@angular/cdk/collections";
import { Overlay } from "@angular/cdk/overlay";
import { CommonModule } from "@angular/common";
import { AfterViewInit, Component, OnInit, ViewChild } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatDialog } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatMenuModule } from "@angular/material/menu";
import { MatPaginator, MatPaginatorModule, PageEvent } from "@angular/material/paginator";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { finalize, forkJoin } from "rxjs";
import { AcademyHttpService } from "@services/academy-http.service";
import { LoaderService } from "@services/loader.service";
import { BulkActivityDialogComponent } from "../bulk-activity-dialog/bulk-activity-dialog.component";
import { PrivilegedUserDirective } from "@shared/directives/privileged-user.directive";
import { DataRequestOptions, FilterOption } from "@shared/dto/data-request-options.dto";
import { FilterDataDto } from "@shared/dto/filter-data-dto";
import { MatChipsModule } from "@angular/material/chips";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { ScheduleMockInterviewDialogComponent } from "@components/interview/schedule-mock-interview-dialog/schedule-mock-interview-dialog.component";

export interface PeriodicElement {
  arrow: string;
  employeeName: string;
  employeeEmail: string;
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
  selector: "app-add-bulk-activity",
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
  ],
  templateUrl: "./add-bulk-activity.component.html",
  styleUrls: ["./add-bulk-activity.component.css"],
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
export class AddBulkActivityComponent implements OnInit, AfterViewInit {
  public totalPages = 0;
  public totalItems = 0;
  public pageSize = 20;
  public pageIndex = 0;
  public rows = [
    {
      icon: "chevron_right",
      arrow: " ",
      employeeId: 0,
      employeeName: "John Doe",
      employeeEmail: "john.doe@example.com",
      workingEcosystem: ".Net FullStack",
      seniority: "SSr",
      community: "DOTNET",
      tdc: "IN/Pune",
      client: "Client",
      joiningDate: "08/09/2024",
      timeOnDojo: "N/A",
      trainingScore: 0,
      designation: "",
    },
  ];
  dataSource = new MatTableDataSource<PeriodicElement>();
  selectedUsers: Set<any> = new Set();
  selection = new SelectionModel<any>(true, []);
  data: any[] = [];
  displayedColumns: string[] = [
    "select",
    "employeeName",
    "employeeEmail",
    "workingEcosystem",
    "seniority",
    "community",
    "tdc",
    "client",
    "trainingScore",
    "joiningDate",
  ];
  expandedElement: PeriodicElement | null | undefined;
  color = '#bfd732';
  public request = new DataRequestOptions();
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  seniorities: any[] = [];
  communities: any[] = [];
  tdcs: any[] = [];
  accounts: any[] = [];
  ecosystems: any[] = [];
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
      value: "EmployeeEmail",
    },
    {
      colName: "WorkingEcosystem",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "WorkingEcosystem",
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
      colName: "Community",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "Community",
    },
    {
      colName: "TDC",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "Tdc",
    },
    {
      colName: "Account",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: true,
      defaultFilter: true,
      value: "Client",
    },
    {
      colName: "Training Completion Score",
      sortAllowed: false,
      filterAllowed: false,
      defaultSort: false,
      defaultFilter: false,
    },
    {
      colName: "Joining Date",
      sortAllowed: false,
      filterAllowed: false,
      defaultSort: false,
      defaultFilter: false,
    },
    {
      colName: "",
      sortAllowed: false,
      filterAllowed: false,
      defaultSort: false,
      defaultFilter: false,
    },
  ];
  selectedSort: string | undefined = "";
  selectedRowIndex: number | null = null;
  selectedFilterColumn: any = "";
  activityList: any;

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
    employeeEmail: "Email",
    workingEcosystem: "Working Ecosystem",
    seniority: "Seniority",
    community: "Community",
    tdc: "TDC",
    client: "Account",
    trainingScore: "Training Completion Score",
    joiningDate: "Joining Date",
    select: "",
  };

  toggleEmployee(element: any) {
    this.expandedElement = this.expandedElement === element ? null : element;
  }

  ngOnInit() {
    this.initialize();
  }

  reset() {
    window.location.reload();
  }

  initialize() {
    this.request.PagingOptions.PageSize = 20;
    this.request.SortOptions.SortBy = "EmployeeEmail";
    this.request.SortOptions.SortByDescending = false;
    this.initalize();
  }

  loadTrackerList() {
    this.loaderService.start();
    this.academyHttpService
      .fetchTrackerList(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.configurePaginator(response.data);
            this.dataBind(response.data.items);
          } else {
            this.toastr.error(response.errorMessage, "Error");
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

  private dataBind(items: []) {
    this.data = items;
    this.setDefaultIcon(-1);
  }

  private setDefaultIcon(index: number) {
    for (let i = 0; i < this.dataSource.data.length; i++) {
      //if (index !== i)
      //this.dataSource.data[i].icon = 'chevron_right';
    }
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

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
    // this.dataSource.filter = filterValue.trim().toLowerCase();

    this.request.SearchText = filterValue;
    this.request.PagingOptions.PageIndex = 0;
    this.loadTrackerList();
  }

  filterTracker() {
    this.request.PagingOptions.PageIndex = 0;
    this.loadTrackerList();
  }

  onSortByOptionChanged(ev: any) {
    this.request.SortOptions.SortBy = ev.value;
  }

  onSortDirOptionChanged(ev: any) {
    this.request.SortOptions.SortByDescending = ev.value == 1;
  }

  onFilterOptionChanged(ev: any) {
    this.selectedFilterColumn = ev.value;
    this.fetchFilterValues(ev.value);
  }

  onFilterValueChanged(ev: any) {
    let filterOption = new FilterOption();
    filterOption.FilterBy = this.selectedFilterColumn;
    filterOption.FilterValue = ev.value;

    var dojoFilter = this.request.FilterOptions.find(
      (x) => x.FilterValue === "DOJO"
    );

    this.request.FilterOptions = [];

    if (dojoFilter) {
      this.request.FilterOptions.push(dojoFilter);
    }

    this.request.FilterOptions.push(filterOption);
  }

  onToggleChange(ev: any) {
    let filterOption = new FilterOption();

    if (ev.checked) {
      filterOption.FilterBy = "Project";
      filterOption.FilterValue = "DOJO";
      this.request.FilterOptions.push(filterOption);
    } else {
      var dojoFilter = this.request.FilterOptions.find(
        (x) => x.FilterValue === "DOJO"
      );
      if (dojoFilter) {
        this.request.FilterOptions = this.request.FilterOptions.filter(
          (x) => x.FilterValue !== "DOJO"
        );
      }
    }
    this.loadTrackerList();
  }

  onPageChanged(e: PageEvent) {
    this.setDefaultIcon(-1);
    this.selectedRowIndex = null;
    if ((Math.ceil(this.totalItems / this.pageSize) - 1) == e.pageIndex) {
      this.request.PagingOptions.PageIndex = e.pageIndex;
    } else {
      this.request.PagingOptions.PageIndex = e.pageIndex + 1;
    }
    this.request.PagingOptions.PageSize = e.pageSize;
    this.loadTrackerList();
  }

  initalize() {
    const trackerList$ = this.academyHttpService.fetchTrackerList(this.request);
    const accounts$ = this.academyHttpService.fetchAllAccount();
    const seniorities$ = this.academyHttpService.fetchSeniorities();
    const tdcs$ = this.academyHttpService.fetchAllTdc();
    const community$ = this.academyHttpService.fetchAllCommunity();
    const ecosystems$ = this.academyHttpService.fetchPrimaryEcosystemsForMenu();
    this.loaderService.start();
    forkJoin([
      trackerList$,
      accounts$,
      seniorities$,
      tdcs$,
      community$,
      ecosystems$,
    ])
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe((responses: any) => {
        if (responses[0].status === 200) {
          this.configurePaginator(responses[0].data);
          this.dataBind(responses[0].data.items);
        }

        if (responses[1].status === 200) {
          this.accounts = responses[1].data;
          this.bindAccountToFilter();
        }
        if (responses[2].status === 200) {
          this.seniorities = responses[2].data;
          this.bindSenioritiesToFilter();
        }

        if (responses[3].status === 200) {
          this.tdcs = responses[3].data;
          this.bindTdcToFilter();
        }

        if (responses[4].status === 200) {
          this.communities = responses[4].data;
          this.bindCommunityToFilter();
        }
        if (responses[5].status === 200) {
          this.ecosystems = responses[5].data;
          this.bindEcosystemToFilter();
        }
      });
  }

  fetchFilterValues(value: string) {
    this.filterValues = [];
    if (value == "Seniority") {
      if (this.seniorities.length == 0) {
        this.fetchSeniority();
      } else {
        this.bindSenioritiesToFilter();
      }
    } else if (value == "WorkingEcosystem") {
      if (this.ecosystems.length == 0) {
        this.fetchWorkingEcosystem();
      } else {
        this.bindEcosystemToFilter();
      }
    } else if (value == "Community") {
      if (this.communities.length == 0) {
        this.fetchCommunity();
      } else {
        this.bindCommunityToFilter();
      }
    } else if (value == "TDC") {
      if (this.tdcs.length == 0) {
        this.fetchTdc();
      } else {
        this.bindTdcToFilter();
      }
    } else if (value == "Client") {
      if (this.accounts.length == 0) {
        this.fetchAccount();
      } else {
        this.bindAccountToFilter();
      }
    }
  }

  bindSenioritiesToFilter() {
    for (let s = 0; s < this.seniorities.length; s++) {
      let data = new FilterDataDto();
      data.id = this.seniorities[s].level;
      data.value = this.seniorities[s].name;
      data.type = "Seniority";
      this.filterValues.push(data);
    }
  }
  bindCommunityToFilter() {
    for (let s = 0; s < this.communities.length; s++) {
      let data = new FilterDataDto();
      data.id = this.communities[s];
      data.value = this.communities[s];
      data.type = "Community";
      this.filterValues.push(data);
    }
  }
  bindEcosystemToFilter() {
    for (let s = 0; s < this.ecosystems.length; s++) {
      let data = new FilterDataDto();
      data.id = this.ecosystems[s];
      data.value = this.ecosystems[s];
      data.type = "WorkingEcosystem";
      this.filterValues.push(data);
    }
  }
  bindTdcToFilter() {
    for (let s = 0; s < this.tdcs.length; s++) {
      let data = new FilterDataDto();
      data.id = this.tdcs[s];
      data.value = this.tdcs[s];
      data.type = "TDC";
      this.filterValues.push(data);
    }
  }
  bindAccountToFilter() {
    for (let s = 0; s < this.accounts.length; s++) {
      let data = new FilterDataDto();
      data.id = this.accounts[s];
      data.value = this.accounts[s];
      data.type = "Client";
      this.filterValues.push(data);
    }
  }

  navigateToProfile(employeeId: number) {
    this.router.navigate(["/dashboard", employeeId]);
  }

  fetchAccount() {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllAccount()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.accounts = response.data;
            this.bindAccountToFilter();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  fetchSeniority() {
    this.loaderService.start();
    this.academyHttpService
      .fetchSeniorities()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.seniorities = response.data;
            this.bindSenioritiesToFilter();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  fetchTdc() {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllTdc()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.tdcs = response.data;
            this.bindTdcToFilter();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  fetchCommunity() {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllCommunity()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.communities = response.data;
            this.bindCommunityToFilter();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  fetchWorkingEcosystem() {
    this.loaderService.start();
    this.academyHttpService
      .fetchPrimaryEcosystemsForMenu()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.ecosystems = response.data;
            this.bindEcosystemToFilter();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
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
  }

  isSelected(row: any): boolean {
    const existing = Array.from(this.selectedUsers).find(
      (x) => x.employeeEmail === row.employeeEmail
    );
    return existing !== undefined;
  }

  // Called when individual checkbox is toggled
  onCheckboxChange(user: any): void {
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

  openActivityDetails(): void {
    const dialogRef = this.dialog.open(BulkActivityDialogComponent, {
      width: "600px",
      maxWidth: "60vw",
      height: "auto",
      data: {},
      panelClass: "",
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });

    dialogRef.afterClosed().subscribe((activity) => {
      if (activity) {
        const activityStartDate = new Date(activity.startDate);
        activityStartDate.setHours(12, 0, 0, 0);
        const utcStartDateString = activityStartDate.toISOString();

        const activityEndDate = new Date(activity.endDate);
        activityEndDate.setHours(12, 0, 0, 0);
        const utcEndDateString = activityEndDate.toISOString();

        // Construct the payload for the API
        const activities = Array.from(this.selectedUsers).map((user: any) => ({
          //employeeActivityId: null, // Assuming this is auto-generated by the backend
          employeeId: user.employeeId, // Ensure employeeId is available
          activityId: activity.activityId,
          activitySource: activity.activitySource,
          activityDetail: activity.activityDetail,
          comments: activity.comments,
          isActive: true,
          startDate: utcStartDateString,
          endDate: utcEndDateString,
          status: 1, // Ensure status is mapped correctly
          account: activity.account, // Assuming account is an array
        }));

        console.log("activities to save", activities);
        this.loaderService.start();
        this.academyHttpService
          .bulkActivities(activities)
          .pipe(finalize(() => this.loaderService.stop()))
          .subscribe({
            next: (res: any) => {
              if (res.status === 200) {
                this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
                this.selectedUsers.clear(); // Clear selected users after successful submission
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
      }
    });
  }

  onRemoveUser(data: any, user: any) {
    for (const item of this.selectedUsers) {
      if (item.employeeEmail === user.employeeEmail) {
        this.selectedUsers.delete(item);
      }
    }
  }

  // Add the mapStatusToId method here
  mapStatusToId(status: string): number {
    switch (status) {
      case "Pending":
        return 1;
      case "On going":
        return 3;
      case "Completed":
        return 2;
      default:
        return 0;
    }
  }
}