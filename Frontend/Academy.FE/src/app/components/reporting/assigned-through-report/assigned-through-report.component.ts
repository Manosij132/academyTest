import { CommonModule } from "@angular/common";
import { Component, inject, OnInit, ViewChild } from "@angular/core";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatPaginator, MatPaginatorModule, PageEvent } from "@angular/material/paginator";
import { MatSelectModule,MatSelect } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { ToastrService } from "ngx-toastr";
import { finalize, forkJoin } from "rxjs";
import { ActivatedRoute } from "@angular/router";
import { AcademyHttpService } from "@services/academy-http.service";
import { LoaderService } from "@services/loader.service";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE, MatNativeDateModule ,MatOptionSelectionChange} from "@angular/material/core";
import { FormsModule } from "@angular/forms";
import { AssignedThroughReportRequest } from "@shared/Interface/Assigned-Through-Report-Request";
import { MAT_MOMENT_DATE_ADAPTER_OPTIONS, MomentDateAdapter } from "@angular/material-moment-adapter";
import { MY_UTC_FORMATS } from "@shared/constants/reporting.constants";

@Component({
  selector: "app-report",
  standalone: true,
  imports: [CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    FormsModule,
    MatSlideToggleModule
  ],
  providers: [
    { provide: DateAdapter, useClass: MomentDateAdapter },
    { provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: true } },
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    { provide: MAT_DATE_FORMATS, useValue: MY_UTC_FORMATS },
  ],
  templateUrl: "./assigned-through-report.component.html",
  styleUrl: "./assigned-through-report.component.scss",
})
export class AssignedThroughReportComponent implements OnInit {
  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService
  ) { }
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];
  dataSource = new MatTableDataSource<any>();
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild('communitySelect') communitySelect!: MatSelect;
  @ViewChild('countrySelect') countrySelect!: MatSelect;
  @ViewChild('aiStudioSelect') aiStudioSelect!: MatSelect;
  @ViewChild('accountSelect') accountSelect!: MatSelect;
  data: any[] = [];
  reportType: string[] = ["Primary", "All"]
  activityCountdata: any[] = [];
  assignedThroughTrainingCount: number = 0;
  notAssignedThroughTrainingCount: number = 0;
  isStartDateSelected: boolean = false;
  selectedRowIndex: number | null = null;
  isVissible: boolean = true;
  isExport: boolean = false;
  exportUrl!: string;
  public totalPages = 0;
  public totalItems = 0;
  public pageSize = 25;
  public pageIndex = 0;
  selectedCommunity: string[] = [];
  communities: any[] = [];
  tdc: any[] = [];
  selectedTdc: string[]=[];
 accounts: string[] = [];
  allAccounts: { aiStudio: string; account: string }[] = [];
  aiStudios: any[] = [];
  selectedAccount: string[] = [];
  selectedAiStudio: string[] = [];
  selectedReportType: any;
  startDate: any = "";
  endDate: any = "";
  appliedStartDate: Date | null = null;
  appliedEndDate: Date | null = null;
  displayedColumns: string[] = [
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
  displayedActivityCount: string[] = [
    "activityName",
    "activityCount"
  ]
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
    ticketNumber: "Ticket"
  };
  color = "#bfd732";
  request: AssignedThroughReportRequest = {
    pageIndex: 1,
    pageSize: 25,
    country: [],
    community: [],
    aiStudio: [],
    account: [],
    dojoStartDate: "",
    dojoEndDate: "",
    isPrimaryRecord: true,
    searchText: ""
  };
  exportRequest: any = {
    detailedReport: [],
    filter: {
      country: [] as string[],
      community: [] as string[],
      aiStudio: [] as string[],
      account: [] as string[], 
      dojoStartDate: "",
      dojoEndDate: "",
    },
    reportCounts: []
  };
  ngOnInit(): void {
    this.initializeMain();
  }
ngAfterViewInit() {
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
  }
  initializeMain() {
    const trackerList$ = this.academyHttpService.fetchAssignedThroughTrainingReportList(
      this.request
    );
    //const community$ = this.academyHttpService.fetchAllCommunity();
    const tdcCommunity$ = this.academyHttpService.fetchAllTdcCommunityForDojo();
    this.loaderService.start();
    forkJoin([trackerList$, tdcCommunity$])
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe((responses: any) => {
        if (responses[0].status === 200) {
          interface ApiResponse {
            success: boolean;
            data: {
              items: any[];
              exportItems: any[];
              activityCounts: any[];
              totalPages: number;
              pageSize: number;
              pageIndex: number;
              totalCount: number;
              assignedThroughTrainingCount: number;
              notAssignedThroughTrainingCount: number;
            };
            error?: string;
          }
          const apiResponse = responses[0] as ApiResponse;
          if (apiResponse.success) {
            this.configurePaginator(apiResponse.data);
            this.dataBind(apiResponse.data.items);
            this.activityDataBind(apiResponse.data.activityCounts);
            this.setAssignedThroughTrainingCount(apiResponse.data.assignedThroughTrainingCount, apiResponse.data.notAssignedThroughTrainingCount);
            this.exportDataBind(apiResponse.data.exportItems, apiResponse.data.activityCounts);
          } else {
            this.toastr.error(
              apiResponse.error || "Unknown error",
              "Error"
            );
          }
        }
        if (responses[1].status === 200) {
          this.communities = responses[1].data.communities;
          this.tdc = responses[1].data.countries;
           this.aiStudios = responses[1].data.aiStudios;
          this.allAccounts = responses[1].data.accounts;
          // Only reset accounts if no AI Studio selected
          if (!this.request.aiStudio || this.request.aiStudio.length === 0) {
            this.accounts = [
              ...new Set(this.allAccounts.map((a: any) => a.account))
            ];
          }
        }
      });
  }


  reset(): void {
    this.request = {
      pageIndex: 1,
      pageSize: 25,
      country: [],
      community: [],
      aiStudio: [],
      account: [],
      dojoStartDate: "",
      dojoEndDate: "",
      isPrimaryRecord: true,
      searchText: ""
    };
    this.selectedCommunity = [];
    this.selectedTdc = [];
    this.selectedAiStudio =[];
    this.selectedAccount = [];
    this.isVissible = true;
    this.selectedReportType = "";
    this.startDate = null;
    this.endDate = null;    
    this.appliedStartDate = null;
    this.appliedEndDate = null;
    this.initializeMain();
  }

  search() {
    this.request.pageIndex = 1;
    this.appliedStartDate = this.startDate;
    this.appliedEndDate = this.endDate;
    this.initializeMain();
  }

  export() {
    this.exportRequest.fileType = "text/plain"
    this.exportReport();
  }

  exportReport() {
    this.isExport = true;
    this.loaderService.start();
    this.academyHttpService
      .exportAssignedThroughTrainingReport(this.exportRequest)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response) => {
          interface ApiResponse {
            success: boolean;
            data: {
              fileUrl: string;
            };
            error?: string;
          }

          const apiResponse = response as ApiResponse;
          if (apiResponse.success) {
            this.exportUrl = apiResponse.data.fileUrl;
          } else {
            this.toastr.error(
              apiResponse.error || "Unknown error",
              "Error"
            );
          }
        },
      });
  }

onCommunityChanged(option: MatOptionSelectionChange) {
    if (!option.isUserInput) {
      return;
    }
    const value = option.source.value;
    const isChecked = option.source.selected;
    if (value === 'ALL') {
      if (isChecked) {
        this.selectedCommunity = ['ALL', ...this.communities];
      } else {
        this.selectedCommunity = [];
      }
    } else {
      if (isChecked) {
        this.selectedCommunity = [...new Set([...this.selectedCommunity, value])];
      } else {
        this.selectedCommunity = this.selectedCommunity.filter(v => v !== value);
      }
      const allSelected = this.communities.every(c =>
        this.selectedCommunity.includes(c)
      );
      this.selectedCommunity = allSelected
        ? ['ALL', ...this.communities]
        : this.selectedCommunity.filter(v => v !== 'ALL');
    }
    this.request.community = this.selectedCommunity.includes('ALL') ? [] : this.selectedCommunity;
    this.exportRequest.filter.community = this.selectedCommunity.includes('ALL') ? [] : this.selectedCommunity;
    this.isExport = false;
  }
  onDojoStartDateChange(event: any) {
    const startDate = event.value;
    this.request.dojoStartDate = startDate;
    this.exportRequest.filter.dojoStartDate = startDate;
    this.isStartDateSelected = true;
    this.request.dojoEndDate = new Date().toISOString().slice(0, 10);
    this.isExport = false;
  }
  onDojoEndDateChange(event: any) {
    const endDate = event.value;
    this.request.dojoEndDate = endDate;
    this.exportRequest.filter.dojoEndDate = endDate;
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
    this.exportRequest.filter.country = this.selectedTdc.includes('ALL') ? [] : this.selectedTdc;
    this.isExport = false;
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

    this.exportRequest.filter.aiStudio = this.request.aiStudio;
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
    this.exportRequest.filter.account = [];
    this.isExport = false;
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

    this.exportRequest.filter.account = this.request.account;

    this.isExport = false;
  }

  onPageChanged(e: PageEvent) {
    this.selectedRowIndex = null;
    this.request.pageIndex = e.pageIndex + 1;
    this.request.pageSize = e.pageSize;
    this.initializeMain();
  }

  onToggleChange(event: any) {
    if (event.checked) {
      this.isVissible = false;
    }
    else {
      this.isVissible = true;
    }
  }
  private configurePaginator(data: any) {
    this.totalPages = data.totalPages;
    this.pageSize = data.pageSize;
    this.pageIndex = data.pageIndex;
    this.totalItems = data.totalCount;
  }

  private dataBind(items: any[]): void {
    this.data = items;
  }

  private exportDataBind(detailedReport: any[], activitySummary: any[]): void {
    this.exportRequest.detailedReport = detailedReport;
    this.exportRequest.reportCounts = [
      { name: 'Assigned Through Training Globers', count: this.assignedThroughTrainingCount },
      { name: 'Not Assigned Through Training Globers', count: this.notAssignedThroughTrainingCount },
      { name: 'Total Globers', count: this.totalItems }
    ]
  }

  private activityDataBind(activityCounts: any[]): void {
    this.activityCountdata = activityCounts;
  }

  private setAssignedThroughTrainingCount(assignedThroughTrainingCount: number, notAssignedThroughTrainingCount: number): void {
    this.assignedThroughTrainingCount = assignedThroughTrainingCount;
    this.notAssignedThroughTrainingCount = notAssignedThroughTrainingCount;
  }
  shouldNotShowColumn(column: string): boolean {
    return column !== 'assignedThroughTraining' &&
      column !== 'isActive' &&
      column !== 'dojoStartDate' &&
      column !== 'dojoEndDate' &&
      column !== 'startDate' &&
      column != 'endDate';
  }
}
