import { Component, inject, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { TDC } from '../model/tdc.model';
import { Community } from '../model/community.model';
import { Seniority } from '../model/seniority.model';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { SlotManagementModel } from '../model/slot-management.model';
import { Panel } from '../model/panel.model';
import { PanelService } from '@services/panel.service';
import { SlotManagementService } from '@services/slot-management.service';
import { SlotManagementFilter } from '../model/slot-mgmt-filter.model';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { CommunitySelectionRatio } from '../model/community-selection-ratio.model';
import { MatDialog } from '@angular/material/dialog';
import { EditSlotmodelComponent } from '../slot-management/edit-slot-model-popup/edit-slot-model.component';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSortModule } from '@angular/material/sort';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'mf-app-slot-management',
  templateUrl: './slot-management.component.html',
  styleUrls: ['./slot-management.component.css'],
  providers: [DatePipe],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatIconModule,
    MatTooltipModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule
  ]
})
export class SlotManagementComponent implements OnInit {
  tdcs: TDC[] = [];
  communities: Community[] = [];
  senorities: Seniority[] = [];
  panels: Panel[] = [];

  PanelAll: Panel = {
    id: -1,
    name: "All"
  }
  selectedPanelTypes : string[] = [];

  public dataSource = new MatTableDataSource<any>([]);
  public reportData: any[] = [];
  displayedColumns: string[] = ['seniority', 'l1SlotsRequired', 'L1SelectionRatio', 'l1Slots', 'l1panels', 'gkSlotsRequired', 'GKSelectionRatio', 'gkSlots','GKpanels', 'L1shortFall', 'GKshortFall', 'riskIndicator', 'isEditing'];
  @ViewChild(MatPaginator, { read: true })
  paginator!: MatPaginator;

  selectedCommunityId: number | undefined;
  selectTDC: string | undefined;
  slotManagementList: SlotManagementModel[] = [];

  slotManagementModel: SlotManagementModel = new SlotManagementModel();

  slotmanagementFilter: SlotManagementFilter = new SlotManagementFilter();
  isSideNavActive:boolean = false;

  range = new FormGroup({
    startDate: new FormControl,
    endDate: new FormControl,
  });


  events: string[] = [];
  communitySelectionRatio: CommunitySelectionRatio = {
    tdc:"",
    communityId:0,
    startDate: new Date,
    endDate: new Date,
    l1SelectionRatio: undefined,
    gkSelectionRatio: undefined
  };

  l1ApplyChecked: boolean = false;
  gkApplyChecked: boolean = false;
  l1Visible: boolean = false;
  GKVisible: boolean = false;

  dateStart: any;
  dateEnd: any;
  isRatioEdit: boolean = true;
  editRatioButtonName: string = "Edit";
  isEditDisable: boolean = true;
  riskIndicator: string = "";

  modeselect = -1;

  constructor(
    public dialog: MatDialog, 
    private slotManagementService: SlotManagementService, 
    private panelService: PanelService, 
    private datePipe: DatePipe
  ) { }

  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  ngOnInit(): void {
    this.getCommunities();
    this.getTDCs();
    this.getSeniorities();
    this.getAllPanel();
    this.setDefaultDates();
    this.slotmanagementFilter.startDate = this.dateStart.toDateString();
    this.slotmanagementFilter.endDate = this.dateEnd.toDateString();
    
  }
  setDefaultDates() {
    // let today = new Date();
    // var day = today.getDay(),
    // diff = today.getDate() - day + (day == 0 ? -6 : 1) - 8; // adjust when day is sunday
    // this.dateStart = new Date(new Date().setDate(diff));
    // this.dateEnd = new Date(new Date().setDate(diff + 6));
    // if(this.dateStart.getDate() > this.dateEnd.getDate())
    // {
    //   this.dateEnd = new Date(today.setMonth(today.getMonth() + 1));
    // }

    var now = new Date();
    var prevMonthLastDate = new Date(now.getFullYear(), now.getMonth(), 0);
    var prevMonthFirstDate = new Date(now.getFullYear() - (now.getMonth() > 0 ? 0 : 1), (now.getMonth() - 1 + 12) % 12, 1);

    this.dateStart = new Date(prevMonthFirstDate.getFullYear(), prevMonthFirstDate.getMonth(), prevMonthFirstDate.getDate());
    this.dateEnd = new Date(prevMonthLastDate.getFullYear(), prevMonthLastDate.getMonth(), prevMonthLastDate.getDate());
  }

  getAllPanel(){
    this.panelService.getAllPanelData().subscribe((res) => {
      if(res !== undefined && res !== null){
        this.panels = res;
        this.panels.push(this.PanelAll);
        this.panels.sort((a,b) => a.name.localeCompare(b.name));
      }
    })
  }

  onPanelChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }){
    if (event.source.selected === true) {

      let selectedPanelType = event.source.value;
      if (selectedPanelType == "L1")
        {
          this.l1Visible = true;
          this.displayedColumns = ['seniority', 'l1SlotsRequired', 'L1SelectionRatio', 'l1Slots', 'l1panels', 'L1shortFall', 'riskIndicator', 'isEditing'];
          this.dataSource.data.forEach(item => {        
            if(item.offersToBeRolledOut != undefined && item.offersToBeRolledOut != "")
              {
                item.riskIndicator = this.calculateRiskIndicatorColorPercentage(item.l1SlotsRequired, item.l1SlotsActual,0,0);
              }  
          });
          this.dataSource.data.sort((a, b) => a.l1SlotsRequired < b.l1SlotsRequired ? 1 : a.l1SlotsRequired > b.l1SlotsRequired ? -1 : 0)
        }
        else
        {
          this.l1Visible = false;
        }
        if(selectedPanelType == "GK")
          {
            this.GKVisible = true;
            this.displayedColumns = ['seniority', 'gkSlotsRequired', 'GKSelectionRatio', 'gkSlots','GKpanels', 'GKshortFall', 'riskIndicator', 'isEditing'];
            this.dataSource.data.forEach(item => {        
              if(item.offersToBeRolledOut != undefined && item.offersToBeRolledOut != "")
                {
                  item.riskIndicator = this.calculateRiskIndicatorColorPercentage(0, 0,item.gkSlotsRequired, item.gkSlotsActual);
                }  
            });
            this.dataSource.data.sort((a, b) => a.gkSlotsRequired < b.gkSlotsRequired ? 1 : a.gkSlotsRequired > b.gkSlotsRequired ? -1 : 0)
          }
          else
          {
            this.GKVisible = false;
          }
          if(selectedPanelType == "-1")
            {
              this.displayedColumns = ['seniority', 'l1SlotsRequired', 'L1SelectionRatio', 'l1Slots', 'l1panels', 'gkSlotsRequired', 'GKSelectionRatio', 'gkSlots','GKpanels', 'L1shortFall', 'GKshortFall', 'riskIndicator', 'isEditing'];
              this.l1Visible = true;
              this.GKVisible = true;
              this.dataSource.data.forEach(item => {        
                if(item.offersToBeRolledOut != undefined && item.offersToBeRolledOut != "")
                  {
                    item.riskIndicator = this.calculateRiskIndicatorColorPercentage(item.l1SlotsRequired, item.l1SlotsActual,item.gkSlotsRequired, item.gkSlotsActual);
                  }  
              });
              this.dataSource.data.sort((a, b) => a.l1SlotsRequired < b.l1SlotsRequired ? 1 : a.l1SlotsRequired > b.l1SlotsRequired ? -1 : 0)
            }
    }
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

  openEditDialog(item : any): void {
    
    const dialogRef = this.dialog.open(EditSlotmodelComponent, {
      data: { seniority: item, selectionRatio: this.communitySelectionRatio, isL1Visible: this.l1Visible, isGKVisible: this.GKVisible}, width: '400px',  
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {   
        if(this.l1Visible)
          {
            item.l1SelectionRatio = result.l1SelectionRatio;
          }
        if(this.GKVisible)
          {
            item.gkSelectionRatio = result.gkSelectionRatio;
          }
          item.positionToBeFilled = result.positionToBeFilled;
          item.dropRatio = result.dropRatio;
          item.offersToBeRolledOut = result.offersToBeRolledOut;
        
          this.performCalculation(item);
          if (!item.isEditing) {
            this.slotManagementModel = item;
          }
        // Set isEditable property to false for all items except the one being edited
        this.editableGridLogic(item);
      }
    });
  }
  
  getCommunities() {
    this.panelService.getCommunityData().subscribe((res) => {
      if (res !== undefined && res !== null) {
        this.communities = res;
      }
    })
  }

  getTDCs() {
    this.panelService.getTDCData().subscribe((res) => {
      if (res !== undefined && res !== null) {
        this.tdcs = res;
      }
    })
  }

  getSeniorities() {
    this.panelService.getSeniorityData().subscribe((res) => {
      if (res !== undefined && res !== null) {
        this.senorities = res;
      }
    })
  }

  navigateToSubTopics(topicData: any) {
    return null
  }

  onPaginateChange(event: any) {
    this.dataSource = new MatTableDataSource<SlotManagementModel>(this.slotManagementList);
    this.dataSource.paginator = this.paginator;
  }

  getSlotManagementDetails() {
    this.slotManagementService.getAllSlotManagementData(this.slotmanagementFilter).subscribe((response: any) => {
      if (response != null && response != undefined) {  
        this.dataSource = new MatTableDataSource<SlotManagementModel>(response);
        this.dataSource.data.forEach(item => {    

          item.seniority = this.getSeniorityName(item.seniorityId);
          if (item.l1SlotsRequired > 0)
            {
              item.shortFallL1 = item.l1SlotsRequired - item.l1SlotsActual;
            }
         if(item.gkSlotsRequired > 0)
          {
            item.shortFallGK = item.gkSlotsRequired - item.gkSlotsActual;
          }
          
          item.riskIndicator = this.calculateRiskIndicatorColorPercentage(item.l1SlotsRequired, item.l1SlotsActual, item.gkSlotsRequired, item.gkSlotsActual);
          
        });
        this.slotManagementList = this.dataSource.data;
        this.senorities.forEach(item => { 
        if (!this.slotManagementList.some(record => record.seniorityId === item.id )) {
          const newRecord = { seniorityId: item.id, seniority: item.name,communityId : this.slotmanagementFilter.communityID, tdc : this.slotmanagementFilter.tDCs};
          this.dataSource.data.push(newRecord);
          this.dataSource.data.sort((a, b) => a.l1SlotsRequired < b.l1SlotsRequired ? 1 : a.l1SlotsRequired > b.l1SlotsRequired ? -1 : 0)
          
        }
        });
        
      }
    });
  }

  calculateRiskIndicatorColorPercentage(l1RequiredSlot: number, l1ActualSlot: number, gkRequiredSlot: number, gkActualSlot: number): string {
    const percentage = ((l1ActualSlot + gkActualSlot) / (l1RequiredSlot + gkRequiredSlot)) * 100;

    if (percentage <= 50) {
      return 'Red';
    } else if (percentage > 50 && percentage < 75) {
      return 'Amber';
    } else if (percentage >= 75 && percentage < 99) {
      return 'Yellow';
    } else if (percentage >= 99) {
      return 'green';
    } else
    return '';
  }

  onTDCChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }) {
    if (this.slotmanagementFilter.tDCs == undefined || this.slotmanagementFilter.tDCs == null) {
      this.slotmanagementFilter.tDCs = "";
    }
    if (event.isUserInput) {
      if (event.source.selected === true) {
        this.slotmanagementFilter.tDCs = event.source.value;
        if(this.slotmanagementFilter.tDCs != "" && this.slotmanagementFilter.communityID != 0)
          {
            this.isEditDisable = false;
            this.getCommunitySelectionRatio();
            this.getSlotManagementDetails();
          }
      }
    }
    
  }

  onCommunityChanged(event: {
    isUserInput: any;
    source: { value: any; selected: any };
  }) {
    
    if (this.slotmanagementFilter.communityID == undefined || this.slotmanagementFilter.communityID == null) {
      this.slotmanagementFilter.communityID = 0;
    }
    if (event.isUserInput) {
      if (event.source.selected === true) {
        this.slotmanagementFilter.communityID = event.source.value;
        if (this.slotmanagementFilter.tDCs != "" && this.slotmanagementFilter.communityID != 0)
          this.isEditDisable = false;
          this.getSlotManagementDetails();
          this.getCommunitySelectionRatio();
        
      } 
    }
  }

  onDateChange(event: any) {
    if ((this.slotmanagementFilter.startDate == undefined || this.slotmanagementFilter.startDate == null)
      && (this.slotmanagementFilter.endDate == undefined || this.slotmanagementFilter.endDate == null)) {
      this.slotmanagementFilter.startDate = "";
      this.slotmanagementFilter.endDate = "";
    }
    if (event != undefined && event.controls['startDate'].value != undefined && event.controls['startDate'].value != null
      && event.controls['endDate'].value != undefined && event.controls['endDate'].value != null) {    
      this.slotmanagementFilter.startDate = (new Date(event.controls['startDate'].value)).toDateString();
      this.slotmanagementFilter.endDate = (new Date(event.controls['endDate'].value)).toDateString();     
      this.getSlotManagementDetails();
      this.getCommunitySelectionRatio();
    }
  }

  updateSlotManagement(dataSource: any) {
    this.slotManagementList = dataSource.data;
    let createSlotManagementList  = this.slotManagementList.filter(record => record.dropRatio !== undefined && record.positionToBeFilled !== undefined && record.offersToBeRolledOut !== undefined && record.id === undefined);
    this.editableGridLogic(null);  
    let updateSlotManagementList =  this.slotManagementList.filter(record => record.id !== undefined);
    if(updateSlotManagementList.length > 0)
      this.slotManagementService.UpdateSlotManagement(updateSlotManagementList).subscribe((response: any) => {
        this.getSlotManagementDetails();
        this.getCommunitySelectionRatio();
      });
    if(createSlotManagementList.length > 0)
      this.slotManagementService.CreateSlotManagement(createSlotManagementList).subscribe((response: any) => {
        this.getSlotManagementDetails();
        this.getCommunitySelectionRatio();
    });
      
  }

  getSeniorityName(seniorityId: number): string {
    if (seniorityId == undefined)
      return '';
    let senorityName: string | undefined
    senorityName = this.senorities.find(x => x.id == seniorityId)?.name;
    return senorityName !== undefined ? senorityName : '';
  }

  performCalculation(item: any) {
    // Find the index of the row to update
    const index = this.dataSource.data.findIndex((row: any) =>
      //row.id === item.id
       row.seniorityId === item.seniorityId
  );
    if (index !== -1) {
      if(item.offersToBeRolledOut != null && item.offersToBeRolledOut != undefined)
        {
          if(item.l1SelectionRatio != null &&  item.l1SelectionRatio != undefined)
            {
              item.l1SlotsRequired = (Math.round(item.offersToBeRolledOut * item.l1SelectionRatio)).toString();
            }
            
          if(item.gkSelectionRatio != null && item.gkSelectionRatio != undefined)
            {
                item.gkSlotsRequired = (Math.round(item.offersToBeRolledOut * item.gkSelectionRatio)).toString();
            }
        } 
        
      if(item.l1SlotsRequired != null && item.l1SlotsRequired != undefined && item.l1SlotsActual != null && item.l1SlotsActual != undefined)
      item.shortFallL1 = item.l1SlotsRequired - item.l1SlotsActual;
      
      if(item.gkSlotsRequired != null && item.gkSlotsRequired != undefined && item.gkSlotsActual != null && item.gkSlotsActual != undefined)
      item.shortFallGK = item.gkSlotsRequired - item.gkSlotsActual;
      
      item.startDate = this.dateStart;
      item.endDate = this.dateEnd;

      // Replace the old row with the updated row
      
      this.dataSource.data[index] = item;
      // Refresh the MatTableDataSource
      this.dataSource._updateChangeSubscription();
    }
  }

  editableGridLogic(item: any) {
    
    this.dataSource.data = this.dataSource.data.map(gridItem => ({
      ...gridItem,  isEditing: gridItem === item ? !gridItem.isEditing : false
    }));
  }

  formatDate(date: Date): string | null {
    return this.datePipe.transform(date, 'yyyy-MM-dd hh:mm:ss');
  }

  getCommunitySelectionRatio()
  {

    this.slotManagementService.getCommunitySelectionRatio(this.slotmanagementFilter).subscribe((response: any) => {
      if (response != null && response != undefined) {
        this.communitySelectionRatio = response;
      }
      
    });
  }

  getPredictedSelectionRatio()
  {
    this.slotManagementService.getPredictedSelectionRatio(this.slotmanagementFilter).subscribe((response: any) => {
      if (response != null && response != undefined) {
        this.communitySelectionRatio = response;
      }
    });
  }

  updateSelectionRatio()
  {
    if(this.isRatioEdit)
      {
        this.editRatioButtonName = "Update";
      }
      else
      {
        this.communitySelectionRatio = {
          tdc: this.slotmanagementFilter.tDCs,
          communityId: this.slotmanagementFilter.communityID,
          startDate: new Date(this.slotmanagementFilter.startDate),
          endDate: new Date(this.slotmanagementFilter.endDate), 
          l1SelectionRatio: this.communitySelectionRatio.l1SelectionRatio,
          gkSelectionRatio: this.communitySelectionRatio.gkSelectionRatio
        };
        this.slotManagementService.UpdateCommunitySelectionRatio(this.communitySelectionRatio).subscribe((response: any) => {
            
        });
        this.editRatioButtonName = "Edit";
      }
      this.isRatioEdit = !this.isRatioEdit;

  }

}
