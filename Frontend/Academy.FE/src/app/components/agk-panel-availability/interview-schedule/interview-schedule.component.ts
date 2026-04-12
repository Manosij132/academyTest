import { Component, inject, Injector, OnInit, ViewContainerRef } from '@angular/core';
import { TDC } from '../model/tdc.model';
import { Community } from '../model/community.model';
import { Seniority } from '../model/seniority.model';
import { Panel } from '../model/panel.model';
import { FormControl, FormGroup, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PanelService } from '@services/panel.service';
import { MatTableDataSource } from '@angular/material/table';
import { DashboardTileModel } from '../model/dashboard-tile.model';
import { PanelFilter } from '../model/panel-filter.model';
import { ChartDataModel } from '../model/chartdata.model';
import { PanelGrid } from '../model/panel-grid.model';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { LegendPosition } from '@swimlane/ngx-charts';
import { DashboardFilterModel } from '../model/dashboard-filter.model';
import { DatePipe, CommonModule  } from '@angular/common';
import {
  InterviewScheduleData,
  PanelGridSlot,
  PanelistGridModel,
} from './heatmap-table/heatmap-table.model';
import { asyncScheduler, BehaviorSubject, Observable, range } from 'rxjs';
import { map, observeOn, switchMap, toArray } from 'rxjs/operators';
import { PanelSearchBarComponent } from '../panel-search-bar/panel-search-bar.component';
import { HeatmapTableComponent } from './heatmap-table/heatmap-table.component';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'mf-app-interview-schedule',
  templateUrl: './interview-schedule.component.html',
  styleUrls: ['./interview-schedule.component.scss'],
  providers: [DatePipe],
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    PanelSearchBarComponent, 
    HeatmapTableComponent,
    NgxChartsModule,
    MatSelectModule]
})
export class InterviewScheduleComponent implements OnInit {
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
    minute: '2-digit'
    
  });

  timeDisplayFormat = new Intl.DateTimeFormat('default', {
    hour12: true,
    hour: 'numeric',
    minute: 'numeric'
   
  });

  panelGridList: PanelGrid[] = [];

  dateStart: any;
  dateEnd: any;

  constructor(
    private injectorObj: Injector,
    private panelService: PanelService,
    private viewContainerRef: ViewContainerRef,
    private datePipe: DatePipe
  ) {
    //this.chartToolTipService = this.injectorObj.get(TooltipService);
    this.viewContainerRef = this.injectorObj.get(ViewContainerRef);
  }
  
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  ngOnInit(): void {
    // this.chartToolTipService.injectionService.setRootViewContainer(
    //   this.viewContainerRef
    // );

    this.setDefaultDates();
    this.getAllDashbaordDetails();
  }

  getAllDatesBetween(startDate: Date, endDate: Date): Observable<Date[]> {
    const start = startDate.getTime();
    const end = endDate.getTime();

    // Calculate how many days are between the start and end date
    const daysCount = Math.floor((end - start) / (1000 * 60 * 60 * 24)) + 1;

    // Create an observable that emits a number for each day in the range
    return range(0, daysCount).pipe(
      observeOn(asyncScheduler), // Ensures computation is done asynchronously
      map(dayOffset => {
        // For each emitted number, calculate the corresponding date
        const date = new Date(start);
        date.setDate(date.getDate() + dayOffset); // Add the day offset
        return date;
      }),
      toArray() // Collect all the emitted values into an array
    );
  }

  setDefaultDates() {
    const now = new Date();
    
    this.dateStart = new Date(now.getFullYear(), now.getMonth(), 1);
    this.dateEnd = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  
    this.getAllDatesBetween(this.dateStart, this.dateEnd).subscribe((dates: Date[]) => {
      this.slotDateRange = dates;
    });
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

  setDashboardSlotFilter(panelFilter: PanelFilter){
    this.dashboardSlotFilter.tDCs = panelFilter.tDCs;
    this.dashboardSlotFilter.communities = panelFilter.communities;
    this.dashboardSlotFilter.seniorities = panelFilter.seniorities;
    this.dashboardSlotFilter.panelTypes = panelFilter.panelTypes;
    this.dashboardSlotFilter.startDate = panelFilter.startDate;
    this.dashboardSlotFilter.endDate = panelFilter.endDate;
    this.dashboardSlotFilter.searchTerm = panelFilter.searchTerm;
  }

  onSearch(panelFilter: PanelFilter) {
    this.setDashboardSlotFilter(panelFilter);
    this.getDashboardFilterData();

    this.getAllDatesBetween(new Date(this.dashboardSlotFilter.startDate), new Date(this.dashboardSlotFilter.endDate)).subscribe((dates: Date[]) => {
      this.slotDateRange = dates;
    });
  }

  onResetFilters(panelFilter: PanelFilter){
    this.setDashboardSlotFilter(panelFilter);
    this.getDashboardFilterData();
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

  getAllDashbaordDetails() {
    this.dashboardSlotFilter.communities = [];
    this.dashboardSlotFilter.panelTypes = [];
    this.dashboardSlotFilter.seniorities = [];
    this.dashboardSlotFilter.tDCs = [];
    this.dashboardSlotFilter.startDate = this.dateStart;
    this.dashboardSlotFilter.endDate = this.dateEnd;

    this.getDashboardFilterData();
  }

  getDashboardFilterData() {
    this.reloadSubject
      .asObservable()
      .pipe(
        switchMap(() =>
          this.panelService.GetInterviewPanelDetails(this.dashboardSlotFilter)
        )
      )
      .subscribe((response: any) => {
        if (response != null && response != undefined) {
          this.loadTiles(response.dashboardTiles);
          this.loadCommunityChartData(response.communityChartDataModel);
          this.loadPanelTypeChartData(response.panelTypeChartDataModel);
          this.loadInterViewPanelTableData(this.dashboardSlotFilter.searchTerm ?? '', response.interviewScheduleData);
        }
      });
  }

  getPanelSlotGridModel(
    searchTerm: string,
    panelists: InterviewScheduleData[]
  ) {
    const normalizedSearchString = searchTerm.toLowerCase();

    return panelists
      .filter(
        (x) =>
          normalizedSearchString.length == 0 ||
          x.primaryPanel.toLowerCase().includes(normalizedSearchString) ||
          x.emailId.toLowerCase().includes(normalizedSearchString)
      )
      .map((panelist) => {
        const panelSlotGridModel = panelist.slots.reduce<
          Record<string, PanelGridSlot>
        >((accumulator, current) => {
          const date = new Date(current.slotDate.replace(" ","T")+"Z");
          accumulator[date.toLocaleDateString()] = {
            time: this.timeFormat.format(date),
            timeDisplay: this.timeDisplayFormat.format(date).toUpperCase(),
            type: current.isUtilized ? 'Utilised' : 'Unutilised',
            id: current.id,
          };
          return accumulator;
        }, {});

        return {
          panelId: panelist.panelId,
          panel: panelist.panel,
          primaryPanel: panelist.primaryPanel,
          upToSeniority: panelist.upToSeniority,
          communityName: panelist.communityName,
          emailId: panelist.emailId,
          slots: panelSlotGridModel,
        };
      });
  }

  loadPanelTypeChartData(response: any) {
    if (response != null && response != undefined) {
      this.panelTypeChartData = response;
    } else {
      this.panelTypeChartData = [];
    }
  }

  loadInterViewPanelTableData(searchTerm: string, response: any) {
    if (response != null && response != undefined) {
      this.panelList = this.getPanelSlotGridModel(searchTerm, response);
    } else {
      this.panelList = [];
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
}
