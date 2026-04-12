import { Component, inject, Injector, OnInit, ViewContainerRef } from '@angular/core';
import { TDC } from '../model/tdc.model';
import { Community } from '../model/community.model';
import { Seniority } from '../model/seniority.model';
import { Panel } from '../model/panel.model';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { PanelService } from '@services/panel.service';
import { SlotManagementService } from '@services/slot-management.service';
import { MatTableDataSource } from '@angular/material/table';
import { DashboardTileModel } from '../model/dashboard-tile.model';
import { PanelFilter } from '../model/panel-filter.model';
import { ChartDataModel } from '../model/chartdata.model';
import { PanelGrid } from '../model/panel-grid.model';
import { PanelDashboardList } from '../model/panel-dashboard.model';
import { NgxChartsModule, TooltipService } from '@swimlane/ngx-charts';
import { LegendPosition } from '@swimlane/ngx-charts';
import { Pagination } from '../model/pagination.model';
import { DashboardFilterModel } from '../model/dashboard-filter.model';
import { CommonModule, DatePipe } from '@angular/common';
import { PanelistGridModel } from '../interview-schedule/heatmap-table/heatmap-table.model';
import { BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { MatCardContent, MatCardModule, MatCardSubtitle } from '@angular/material/card';
import { PanelSearchBarComponent } from '../panel-search-bar/panel-search-bar.component';
import { PanelGridComponent } from '../panel-list/panel-grid/panel-grid.component';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'mf-app-dashboard',
  templateUrl: './panel-dashboard.component.html',
  styleUrls: ['./panel-dashboard.component.css'],
  providers: [DatePipe],
  standalone: true,
  imports: [
      CommonModule, 
      ReactiveFormsModule, 
      MatCardModule,
      NgxChartsModule,
      PanelSearchBarComponent,
      PanelGridComponent
    ]
})

export class PanelDashboardComponent implements OnInit {
  isPanelSearchTermVisible: boolean = false;
  communityChartData: ChartDataModel[] = [];
  panelTypeChartData: ChartDataModel[] = [];
  customColors = [
    {
      name: 'DOTNET',
      value: '#7FB3D5',
    },
    {
      name: 'JAVA',
      value: '#9D9D76',
    },
    {
      name: 'L1',
      value: '#7FB3D5',
    },
    {
      name: 'GK',
      value: '#FAD7A0',
    },
  ];

  isSideNavActive: boolean = false;
  public legendPosition: LegendPosition = LegendPosition.Below;
  legendTitlePanel = 'Panel Types';
  legendTitleCommunity = 'Communities';
  communityChartTitle = 'Community Wise Slots';
  panelTypeChartTitle = 'Panel Type Wise Slots';

  totalRecords: number = 0;
  pageNumber: number = 0;
  showPageNumber: number = 0;
  pageSize: number = 0;

  tdcs: TDC[] = [];
  communities: Community[] = [];
  senorities: Seniority[] = [];
  panels: Panel[] = [];

  range = new FormGroup({
    startDate: new FormControl(),
    endDate: new FormControl(),
  });

  // startDate = new Date(2024, 6, 1);
  // endDate = new Date(2024, 6, 30);

  startDate!: Date;
  endDate!: Date;
  slotDateRange: Date[] = [];

  reloadSubject = new BehaviorSubject(0);

  dashboardSlotFilter: DashboardFilterModel = new DashboardFilterModel();
  panelFilterModel: PanelFilter = new PanelFilter();
  public dataSource = new MatTableDataSource<any>([]);
  dashboardTileList: DashboardTileModel = {
    totalSlots: 0,
    l1Slots: 0,
    gkSlots: 0,
    l1UntilizedSlots: 0,
    gkUnutilizedSlots: 0,
    l1Deficit: 0,
    gkDeficit: 0,
  };

  panelList: PanelistGridModel[] = [];

  timeFormat = new Intl.DateTimeFormat('default', {
    hour12: false,
    hour: '2-digit',
    minute: '2-digit',
  });

  timeDisplayFormat = new Intl.DateTimeFormat('default', {
    hour12: true,
    hour: 'numeric',
  });

  panelGridList: PanelGrid[] = [];
  showChildComponent: boolean = false;

  dateStart: any;
  dateEnd: any;
  
  constructor(
    private injectorObj: Injector,
    private panelService: PanelService,
    private slotManagementService: SlotManagementService,
    private chartToolTipService: TooltipService,
    private viewContainerRef: ViewContainerRef,
    private datePipe: DatePipe
    ) {
      this.chartToolTipService = this.injectorObj.get(TooltipService);
      this.viewContainerRef = this.injectorObj.get(ViewContainerRef);
    }

  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  ngOnInit(): void {
    this.chartToolTipService.injectionService.setRootViewContainer(
      this.viewContainerRef
    );

    this.setDefaultDates();
    this.getAllDashbaordDetails();
  }

  setDefaultDates() {
    const now = new Date();
    this.dateStart = new Date(now.getFullYear(), now.getMonth(), 1);
    this.dateEnd = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    // const prevMonthLastDate = new Date(now.getFullYear(), now.getMonth(), 0);
    // const prevMonthFirstDate = new Date(
    //   now.getFullYear() - (now.getMonth() > 0 ? 0 : 1),
    //   (now.getMonth() - 1 + 12) % 12,
    //   1
    // );

    // this.dateStart = new Date(
    //   prevMonthFirstDate.getFullYear(),
    //   prevMonthFirstDate.getMonth(),
    //   prevMonthFirstDate.getDate()
    // );
    // this.dateEnd = new Date(
    //   prevMonthLastDate.getFullYear(),
    //   prevMonthLastDate.getMonth(),
    //   prevMonthLastDate.getDate()
    // );
  }

  formatDate(date: Date): string | null {
    return this.datePipe.transform(date, 'MM-dd-yyyy');
  }

  ngDoCheck(): void {
    const sideNavControl: any = document.getElementsByClassName('hamburger')[0];
    if (sideNavControl !== undefined && sideNavControl !== null) {
      if (
        sideNavControl.classList !== undefined &&
        sideNavControl.classList !== null
      ) {
        if (sideNavControl.classList.contains('open')) {
          this.isSideNavActive = true;
        } else {
          this.isSideNavActive = false;
        }
      }
    }
  }

  onSearch(panelFilter: PanelFilter) {
    
    this.dashboardSlotFilter.tDCs = panelFilter.tDCs;
    this.dashboardSlotFilter.communities = panelFilter.communities;
    this.dashboardSlotFilter.seniorities = panelFilter.seniorities;
    this.dashboardSlotFilter.panelTypes = panelFilter.panelTypes;
    this.dashboardSlotFilter.startDate = panelFilter.startDate;
    this.dashboardSlotFilter.endDate = panelFilter.endDate;

    this.getDashboardFilterData();
  }

  onPanelTypeSelect(event: any) {
    this.getPanelGridDataByPanelType(event.label);
  }

  onCommunityTypeSelect(event: any) {
    this.getCommunitySlotsDataforCards(event.label);
  }
  getCommunitySlotsDataforCards(label: any) {
    
    let communityId = this.communities
      .filter((x) => x.communityName == label)
      .map((x) => x.communityId);
    this.dashboardSlotFilter.communities = communityId;
    this.getDashboardFilterData();
  }

  pieChartLabel(series: any[], name: string): string {
    const item = series.filter((data) => data.name == name);
    if (item.length > 0) {
      return item[0].name + '-' + item[0].value;
    }
    return name;
  }

  getPanelGridDataByPanelType(panelType: string) {
    this.dashboardSlotFilter.panelTypes = [panelType];
    this.pageNumber = 1;
    this.pageSize = 10;

    this.getPanelGridData();
  }

  getAllDashbaordDetails() {
    this.dashboardSlotFilter.communities = [];
    this.dashboardSlotFilter.panelTypes = [];
    this.dashboardSlotFilter.seniorities = [];
    this.dashboardSlotFilter.tDCs = [];
    this.dashboardSlotFilter.startDate = this.dateStart;
    this.dashboardSlotFilter.endDate = this.dateEnd;
    this.getDashboardFilterData();
    this.panelService.getCommunityData().subscribe({
      next : response => this.communities = response,
      error : error=> console.log(error)
    });
  }

  getDashboardFilterData() {
    this.reloadSubject
      .asObservable()
      .pipe(
        switchMap(() =>
          this.panelService.getDashboardData(this.dashboardSlotFilter)
        )
      )
      .subscribe((response: any) => {
        if (response != null && response != undefined) {
          this.loadTiles(response.dashboardTiles);
          this.loadCommunityChartData(response.communityChartDataModel);
          this.loadPanelTypeChartData(response.panelTypeChartDataModel);
        }
      });
  }

  loadPanelTypeChartData(response: any) {
    if (response != null && response != undefined) {
      this.panelTypeChartData = response;
    } else {
      this.panelTypeChartData = [];
    }
  }

  loadCommunityChartData(response: any) {
    if (response != null && response != undefined) {
      this.communityChartData = response;
    } else {
      this.communityChartData = [];
    }
  }

  loadTiles(dashboardTiles: any) {
    if (dashboardTiles != null) {
      this.dashboardTileList = dashboardTiles;
    } else {
      this.dashboardTileList = new DashboardTileModel();
    }
  }
  onPaginationChanged(event: Pagination) {
    if (event !== undefined && event !== null) {
      this.pageSize = event.pageSize;
      this.pageNumber = event.pageIndex + 1;
      this.getPanelGridData();
    }
  }

  getPanelGridData() {
    this.panelFilterModel.tDCs = this.dashboardSlotFilter.tDCs;
    this.panelFilterModel.communities = this.dashboardSlotFilter.communities;
    this.panelFilterModel.panelTypes = this.dashboardSlotFilter.panelTypes;
    this.panelFilterModel.seniorities = this.dashboardSlotFilter.seniorities;
    this.panelFilterModel.startDate = this.dashboardSlotFilter.startDate;
    this.panelFilterModel.endDate = this.dashboardSlotFilter.endDate;

    this.panelService
      .getPanelData(this.pageSize, this.pageNumber, this.panelFilterModel)
      .subscribe((result) => {
        if (result !== undefined && result !== null && result) {
          let panelDashboardList: PanelDashboardList =
            result as PanelDashboardList;
          this.pageNumber = result.pageNumber;
          this.pageSize = result.pageSize;
          this.totalRecords = result.totalRecords;
          if (
            panelDashboardList !== undefined &&
            panelDashboardList !== null &&
            panelDashboardList.data !== undefined &&
            panelDashboardList.data !== null
          ) {
            this.panelGridList = panelDashboardList.data;
            this.showChildComponent = true;
          }
        }
      });
  }
}
