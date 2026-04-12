import { Component, DoCheck, inject, OnInit } from "@angular/core";
import { TDC } from "../model/tdc.model";
import { Community } from "../model/community.model";
import { Seniority } from "../model/seniority.model";
import { Panel } from "../model/panel.model";
import { PanelGrid } from "../model/panel-grid.model";
import { PanelService } from '@services/panel.service';
import { PanelFilter } from "../model/panel-filter.model";
import { PanelDashboardList } from "../model/panel-dashboard.model";
import { Pagination } from "../model/pagination.model";
import { PanelSendEmailPopupComponent } from "./panel-send-email-popup/panel-send-email-popup.component";
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatButtonToggleModule } from "@angular/material/button-toggle";
import { PanelGridComponent } from "./panel-grid/panel-grid.component";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: 'mf-app-panels',
  templateUrl: './panel-list.component.html',
  styleUrls: ['./panel-list.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatButtonToggleModule,
    PanelGridComponent,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    MatInputModule
  ]
})

export class PanelListComponent implements OnInit, DoCheck {
  
  tdcs: TDC[] = [];
  communities: Community[] = [];
  senorities: Seniority[] = [];
  panels: Panel[] = [];
  isSideNavActive: boolean = false;
  isAvailable: boolean = true;
  panelGridList : PanelGrid[] = [];
  showChildComponent : boolean = false;

  pageNumber:number = 0;
  pageSize:number = 0;
  totalRecords : number = 0;
  searchText : string = "";

  panelFilter : PanelFilter = new PanelFilter();
  dialog: any;
  dateStart: Date = new Date();
  dateEnd: Date = new Date();

  calendarStartDate: Date = this.dateStart;

  range = new FormGroup({
    startDate: new FormControl,
    endDate: new FormControl,
  });

  constructor(private panelService:PanelService)
  { }

  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  ngOnInit(): void {
    
    this.getCommunities();
    this.getAllPanel();
    this.getTDCs();
    this.getSeniorities();
    this.getPanelGridList();
  }
  setDefaultDates() {
    // let today = new Date();
    // var day = today.getDay();
    // var diff = today.getDate() - day + (day == 0 ? -6 : 1) - 8; // adjust when day is sunday
    // this.dateStart = new Date(new Date().setDate(diff));
    // this.dateEnd = new Date(new Date().setDate(diff + 6));
    // //this.dateStart = new Date(2024, 5, 1);
    // //this.dateEnd = new Date(2024,5,30); 
    // if(this.dateStart.getDate() > this.dateEnd.getDate())
    // {
    //    this.dateEnd = new Date(today.setMonth(today.getMonth() + 1));
    // }
  
    var now = new Date();
    // var prevMonthLastDate = new Date(now.getFullYear(), now.getMonth(), 0);
    // var prevMonthFirstDate = new Date(now.getFullYear() - (now.getMonth() > 0 ? 0 : 1), (now.getMonth() - 1 + 12) % 12, 1);

    // this.dateStart = new Date(prevMonthFirstDate.getFullYear(), prevMonthFirstDate.getMonth(), prevMonthFirstDate.getDate());
    // this.dateEnd = new Date(prevMonthLastDate.getFullYear(), prevMonthLastDate.getMonth(), prevMonthLastDate.getDate());

    this.dateStart = new Date(now.getFullYear(), now.getMonth(), 1);
    this.dateEnd = new Date(now.getFullYear(), now.getMonth() + 1, 0);

  }


  ngDoCheck(): void {
    const sideNavControl: any = document.getElementsByClassName('hamburger')[0];
    if (sideNavControl !== undefined && sideNavControl !== null) {
      if (sideNavControl.classList !== undefined && sideNavControl.classList !== null) {
        if (sideNavControl.classList.contains('open')) {
          this.isSideNavActive = true;
        }
        else {
          this.isSideNavActive = false;
        }
      }
    }
  }

  openSendEmailDialog(): void{
    const dialogRef = this.dialog.open(PanelSendEmailPopupComponent, {
      data: { panel: this.panelGridList}, width: '700px', height: '100%'
      
    });
  }

  getSeniorities(){
    this.panelService.getSeniorityData().subscribe((res) => {
      if(res !== undefined && res !== null){
        this.senorities = res;
      }
    })
  }

  getCommunities(){
    this.panelService.getCommunityData().subscribe((res) => {
      if(res !== undefined && res !== null){
        this.communities = res;
      }
    })
  }

  getAllPanel(){
    this.panelService.getAllPanelData().subscribe((res) => {
      if(res !== undefined && res !== null){
        this.panels = res;
      }
    })
  }

  getTDCs(){
    this.panelService.getTDCData().subscribe((res) => {
      if(res !== undefined && res !== null){
        this.tdcs = res;
      }
    })
  }

  getPanelGridData()
  {
    this.panelService.getPanelData(this.pageSize,this.pageNumber,this.panelFilter).subscribe(result => {
      if(result !== undefined && result !== null && result){
        let panelDashboardList : PanelDashboardList = result as PanelDashboardList;
        this.pageNumber = result.pageNumber;
        this.pageSize = result.pageSize;
        this.totalRecords = result.totalRecords;
        if(panelDashboardList !== undefined && panelDashboardList !== null && panelDashboardList.data !== undefined && panelDashboardList.data !== null){
          if(this.panelGridList.length > 0){
            this.panelGridList = [];
          }
          this.panelGridList = panelDashboardList.data;
          this.showChildComponent = true;
        } 
      }
    });
  }

  onTDCChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }){
    if(this.panelFilter.tDCs == undefined || this.panelFilter.tDCs == null){
      this.panelFilter.tDCs = [];
    }
    if (event.isUserInput) {
      if (event.source.selected === true) {
        let selectedTDC = event.source.value;
        this.panelFilter.tDCs.push(selectedTDC);
        this.getPanelGridData()
      } else {
        let deSelectedTDC = event.source.value;
        if(this.panelFilter.tDCs.length > 0){
          const index = this.panelFilter.tDCs.indexOf(deSelectedTDC);
          if (index > -1) { 
            this.panelFilter.tDCs.splice(index, 1); 
            this.getPanelGridData();
          }
        }
      }
    }
  }

  onCommunityChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }){
    if(this.panelFilter.communities == undefined || this.panelFilter.communities == null){
      this.panelFilter.communities = [];
    }
    if (event.isUserInput) {
      if (event.source.selected === true) {
        let selectedCommunity = event.source.value;
        this.panelFilter.communities.push(selectedCommunity);
        this.getPanelGridData();
      } else {
        let deSelectedCommunity = event.source.value;
        if(this.panelFilter.communities.length > 0){
          const index = this.panelFilter.communities.indexOf(deSelectedCommunity);
          if (index > -1) { 
            this.panelFilter.communities.splice(index, 1); 
            this.getPanelGridData();
          }
        }
      }
    }
  }

  onSeniorityChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }){
    if(this.panelFilter.seniorities == undefined || this.panelFilter.seniorities == null){
      this.panelFilter.seniorities = [];
    }
    if (event.isUserInput) {
      if (event.source.selected === true) {
        let selectedSeniority = event.source.value;
        this.panelFilter.seniorities.push(selectedSeniority);
        this.getPanelGridData();
      } else {
        let deSelectedSeniority = event.source.value;
        if(this.panelFilter.seniorities.length > 0){
          const index = this.panelFilter.seniorities.indexOf(deSelectedSeniority);
          if (index > -1) { 
            this.panelFilter.seniorities.splice(index, 1); 
            this.getPanelGridData();
          }
        }
      }
    }
  }

  onPanelChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }){
    if(this.panelFilter.panelTypes == undefined || this.panelFilter.panelTypes == null){
      this.panelFilter.panelTypes = [];
    }
    if (event.isUserInput) {
      if (event.source.selected === true) {
        let selectedPanel = event.source.value;
        this.panelFilter.panelTypes.push(selectedPanel);
        this.getPanelGridData();
      } else {
        let deSelectedPanel = event.source.value;
        if(this.panelFilter.panelTypes.length > 0){
          const index = this.panelFilter.panelTypes.indexOf(deSelectedPanel);
          if (index > -1) { 
            this.panelFilter.panelTypes.splice(index, 1); 
            this.getPanelGridData();
          }
        }
      }
    }
  }

  onDateChange(event: any) {
    if (event != undefined 
      && event.controls['startDate'].value != undefined 
      && event.controls['startDate'].value != null
      && event.controls['endDate'].value != undefined 
      && event.controls['endDate'].value != null) {    
      this.panelFilter.startDate = event.controls['startDate'].value.toDateString();
      this.panelFilter.endDate = event.controls['endDate'].value.toDateString();
      this.calendarStartDate = new Date(this.panelFilter.startDate);
      this.getPanelGridData();
    }
    else
    {
      this.panelFilter.startDate = "";
      this.panelFilter.endDate = "";
    }
  }

  isToggleButtonChange(event: any) {
    if (event !== undefined && event !== null) {
      this.isAvailable = event.value;
      if(this.isAvailable){
        this.panelFilter.availableSlots = true;
        this.panelFilter.isDeficit = false;
      }
      else{
        this.panelFilter.isDeficit = true;
        this.panelFilter.availableSlots = false;
      }
      this.getPanelGridData();
    }
  }

  applySearchFilter(){
    if(this.searchText !== undefined && this.searchText !== null){
      this.panelFilter.searchTerm = this.searchText;
    }
    else
    {
      this.panelFilter.searchTerm = "";
    }
    this.getPanelGridData();
  }

  onPaginationChanged(event : Pagination){
    
    if(event !== undefined && event !== null){
      this.pageSize = event.pageSize;
      this.pageNumber = event.pageIndex + 1;
      this.getPanelGridData();
    }
  }

  getPanelGridList(){
    this.setDefaultDates();
    this.panelFilter.startDate = this.dateStart.toDateString();
    this.panelFilter.endDate = this.dateEnd.toDateString();
    this.isAvailable = true
    this.panelFilter.availableSlots = true;
    this.panelFilter.isDeficit = false;
    this.pageNumber = 1;
    this.pageSize = 10;

    this.getPanelGridData();
  }
}
