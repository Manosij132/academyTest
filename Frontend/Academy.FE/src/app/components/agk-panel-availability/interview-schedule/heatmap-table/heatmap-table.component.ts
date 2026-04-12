import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { HeatmapTableColumn, HeatmapTableData, PanelistGridModel, SlotType} from './heatmap-table.model';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ScheduleInterviewPopupComponent } from '../schedule-interview-popup/schedule-interview-popup.component';
import { MatDialog } from '@angular/material/dialog';
import { PanelService } from '../../../../services/panel.service';
import { PanelSlotDataModel } from '../../model/panel-slot-data.model';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Overlay } from "@angular/cdk/overlay";

@Component({
  selector: 'app-heatmap-table',
  templateUrl: './heatmap-table.component.html',
  styleUrls: ['./heatmap-table.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports:[
    CommonModule, 
    MatTableModule,
    MatTooltipModule
  ]
})
export class HeatmapTableComponent
  implements OnInit, OnChanges, OnDestroy, AfterViewInit
{
  @Input() panelists!: PanelistGridModel[];
  @Input() slotDateRange: Date[] = [];
  @Input() reloadSubject!: BehaviorSubject<number>;
  @ViewChild(MatSort) sort!: MatSort;

  columns: HeatmapTableColumn[] = [];
  columnsToDisplay: string[] = [];
  initalDataSource: HeatmapTableData[] = [];
  updatedPanelData!: PanelSlotDataModel;
  isEditing: boolean = false;
  dataSource = new MatTableDataSource<HeatmapTableData>([]);

  searchControl = new FormControl<string>('', { nonNullable: true });
  searchControlSubscription?: Subscription;

  unavailableSlotType: SlotType = 'Unavailable';
  slotClasses: Map<SlotType, string> = new Map<SlotType, string>([
    ['Utilised', 'slot slot-red'],
    ['Unutilised', 'slot slot-green'],
    ['Unavailable', 'slot slot-grey'],
  ]);

  constructor(
    private panelService:PanelService, 
    public dialog: MatDialog,
    private overlay: Overlay) {}

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnInit(): void {
    this.initializeTable();

    this.searchControlSubscription = this.searchControl.valueChanges
      .pipe(debounceTime(300))
      .subscribe((searchValue) => {
        this.dataSource.data = HeatmapTableComponent.filterDataSrouce(
          searchValue,
          this.initalDataSource
        );
      });
  }

  ngOnChanges(_: SimpleChanges): void {
    this.initializeTable();
  }

  ngOnDestroy(): void {
    this.searchControlSubscription?.unsubscribe();
  }

  static createColumns(dateRange: Date[]): HeatmapTableColumn[] {
    const dayNames = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];

    return dateRange.map((date) => ({
      key: date.toLocaleDateString(),
      day: dayNames[date.getDay()],
      date: date.getDate(),
    }));
  }

  static createDateSource(
    dateRange: Date[],
    panelists: PanelistGridModel[]
  ): HeatmapTableData[] {
    return panelists.map(({ slots, ...panelist }) => {
      const element: any = { ...panelist };

      for (let date of dateRange) {
        element[date.toLocaleDateString()] = slots[date.toLocaleDateString()];
      }

      return element;
    });
  }

  static getSlotDateRange(startDate: Date, endDate: Date): Date[] {
    const slotDateRange: Date[] = [];
    const currentDate = new Date(startDate);

    while (currentDate <= endDate) {
      const date = new Date(currentDate);
      slotDateRange.push(date);
      currentDate.setDate(currentDate.getDate() + 1);
    }

    return slotDateRange;
  }

  static filterDataSrouce(searchValue: string, dataSource: HeatmapTableData[]) {
    const normalizedSearchString = searchValue.toLowerCase();

    return dataSource.filter(
      (x) =>
        searchValue.length == 0 ||
        x.primaryPanel.toLowerCase().includes(normalizedSearchString) ||
        x.emailId.toLowerCase().includes(normalizedSearchString)
    );
  }

  private initializeTable() {
    this.columns = HeatmapTableComponent.createColumns(this.slotDateRange);
    this.columnsToDisplay = ['panel', 'primaryPanel', 'upToSeniority'];
    this.columnsToDisplay.push(...this.columns.map((c) => c.key));

    this.dataSource.sort = this.sort;

    this.initalDataSource = HeatmapTableComponent.createDateSource(
      this.slotDateRange,
      this.panelists
    );

    this.dataSource.data = HeatmapTableComponent.filterDataSrouce(
      this.searchControl.value,
      this.initalDataSource
    );
  }
  
  sceduleInterviewDialog(panelData: any, slotType: SlotType , id : number, date: string): void {
    const time = panelData[date].time;
    
    if (slotType == this.unavailableSlotType) {
      return;
    }
    
    if (slotType == 'Utilised') {
     this.isEditing = true;
    } else {
      this.isEditing = false;
    }

    this.panelService.getPanelSlotDataById(id).subscribe((res) => {
        this.updatedPanelData = res;
        const dialogRef = this.dialog.open(ScheduleInterviewPopupComponent, {
          data: id,
          disableClose: true
        });
        dialogRef.componentInstance.updatedPanelData = this.updatedPanelData;
        dialogRef.componentInstance.panelData = panelData;
        dialogRef.componentInstance.date = date;
        dialogRef.componentInstance.time = time;
        dialogRef.componentInstance.isEditing = this.isEditing;
        
        dialogRef.afterClosed().subscribe((result) => {
          if(result ==="true") {
           this.reloadSubject.next(Math.random());
           }
        });
      })
  }
}
