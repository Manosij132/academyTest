import { Component, inject, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { MatDatepickerInputEvent, MatDatepickerModule } from '@angular/material/datepicker';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ChangeDetectorRef } from '@angular/core';
import { PanelEfficiencyGraphDialogComponent } from './panel-efficiency-graph-dialog/panel-efficiency-graph-dialog.component';
import * as FileSaver from 'file-saver';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import 'jspdf-autotable';
import { PanelEfficiencyEvaluationComponent } from './panel-efficiency-evaluation/panel-efficiency-evaluation.component';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { MatMenuModule } from '@angular/material/menu';
import { PanelService } from '@services/panel.service';
import { PanelEfficiencyService } from '@services/panel-efficiency.service';
import { ActivatedRoute } from '@angular/router';

interface PanelEfficiency {
  panelName: string;
  panelType: string;
  seniority: string;
  l1Conducted: number;
  l1Selected: number;
  gkConducted: number;
  gkSelected: number;
  efficiency: number;
  countwiseEfficiency: number;
  tdc: string;
  community: string;
}

interface TDC {
  tdcId: number;
  tdcName: string;
}

interface Community {
  communityId: number;
  communityName: string;
}

@Component({
  selector: 'mf-app-panel-efficiency-report',
  templateUrl: './panel-efficiency-report.component.html',
  styleUrls: ['./panel-efficiency-report.component.css'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatCardModule,
    MatDialogModule,
    MatButtonModule,
    NgxMatSelectSearchModule,
    MatMenuModule,
    MatPaginatorModule
  ]
})

export class PanelEfficiencyReportComponent implements OnInit {
  panelData: PanelEfficiency[] = [];
  filteredData: PanelEfficiency[] = [];
  paginatedPanelData: PanelEfficiency[] = [];
  uniquePanelNames: string[] = [];
  uniquePanelTypes: string[] = [];
  selectedPanelNames: string[] = [];
  selectedPanelType: string = '';
  selectedTDC: string[] = [];
  selectedCommunity: string[] = [];

  panelNameControl = new FormControl();
  filteredPanelNames: Observable<string[]>;
  tdcControl = new FormControl();
  filteredTDCs: Observable<TDC[]>;
  communityControl = new FormControl();
  filteredCommunities: Observable<Community[]>;
  isSideNavActive: boolean = false;

  uniqueTDCs: TDC[] = [];
  uniqueCommunities: Community[] = []; 

  range = new FormGroup({
    startDate: new FormControl(),
    endDate: new FormControl(),
  });

  totalL1Conducted: number = 0;
  totalL1Selected: number = 0;
  totalGKConducted: number = 0;
  totalGKSelected: number = 0;

  // Pagination properties
  pageNumber: number = 1;
  pageSize: number = 20;
  totalItems: number = 0;
  totalPages: number = 0;

  // Date filter properties
  startDate: Date | null = null;
  endDate: Date | null = null;

  EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
  EXCEL_EXTENSION = '.xlsx';

  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private http: HttpClient, 
    public dialog: MatDialog, 
    private cdr: ChangeDetectorRef,
    private panelService: PanelService,
    private panelEfficiencyService: PanelEfficiencyService
  ) {
    this.filteredPanelNames = this._createFilterObservable(this.panelNameControl, this._filter.bind(this));
    this.filteredTDCs = this._createFilterObservable(this.tdcControl, this._filterTDCs.bind(this));
    this.filteredCommunities = this._createFilterObservable(this.communityControl, this._filterCommunities.bind(this));
  }

    private readonly _route = inject(ActivatedRoute);
    protected pageHeader = this._route.snapshot.data["pageHeader"];

  private _createFilterObservable<T>(control: FormControl, filterFn: (value: any) => T): Observable<T> {
    return control.valueChanges.pipe(
      startWith(''),
      map(value => filterFn(value))
    );
  }

  ngOnInit() {
    const currentDate = new Date();
    const startOfYear = new Date(currentDate.getFullYear(), 0, 1);
    
    this.range.setValue({
      startDate: startOfYear,
      endDate: currentDate,
    });
    
    this.startDate = startOfYear;
    this.endDate = currentDate;

    this.fetchTDCData();
    this.fetchCommunityData();
    // Fetch panel efficiency data and initialize unique values
    this.fetchPanelEfficiency().then(() => {
      // Filter observables are already initialized in constructor
    });
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

  fetchTDCData() {
    this.panelService.getTDCData().subscribe({
      next: (res) => {
        this.uniqueTDCs = res.sort((a, b) =>
          a.tdcName.localeCompare(b.tdcName)
        );
      }
    });

    this.filteredTDCs = this.tdcControl.valueChanges.pipe(
      startWith(''),
      map(value => this._filterTDCs(value))
    );
  }

  fetchCommunityData() {
    this.panelService.getCommunityData().subscribe((res) => {
      if (res !== undefined && res !== null) {
        this.uniqueCommunities = res.sort((a, b) => a.communityName.localeCompare(b.communityName));
        this.filteredCommunities = this.communityControl.valueChanges.pipe(
          startWith(''),
          map(value => this._filterCommunities(value))
        );
      }
    })
  }

  async fetchPanelEfficiency(): Promise<void> {

    let startDateStr: string | null = null;
    let endDateStr: string | null = null;

    if (this.startDate && this.endDate == null) {
      startDateStr = this.formatDate(this.startDate);
    } 
    else if (this.startDate && this.endDate) {
      startDateStr = this.formatDate(this.startDate);
      endDateStr = this.formatDate(this.endDate);
    }

      const data = {
        startDate: startDateStr,
        endDate: endDateStr
      };

    const res = await this.panelEfficiencyService.GetPanelEfficiencyDetails(data); 
    
      if (res !== undefined && res !== null) {
        this.panelData = res.items;
        this.filteredData = [...this.panelData];
        this.totalItems = res.totalCount;
        this.totalPages = Math.ceil(this.totalItems / this.pageSize);
        this.extractUniqueValues();
        this.calculateTotals(this.filteredData);
        this.paginateData(this.filteredData);
      }
  }

  formatDate(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${month}/${day}/${year}`;
  }

  extractUniqueValues() {
    this.uniquePanelNames = [...new Set(this.panelData.map(item => item.panelName))].sort();
    this.uniquePanelTypes = [...new Set(this.panelData.map(item => item.panelType))];
  }

  calculateTotals(data: PanelEfficiency[]) {
    this.totalL1Conducted = data.reduce((sum, item) => sum + item.l1Conducted, 0);
    this.totalL1Selected = data.reduce((sum, item) => sum + item.l1Selected, 0);
    this.totalGKConducted = data.reduce((sum, item) => sum + item.gkConducted, 0);
    this.totalGKSelected = data.reduce((sum, item) => sum + item.gkSelected, 0);
  }

  applyFilters(fetchFromApi: boolean = false) {
    if (fetchFromApi) {
      this.fetchPanelEfficiency().then(() => {
        // Reset filters when fetching new data
        this.selectedPanelNames = [];
        this.selectedPanelType = '';
        this.selectedTDC = [];
        this.selectedCommunity = [];
        
        this.filteredData = this.panelData;
        this.calculateTotals(this.filteredData);
        this.updatePagination();
      });
    } else {
      this.filteredData = this.panelData;
      
      if (this.selectedPanelNames.length > 0) {
        this.filteredData = this.filteredData.filter(item => this.selectedPanelNames.includes(item.panelName));
      }

      if (this.selectedPanelType) {
        this.filteredData = this.filteredData.filter(item => item.panelType === this.selectedPanelType);
      }

      if (this.selectedTDC.length > 0) {
        this.filteredData = this.filteredData.filter(item => this.selectedTDC.includes(item.tdc));
      }

      if (this.selectedCommunity.length > 0) {
        this.filteredData = this.filteredData.filter(item => this.selectedCommunity.includes(item.community));
      }

      this.calculateTotals(this.filteredData);
      this.updatePagination();
    }
  }

  updatePagination() {
    this.totalItems = this.filteredData.length;
    this.totalPages = Math.ceil(this.totalItems / this.pageSize);
    this.paginateData(this.filteredData);
  }

  sortData(sort: Sort) {
    const data = this.filteredData.slice();
    if (!sort.active || sort.direction === '') {
      this.paginatedPanelData = data.slice(0, this.pageSize);
      return;
    }

    this.paginatedPanelData = data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'panelName': return compare(a.panelName, b.panelName, isAsc);
        case 'panelType': return compare(a.panelType, b.panelType, isAsc);
        case 'panelSeniority': return compare(a.seniority, b.seniority, isAsc)
        case 'tdc' : return compare(a.tdc, b.tdc, isAsc);
        case 'community' : return compare(a.community, b.community, isAsc);
        case 'l1Conducted': return compare(a.l1Conducted, b.l1Conducted, isAsc);
        case 'l1Selected': return compare(a.l1Selected, b.l1Selected, isAsc);
        case 'gkConducted': return compare(a.gkConducted, b.gkConducted, isAsc);
        case 'gkSelected': return compare(a.gkSelected, b.gkSelected, isAsc);
        case 'efficiency': return compare(a.efficiency, b.efficiency, isAsc);
        case 'countwiseEfficiency': return compare(a.countwiseEfficiency, b.countwiseEfficiency, isAsc);
        default: return 0;
      }
    });

    this.paginateData(this.paginatedPanelData);
  }

  paginateData(data: PanelEfficiency[]) {
    const startIndex = (this.pageNumber - 1) * this.pageSize;
    this.paginatedPanelData = data.slice(startIndex, startIndex + this.pageSize);
  }

  getSerialNumber(index: number): number {
    return (this.pageNumber - 1) * this.pageSize + index + 1;
  }

  // Filter methods moved to constructor initialization using _createFilterObservable

  // Pagination methods
  nextPage() {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.paginateData(this.filteredData);
    }
  }

  previousPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.paginateData(this.filteredData);
    }
  }

  pageChanged(event: PageEvent) {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.paginateData(this.filteredData);
  }

  // Date filter methods
  onStartDateChange(event: MatDatepickerInputEvent<Date>) {
    this.startDate = event.value;

    if (!this.startDate) {
      this.endDate = null;
    }
  }

  onEndDateChange(event: MatDatepickerInputEvent<Date>) {
    this.endDate = event.value;

    if (this.endDate && !this.startDate) {
      alert('Please select a start date first.');
      this.endDate = null;
      return;
    }

    // Only apply filters if both start and end dates are set
    if (this.startDate && this.endDate) {
      this.applyFilters(true);
    }
  }

  toggleAllTDCSelection(isChecked: boolean) {
    if (isChecked) {
      this.selectAllTDC();
    } else {
      this.deselectAllTDC();
    }
  }

  toggleAllCommunitySelection(isChecked: boolean) {
    if (isChecked) {
      this.selectAllCommunity();
    } else {
      this.deselectAllCommunity();
    }
  }

  // Handle toggle all event
  toggleAllSelection(isChecked: boolean) {
    if (isChecked) {
      this.selectAll();
    } else {
      this.deselectAll();
    }
  }

  // Select All and Deselect All methods
  selectAll() {
    this.selectedPanelNames = [...this.uniquePanelNames];
    this.applyFilters();
  }

  deselectAll() {
    this.selectedPanelNames = [];
    this.applyFilters();
  }

  selectAllTDC() {
    this.selectedTDC = this.uniqueTDCs.map(tdc => tdc.tdcName);
    this.applyFilters();
  }

  deselectAllTDC() {
    this.selectedTDC = [];
    this.applyFilters();
  }

  selectAllCommunity() {
    this.selectedCommunity = this.uniqueCommunities.map(community => community.communityName);
    this.applyFilters();
  }

  deselectAllCommunity() {
    this.selectedCommunity = [];
    this.applyFilters();
  }

  onSelectionChange(event: any) {
    this.applyFilters();
  }

  onTDCSelectionChange(event: MatSelectChange) {
    const newSelection = event.value;
    this.selectedTDC = this.mergeSelections(this.selectedTDC, newSelection);
    this.applyFilters();
  }

  // Event handler for Community selection change
  onCommunitySelectionChange(event: MatSelectChange) {
    const newSelection = event.value;
    this.selectedCommunity = this.mergeSelections(this.selectedCommunity, newSelection);
    this.applyFilters();
  }

  // Helper method to merge new selections with existing ones
  private mergeSelections(existingSelections: string[], newSelection: string[]): string[] {
    const mergedSelections = new Set(existingSelections);
    newSelection.forEach(selection => mergedSelections.add(selection));
    return Array.from(mergedSelections);
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.uniquePanelNames.filter(name => name.toLowerCase().includes(filterValue));
  }

  private _filterTDCs(value: string): TDC[] {
    const filterValue = value.toLowerCase();
    return this.uniqueTDCs.filter(tdc => tdc.tdcName.toLowerCase().includes(filterValue));
  }

  private _filterCommunities(value: string): Community[] {
    const filterValue = value.toLowerCase();
    return this.uniqueCommunities.filter(community => community.communityName.toLowerCase().includes(filterValue));
  }
  openAnalyticsDialog(row: PanelEfficiency) {
    const graphData = [
      {
        name: 'Efficiency',
        value: row.efficiency
      },
      {
        name: 'Countwise Efficiency',
        value: row.countwiseEfficiency
      }
    ];
    
    this.panelService.PanelAIEvaluation(row.panelName).subscribe(res => {
      const dialogRef = this.dialog.open(PanelEfficiencyEvaluationComponent, {
          data: {
            panelEmail: row.panelName,
            panelName: res[0]?.panelName,
            Data: res
          }
        });

        dialogRef.afterOpened().subscribe(() => {
          this.cdr.detectChanges();
        });
    },
    error => {
      console.error('Error fetching TDC data:', error);
    });
  }

  openGraphDialog(row: PanelEfficiency) {
    const graphData = [
      {
        name: 'Efficiency',
        value: row.efficiency
      },
      {
        name: 'Countwise Efficiency',
        value: row.countwiseEfficiency
      }
    ];

    const dialogRef = this.dialog.open(PanelEfficiencyGraphDialogComponent, {
      data: {
        panelName: row.panelName,
        graphData: graphData
      }
    });
  
    dialogRef.afterOpened().subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  exportToExcel() {
    const data = this.filteredData.map((row, index) => ({
      'Sr. No.': this.getSerialNumber(index),
      'Panel Name': row.panelName,
      'Panel Type': row.panelType,
      'Panel Seniority': row.seniority,
      'TDC': row.tdc,
      'Community': row.community,
      'L1 Conducted': row.l1Conducted,
      'L1 Selected': row.l1Selected,
      'GK Conducted': row.gkConducted,
      'GK Selected': row.gkSelected,
      'Efficiency': row.efficiency,
      'Countwise Efficiency': row.countwiseEfficiency
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
    const workbook: XLSX.WorkBook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    this.saveAsExcelFile(excelBuffer, 'panel_efficiency');
  }

  private saveAsExcelFile(buffer: any, fileName: string): void {
    const data: Blob = new Blob([buffer], { type: this.EXCEL_TYPE });
    FileSaver.saveAs(data, fileName + '_export_' + new Date().getTime() + this.EXCEL_EXTENSION);
  }

  exportToPDF() {
    const doc = new jsPDF();
    const columns = ['Sr. No.', 'Panel Name', 'Panel Type', 'Panel Seniority', 'TDC', 'Community', 'L1 Conducted', 'L1 Selected', 'GK Conducted', 'GK Selected', 'Efficiency', 'Countwise Efficiency'];
    const rows = this.filteredData.map((row, index) => [
      this.getSerialNumber(index),
      row.panelName,
      row.panelType,
      row.seniority,
      row.tdc,
      row.community,
      row.l1Conducted,
      row.l1Selected,
      row.gkConducted,
      row.gkSelected,
      row.efficiency,
      row.countwiseEfficiency
    ]);

    (doc as any).autoTable(columns, rows);
    doc.save('panel_efficiency.pdf');
  }

}

function compare(a: number | string, b: number | string, isAsc: boolean) {
  return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
}