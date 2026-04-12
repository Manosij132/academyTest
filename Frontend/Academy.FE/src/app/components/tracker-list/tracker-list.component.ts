import { animate, state, style, transition, trigger } from "@angular/animations";
import { CommonModule } from "@angular/common";
import { AfterViewInit, Component, inject, OnInit, ViewChild } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
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
import { ActivatedRoute, Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { finalize, forkJoin } from "rxjs";
import { AcademyHttpService } from "@services/academy-http.service";
import { LoaderService } from "@services/loader.service";
import { SubTableComponent } from "@shared/component/sub-table/sub-table.component";
import { DataRequestOptions, FilterOption } from "@shared/dto/data-request-options.dto";
import { FilterDataDto } from "@shared/dto/filter-data-dto";
import { MatDialog } from "@angular/material/dialog";
import { ScheduleMockInterviewDialogComponent } from "@components/interview/schedule-mock-interview-dialog/schedule-mock-interview-dialog.component";
import { PrivilegedUserDirective } from "@shared/directives/privileged-user.directive";
import { Overlay } from "@angular/cdk/overlay";
import { ActivityDetailDialogComponent } from "@components/activity/activity-detail-dialog/activity-detail-dialog.component";
import { ChatUIComponent } from "@components/chat-ui/chat-ui.component";
import { GxLeaderDialogComponent } from "@components/dojo-gx-leader/gx-leader-dialog/gx-leader-dialog.component";
import { CvProfileUploadDialogComponent } from "@components/document/cv-profile-upload-dialog/cv-profile-upload-dialog.component";
import { MatTooltipModule } from "@angular/material/tooltip";
import { RouterModule } from '@angular/router';
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { AuthenticationService } from "@services/authentication.service";
import { GxMenteesDialogComponent } from "@components/dojo-gx-leader/gx-mentees-dialog/gx-mentees-dialog.component";

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
  status: string
  nestedData?: NestedElement[];
}

export interface NestedElement {
  detail: string;
  value: string;
}

export enum Position {
  ProjectManager = "Project Manager",
  OperationsManager = "Operations Manager",
  ProductManager = "Product Manager",
  DeliveryManager = "Delivery Manager",
  TechManager = "Tech Manager",
  SubjectMatterExpert = "Subject Matter Expert",
  Architect = "Architect",
  SrLevel3 = "Sr Level 3",
  SoftwareDesigner = "Software Designer",
  SrLevel2 = "Sr Level 2"
}

@Component({
  selector: "app-tracker-list",
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
    SubTableComponent,
    PrivilegedUserDirective,
    ChatUIComponent,
    MatTooltipModule,
    RouterModule,
    MatChipsModule,
    FormsModule
  ],
  templateUrl: "./tracker-list.component.html",
  styleUrls: ["./tracker-list.component.scss"],
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
export class TrackerListComponent implements OnInit, AfterViewInit {
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
      isActive: true,
      timeOnDojo: "N/A",
      trainingScore: 0,
      designation: "",
      engaged: ""
    },
  ];
  dataSource = new MatTableDataSource<PeriodicElement>();
  data: any[] = [];
  displayedColumns: string[] = [];

  expandedElement: PeriodicElement | null | undefined;
  color = '#bfd732';
  public request = new DataRequestOptions();
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  sortOptions: any[] = [];
  seniorities: any[] = [];
  communities: any[] = [];
  tdcs: any[] = [];
  accounts: any[] = [];
  ecosystems: any[] = [];
  getleaders: any[] = [];
  filterValues: FilterDataDto[] = [];
  public columns = [
    {
      colName: "",
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
      defaultSort: false,
      defaultFilter: false,
      value: "EmployeeEmail",
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
      value: "TDC",
    },
    {
      colName: "Dojo Gx Leader",
      sortAllowed: true,
      filterAllowed: true,
      defaultSort: false,
      defaultFilter: false,
      value: "ProposedDojoGxLeader",
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
      colName: "Training Completetion Score",
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
      colName: "Activity",
      sortAllowed: false,
      filterAllowed: false,
      defaultSort: false,
      defaultFilter: false,
    },
    {
      colName: "Engaged",
      sortAllowed: true,
      filterAllowed: false,
      defaultSort: true,
      defaultFilter: false,
    },
  ];
  selectedSort: string | undefined = "";
  selectedRowIndex: number | null = null;
  selectedFilters: FilterOption[] = [];
  isDojoFilterSelected = false;
  dojoAppSheetUrl =
    "https://www.appsheet.com/start/9f7d9780-82bf-466e-8f82-454dc8355c6d";
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  selectedFilterColumn: string | null = null;
  selectedFilterValue: string | null = null;
  selectedSortColumn: string | null = "EmployeeEmail";
  selectedSortDirection: string | null = "asc";
  isUserSysAdmin:  boolean = false;

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly router: Router,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService,
    private dialog: MatDialog,
    private overlay: Overlay,
    private authService: AuthenticationService
  ) {
    this.selectedSort = this.columns.filter((el) => el.defaultSort)[0].value;
  }
  columnTitles: { [key: string]: string } = {
    employeeName: "Glober",
    employeeEmail: "Email",
    seniority: "Seniority",
    community: "Community",
    tdc: "TDC",
    proposedDojoGxLeader: "Dojo Gx Leader",
    client: "Account",
    trainingScore: "Completion Score",
    action: "Action",
    result: "Result",
    activity: "Activity",
    engaged: "Engaged?",
    uploadCvProfile: "Upload",
  };

  toggleEmployee(element: any) {
    this.expandedElement = this.expandedElement === element ? null : element;
  }

  ngOnInit() {
    this.initialize();
    this.updateDisplayedColumns();
    this.updateSortOptions(); 
    const roles = this.authService.userDetails?.roles || [];
    this.isUserSysAdmin = roles?.some(role =>
    role.roleName && ['SystemAdmin'].includes(role.roleName));
  }



  updateSortOptions() {
    if (this.isDojoFilterSelected) {
      this.sortOptions = [
        { label: "Engaged", value: "Engaged" },
        { label: "Email", value: "EmployeeEmail" },
        { label: "Seniority", value: "Seniority" },
        { label: "Community", value: "Community" },
        { label: "TDC", value: "TDC" },
        { label: "Account", value: "Client" },
        { label: "Dojo Gx Leader", value: "ProposedDojoGxLeader" }
      ];

      // default sort when dojo selected
      // default sort when dojo selected
      this.selectedSortColumn = "Engaged";
      this.selectedSortDirection = "asc";

      this.request.SortOptions.SortBy = "Engaged";
      this.request.SortOptions.SortByDescending = false;

    } else {

      this.sortOptions = [
        { label: "Email", value: "EmployeeEmail" },
        { label: "Seniority", value: "Seniority" },
        { label: "Community", value: "Community" },
        { label: "TDC", value: "TDC" },
        { label: "Account", value: "Client" }
      ];

      // default sort when dojo OFF
      this.selectedSortColumn = "EmployeeEmail";
      this.selectedSortDirection = "asc";

      this.request.SortOptions.SortBy = "EmployeeEmail";
      this.request.SortOptions.SortByDescending = false;
    }
  }

  reset() {

    // Reset request object exactly like first load
    this.request = new DataRequestOptions();
    this.request.PagingOptions.PageIndex = 0;
    this.request.PagingOptions.PageSize = 20;
    this.request.SortOptions.SortBy = "EmployeeEmail";
    this.request.SortOptions.SortByDescending = false;

    // Reset UI filters
    this.selectedFilters = [];
    this.selectedFilterColumn = null;
    this.selectedFilterValue = null;
    this.filterValues = [];

    // Reset sorting UI
    this.selectedSortColumn = null;
    this.selectedSortDirection = null;

    this.request.SortOptions.SortBy = null as any;
    this.request.SortOptions.SortByDescending = false;

    // Reset search
    this.request.SearchText = "";

    // Reset dojo toggle
    this.isDojoFilterSelected = false;

    // Reset paginator UI
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }

    // Reset table expansion
    this.expandedElement = null;
    this.selectedRowIndex = null;

    // Reset columns (because dojo toggle affects them)
    this.updateDisplayedColumns();

    this.sortOptions = [
      { label: "Email", value: "EmployeeEmail" },
      { label: "Seniority", value: "Seniority" },
      { label: "Community", value: "Community" },
      { label: "TDC", value: "TDC" },
      { label: "Account", value: "Client" }
    ];

    // 🔥 REUSE FIRST PAGE LOAD LOGIC
    this.initalize();
  }

  initialize() {
    // recreate request exactly like first load
    this.request = new DataRequestOptions();
    this.request.PagingOptions.PageIndex = 0;
    this.request.PagingOptions.PageSize = 20;
    this.request.SortOptions.SortBy = "EmployeeEmail";
    this.request.SortOptions.SortByDescending = false;
    this.selectedFilters = [];
    this.isDojoFilterSelected = false;
    this.selectedSortColumn = "EmployeeEmail";
    this.selectedSortDirection = "asc";
    this.updateDisplayedColumns();
    this.initalize();
  }


  onSortColumnChanged(ev: any) {
    this.selectedSortColumn = ev.value;
    this.request.SortOptions.SortBy = ev.value;
    this.loadTrackerList();
  }

  onSortDirectionChanged(ev: any) {
    this.selectedSortDirection = ev.value;
    this.request.SortOptions.SortByDescending = ev.value === "desc";
    this.loadTrackerList();
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
            if (response.data.items && response.data.items.length > 0) {
              response.data.items.sort((a: any, b: any) =>
                a.employeeName.localeCompare(b.employeeName)
              );
            }

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
    this.dataSource.data = items;
    this.setDefaultIcon(-1);
  }

  private setDefaultIcon(index: number) {
    for (let i = 0; i < this.dataSource.data.length; i++) {
      //if (index !== i)
      //this.dataSource.data[i].icon = 'chevron_right';
    }
  }

  ngAfterViewInit() {
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

  private updateDisplayedColumns() {
    this.displayedColumns = [
      "arrow",
      "action",
      "employeeName",
      "employeeEmail",
      "seniority",
      "community",
      ...(this.isDojoFilterSelected ? ["proposedDojoGxLeader"] : []),
      "tdc",
      "client",
      "trainingScore",
      "uploadCvProfile",
      ...(this.isDojoFilterSelected ? ["engaged"] : [])
    ];
  }

  isTextTruncated(element: HTMLElement): boolean {
    return element.scrollWidth > element.clientWidth;
  }

  onSortDirOptionChanged(ev: any) {
    this.request.SortOptions.SortByDescending = ev.value == 1;
  }

  onFilterOptionChanged(ev: any) {
    this.selectedFilterColumn = ev.value;
    // clear previous selections
    this.filterValues = [];
    this.selectedFilterValue = null;
    this.fetchFilterValues(ev.value);
  }

  onFilterValueChanged(ev: any) {
    let filterOption = new FilterOption();
    filterOption.FilterBy = this.selectedFilterColumn!;
    filterOption.FilterValue = ev.value;

    const exists = this.selectedFilters.some(
      f =>
        f.FilterBy === filterOption.FilterBy &&
        f.FilterValue === filterOption.FilterValue
    );
    if (!exists) {
      this.selectedFilters.push(filterOption);
    }
    //Preserve DOJO filter if present
    const dojoFilter = this.request.FilterOptions.find(
      (x) => x.FilterValue === "DOJO"
    );

    this.request.FilterOptions = [];

    if (dojoFilter) {
      this.request.FilterOptions.push(dojoFilter);
    }

    this.request.FilterOptions.push(...this.selectedFilters);
    this.loadTrackerList();
  }
  removeFilter(filter: FilterOption) {
    this.selectedFilters = this.selectedFilters.filter(
      f =>
        !(
          f.FilterBy === filter.FilterBy &&
          f.FilterValue === filter.FilterValue
        )
    );
    // rebuild request filters (keep DOJO if exists)
    const dojoFilter = this.request.FilterOptions.find(
      (x) => x.FilterValue === "DOJO"
    );
    this.request.FilterOptions = [];
    if (dojoFilter) {
      this.request.FilterOptions.push(dojoFilter);
    }
    this.request.FilterOptions.push(...this.selectedFilters);
    this.loadTrackerList();
  }

  onToggleChange(ev: any) {
    let filterOption = new FilterOption();
    this.isDojoFilterSelected = ev.checked;
    if (ev.checked) {
      filterOption.FilterBy = "Project";
      filterOption.FilterValue = "DOJO";
      this.request.FilterOptions.push(filterOption);
      this.isDojoFilterSelected = true;
    } else {
      var dojoFilter = this.request.FilterOptions.find(
        (x) => x.FilterValue === "DOJO"
      );
      if (dojoFilter) {
        this.isDojoFilterSelected = false;
        this.request.FilterOptions = this.request.FilterOptions.filter(
          (x) => x.FilterValue !== "DOJO"
        );
        if (this.request.SortOptions.SortBy == "ProposedDojoGxLeader" ||
          this.request.SortOptions.SortBy == "Engaged") {
          this.request.SortOptions.SortBy = "EmployeeEmail";
          this.selectedSort = "EmployeeEmail";
        }
      }
    }
    this.updateDisplayedColumns();
    this.updateSortOptions();
    this.loadTrackerList();
  }

  onPageChanged(e: PageEvent) {
    this.setDefaultIcon(-1);
    this.selectedRowIndex = null;
    this.request.PagingOptions.PageIndex = e.pageIndex;
    this.request.PagingOptions.PageSize = e.pageSize;
    this.loadTrackerList();
  }

  initalize() {
    const trackerList$ = this.academyHttpService.fetchTrackerList(this.request);
    const accounts$ = this.academyHttpService.fetchAllAccount();
    const seniorities$ = this.academyHttpService.fetchSeniorities();
    const tdcs$ = this.academyHttpService.fetchAllTdc();
    const community$ = this.academyHttpService.fetchAllCommunity();
    this.loaderService.start();
    forkJoin([
      trackerList$,
      accounts$,
      seniorities$,
      tdcs$,
      community$,
    ])
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe((responses: any) => {
        if (responses[0].status === 200) {
          this.configurePaginator(responses[0].data);

          responses[0].data.items.sort((a: any, b: any) =>
            a.employeeName.localeCompare(b.employeeName)
          );

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
    else if (value == "ProposedDojoGxLeader") {
      this.bindDojoGxLeaderToFilter();
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
  bindDojoGxLeaderToFilter() {
    let dataInDojo = new FilterDataDto();
    dataInDojo.id = 1
    dataInDojo.value = "InDojo"
    dataInDojo.type = "ProposedDojoGxLeader";
    this.filterValues.push(dataInDojo);
    let dataOutDojo = new FilterDataDto();
    dataOutDojo.id = 2
    dataOutDojo.value = "OutDojo"
    dataOutDojo.type = "ProposedDojoGxLeader";
    this.filterValues.push(dataOutDojo);
    let dataNotAssigned = new FilterDataDto();
    dataNotAssigned.id = 3
    dataNotAssigned.value = "NotAssigned"
    dataNotAssigned.type = "ProposedDojoGxLeader";
    this.filterValues.push(dataNotAssigned);
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
            // console.log(response);
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

  openMockInterviewDialog(employeeId: number, employeeName: string, employeeEmail: string): void {
    const dialogRef = this.dialog.open(ScheduleMockInterviewDialogComponent, {
      data: { employeeId: employeeId, name: employeeName, email: employeeEmail },
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
  openActivityDetails(activityId: number, employeeName: string): void {
    this.dialog.open(ActivityDetailDialogComponent, {
      //width: '100%',
      width: "800px",
      maxWidth: "90vw",
      height: "auto",
      maxHeight: "50vw",
      data: { id: activityId, employeeName: employeeName },
      panelClass: "full-width-dialog",
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });
  }
  openGxLeaderDetails(element: any): void {

    this.loaderService.start();
    this.initializeMain(element.employeeEmail);
    this.academyHttpService
      .GetAllGXLeader(element.community)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.getleaders = response.data;
            const dialogRef = this.dialog.open(GxLeaderDialogComponent, {
              width: "800px",
              maxWidth: "90vw",
              height: "auto",
              data: {
                employee: element,
                leaders: this.getleaders,
                locations: this.getAllLocations,
                communities: this.communities
              },
              panelClass: "full-width-dialog",
              autoFocus: false
            });

            dialogRef.afterClosed().subscribe(result => {
              if (result != "undefined" && result != null && result.status === 200) {
                this.loadTrackerList();
              }
            });
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  selectedMentees: number[] = [];
  menteesRequest = new DataRequestOptions();
  allMentees: any;
  openGxMenteesDetails(element: any): void {
    this.menteesRequest.PagingOptions.PageSize = this.totalItems;
    this.menteesRequest.SortOptions.SortBy = "EmployeeEmail";
    this.menteesRequest.SortOptions.SortByDescending = false;
    this.menteesRequest.FilterOptions = this.request.FilterOptions;
    this.loaderService.start();
    this.initializeMain(element.employeeEmail);
    this.academyHttpService
      .fetchTrackerList(this.menteesRequest)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.allMentees = response.data.items;
            const dialogRef = this.dialog.open(GxMenteesDialogComponent, {
              width: "800px",
              maxWidth: "90vw",
              height: "auto",
              data: {
                employee: element,
                mentees: this.allMentees,
                locations: this.getAllLocations,
                communities: this.communities,
                selectedMentees: this.selectedMentees
              },
              panelClass: "full-width-dialog",
              autoFocus: false
            });

            dialogRef.afterClosed().subscribe(result => {
              if (result != "undefined" && result != null && result.status === 200) {
                this.loadTrackerList();
              }
            });
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  getAllLocations: any;
  initializeMain(employeeEmail: string) {
      const selectedMentees$ = this.academyHttpService.GetMenteesByEmail(employeeEmail);
      const community$ = this.academyHttpService.fetchAllCommunity();
      const country$ = this.academyHttpService.fetchAllTdc();
      forkJoin([community$, country$, selectedMentees$])
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe((responses: any) => {
          // if (responses[0].status === 200) {
          //   this.configurePaginator(responses[0].data);
          //   this.dataBind(responses[0].data.items);
          // }
          if (responses[2].status === 200) {
            this.communities = responses[0].data;
            this.getAllLocations = responses[1].data;
            this.selectedMentees = responses[2].data;
            // this.aiStudios = responses[3].data;
            // this.allAccounts = responses[4].data;
            // if (!this.request.aiStudio || this.request.aiStudio.length === 0) {
            //   this.accounts = [
            //     ...new Set(this.allAccounts.map((a: any) => a.account))
            //   ];
            // }
          }
        });
    }
  
  openUploadCVOrProfilePopup(employee: any) {
    this.dialog.open(CvProfileUploadDialogComponent, {
      width: "800px",
      maxWidth: "90vw",
      height: "auto",
      maxHeight: "30vw",
      data: { employee: employee },
      panelClass: "full-width-dialog",
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });
  }

   canShowAssignDojoMentees(element: any): boolean {
    if (this.isUserSysAdmin==true && (element.project=='DOJO' || element.project == 'Exusia - Dojo') && this.isGXLeader(element) ) {
      return true;
    } else {
      return false;
    }
  }

  isGXLeader(element: any): boolean {
    const isValid = Object.values(Position).some(
      v => v.toLowerCase() === element.seniority.toLowerCase());
    return isValid;
  }
}
