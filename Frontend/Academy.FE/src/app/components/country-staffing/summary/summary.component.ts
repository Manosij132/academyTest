import { Component, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, MatOptionModule } from '@angular/material/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { SummaryService } from "@services/summary.services";
import { PivotRow } from "@shared/Interface/summary.model";
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox'
import { MatDialog } from '@angular/material/dialog';
import { TicketEditDialogComponent } from '@shared/component/ticket-edit-dialog/ticket-edit-dialog.component';
import { TicketsService } from '@services/tickets.service';
import { ToastrService } from 'ngx-toastr';
import { LoaderService } from "@services/loader.service";
import { finalize } from 'rxjs';
import { MatSelect } from '@angular/material/select';

@Component({
    selector: 'app-summary-by-status',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatFormFieldModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatTooltipModule,
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatOptionModule,
        MatIconModule,
        MatCheckboxModule
    ],
    templateUrl: './summary.component.html',
    styleUrls: ['./summary.component.css'],
})

export class SummaryByStatusComponent implements OnInit {

    aiStudioGroups: any[] = [];
    filteredStudioGroups: any[] = [];

    clients: any[] = [];
    filteredClients: any[] = [];

    detailedStatuses: any[] = [];
    filteredStatuses: any[] = [];

    selectedStudioGroups: string[] = [];
    selectedClients: string[] = [];
    selectedStatuses: string[] = [];

    isDependentDisabled = false;
    showNoTicketData = false;

    startDate: Date | null = null;
    endDate: Date | null = null;

    summaryData: PivotRow[] = [];
    columns: string[] = [];
    grandTotal: { [key: string]: number } = {};

    displayedColumns = [
        'index',
        'detailedStatus',
        'requestID',
        'monthClosure',
        'client',
        'ticketStatus',
        'comments'
    ];

    dataSource = new MatTableDataSource<any>([]);
    pageSize = 25;
    currentPage = 1;
    totalCount = 0;
    tableData: any[] = [];
    expandAll = false;
    selectedStatus: string | null = null;
    statusSearch = '';
    allRows: PivotRow[] = [];     // original data
    filteredRows: PivotRow[] = []; // used by UI
    selectedTicketStatus: string | null = null;
    selectedMonthClosure: string | null = null;
    selectedClient: string | null = null;

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild('clientSelect') clientSelect!: MatSelect;
    @ViewChild('studioGroupSelect') studioGroupSelect!: MatSelect;
    @ViewChild('statusSelect') statusSelect!: MatSelect;
    selectedCell: {
        ticketStatus: string | null;
        column: string | null;
        client?: string | null;
    } | null = null;

    /**
     *
     */
    constructor(private summaryService: SummaryService, private dialog: MatDialog, private ticketsService: TicketsService,
        private toastr: ToastrService, private loaderService: LoaderService) { }

    ngOnInit(): void {
        this.loadDropdownData();
    }

    ngAfterViewInit() {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
    }

    loadDropdownData() {
        this.loaderService.start();
        this.summaryService.getDropdownData(this.startDate, this.endDate).pipe(finalize(() => {
            this.loaderService.stop();
        }))
            .subscribe({
                next: (res) => {
                    console.log("API Response:", res);

                    this.aiStudioGroups = this.filteredStudioGroups = res.aiStudioGroups;
                    this.clients = this.filteredClients = res.clients;
                    this.detailedStatuses = this.filteredStatuses = res.detailedStatuses;

                    this.selectedStudioGroups = this.aiStudioGroups.map(g => g.groupName);
                    this.selectedClients = this.clients.map(c => c.client);
                    this.selectedStatuses = this.detailedStatuses.map(s => s.statusName);

                    this.applyFilters();
                    this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [], [], null, null);
                },
                error: (err) => {
                    console.error("Dropdown API failed:", err);
                }
            });
    }

    applyFilters() {
        // if (!this.selectedStudioGroups?.length && !this.selectedClients?.length || !this.selectedStatuses?.length) {
        //     // CLEAR SUMMARY GRID
        //     this.summaryData = [];
        //     this.filteredRows = [];
        //     this.allRows = [];
        //     this.columns = [];
        //     this.grandTotal = {};
        //     return;
        // }
        if (
            !this.selectedStudioGroups?.length ||
            !this.selectedClients?.length ||
            !this.selectedStatuses?.length
        ) {
            this.summaryData = [];
            this.filteredRows = [];
            this.grandTotal = {};
            return;
        }
        this.loaderService.start();
        this.summaryService.getFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, this.startDate, this.endDate).pipe(finalize(() => {
            this.loaderService.stop();
        }))
            .subscribe({
                next: res => {
                    const data = res.summaryData;
                    console.log(data);

                    this.columns = this.getColumns(data);
                    this.summaryData = this.transformToPivot(data);
                    this.grandTotal = this.calculateGrandTotal(this.summaryData);
                    this.allRows = [...this.summaryData];
                    this.filteredRows = [...this.summaryData];
                },
                error: err => {
                    console.error('Failed to apply filters', err);
                }
            });
    }


    getTicketFilteredData(groupNames?: string[] | [], client?: string[] | [], detailedStatuses?: string[] | [], ticketStatus?: string[] | [], monthClosure?: string[] | [], startDate?: Date | null, endDate?: Date | null) {
        this.loaderService.start();
        this.summaryService.getTicketData(groupNames, client, detailedStatuses, ticketStatus, monthClosure, startDate, endDate, this.currentPage, this.pageSize).pipe(finalize(() => {
            this.loaderService.stop();
        }))
            .subscribe({
                next: response => {
                    console.log("Ticket API Response:", response);

                    this.dataSource = new MatTableDataSource(response.data || []);
                    this.totalCount = response.totalRecords;

                    this.dataSource.paginator = this.paginator;
                    this.dataSource.sort = this.sort;
                },
                error: err => {
                    console.error('Error loading ticket details', err);
                }
            });
    }

    calculateGrandTotal(rows: any[]) {
        const grand: Record<string, number> = {};

        rows.forEach(row => {
            Object.entries(row.totals).forEach(([key, value]: any) => {
                grand[key] = (grand[key] || 0) + value;
            });
        });

        return grand;
    }


    transformToPivot(data: any[]): PivotRow[] {
        const map = new Map<string, any>();

        data.forEach(item => {
            const status = item.ticketStatus || 'null';

            if (!map.has(status)) {
                map.set(status, {
                    ticketStatus: status,
                    totals: {},
                    children: [],
                    expanded: false
                });
            }

            const parent = map.get(status);

            // Add child
            parent.children.push({
                client: item.client,
                totals: item.monthCounts
            });

            // Aggregate parent totals
            Object.entries(item.monthCounts).forEach(([key, value]: any) => {
                if (key === 'Grand total') return;

                parent.totals[key] =
                    (parent.totals[key] || 0) + value;
            });
        });

        return Array.from(map.values());
    }

    getColumns(data: any[]): string[] {
        const set = new Set<string>();

        data.forEach(d => {
            Object.keys(d.monthCounts).forEach(k => {
                if (k !== 'Grand total') {
                    // normalize null / empty / 'null'
                    const columnName =
                        k === null || k === undefined || k === '' || k === 'null'
                            ? 'Blank'
                            : k;

                    set.add(columnName);
                }
            });
        });

        return Array.from(set);
    }


    filterStudioGroups(value: string) {
        const val = value.toLowerCase();
        this.filteredStudioGroups = this.aiStudioGroups.filter(x =>
            x.groupName.toLowerCase().includes(val)
        );
    }

    filterClients(value: string) {
        const val = value.toLowerCase();
        this.filteredClients = this.clients.filter(x =>
            x.client.toLowerCase().includes(val)
        );
    }

    filterStatuses(value: string) {
        const val = value.toLowerCase();
        this.filteredStatuses = this.detailedStatuses.filter(x =>
            x.statusName.toLowerCase().includes(val)
        );
    }

    toggleAll() {
        this.expandAll = !this.expandAll;
    }


    get expandedAny() {
        // return this.summaryData?.some(r => r.expanded) ?? false;
        return this.expandAll;
    }

    rowTotal(row: any) {
        if (!row?.totals) return 0;
        return Object.values(row.totals).reduce((a: any, b: any) => a + b, 0);
    }

    get overallTotal() {
        if (!this.grandTotal) return 0;
        return Object.values(this.grandTotal).reduce((a: any, b: any) => a + b, 0);
    }

    onCellClick(row: any, column: string, event: MouseEvent) {
        event.stopPropagation(); // prevents row click conflicts
        this.selectedTicketStatus = row.ticketStatus;
        this.selectedMonthClosure = column;
        this.selectedCell = {
            ticketStatus: row.ticketStatus,
            column
        };

        this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [row.ticketStatus], [column], this.startDate, this.endDate);
        //this.onPageChange(event, [row.ticketStatus], [column])
    }

    onChildCellClick(
        row: any,
        client: string,
        column: string | null,
        event: MouseEvent
    ) {
        event.stopPropagation();
        this.selectedClient = client;
        this.selectedCell = {
            ticketStatus: row.ticketStatus,
            client,
            column
        };

        this.getTicketFilteredData(this.selectedStudioGroups, [client], this.selectedStatuses, [row.ticketStatus], column ? [column] : [], this.startDate, this.endDate);
        //this.onPageChange(event, [row.ticketStatus], column ? [column] : [])
    }


    onPageChange(event: any) {
        this.pageSize = event.pageSize;
        this.currentPage = event.pageIndex + 1;
        const ticketStatuses: string[] = this.selectedTicketStatus ? [this.selectedTicketStatus] : [];
        const monthClosures: string[] = this.selectedMonthClosure ? [this.selectedMonthClosure] : [];
        const clients: string[] = this.selectedClient ? [this.selectedClient] : [];
        if (clients && clients.length > 0) {
            this.getTicketFilteredData(this.selectedStudioGroups, clients, this.selectedStatuses, ticketStatuses, monthClosures, null, null,);
        }
        else {
            this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, ticketStatuses, monthClosures, null, null,);
        }
    }

    openEditDialog(row: any) {
        const ticketStatuses: string[] = this.selectedTicketStatus ? [this.selectedTicketStatus] : [];
        const monthClosures: string[] = this.selectedMonthClosure ? [this.selectedMonthClosure] : [];
        const dialogRef = this.dialog.open(TicketEditDialogComponent, {
            width: '450px',
            data: row
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.ticketsService.updateEditableTicketFields(row.requestID, result).subscribe(() => {
                    this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, ticketStatuses, monthClosures, this.startDate, this.endDate);
                    this.toastr.success('Ticket updated successfully!');
                }, error => {
                    this.toastr.error('Update failed. Please try again.');
                });
            }
        });
    }

    onStatusSearch(event: Event): void {
        const value = (event.target as HTMLInputElement).value
            .toLowerCase()
            .trim();

        if (!value) {
            this.filteredRows = [...this.allRows];
            return;
        }

        this.filteredRows = this.allRows.filter(row =>
            (row.ticketStatus ?? '').toLowerCase().includes(value)
        );
    }

    clearSearch() {
        this.statusSearch = '';
        this.filteredRows = [...this.allRows];
    }

    onDateRangeSelected() {
        if (!this.startDate && !this.endDate) {
            return;
        }
        this.applyFilters();
        this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [], [], this.startDate, this.endDate);
    }

    toggleSelectAll(
        filteredList: any[],
        selectedList: string[],
        valueKey: string
    ): string[] {

        const allValues = filteredList.map(item => item[valueKey]);

        const isAllSelected =
            allValues.length > 0 &&
            allValues.every(v => selectedList.includes(v));

        return isAllSelected ? [] : allValues;
    }

    toggleStudioGroups() {
        const allGroups = this.filteredStudioGroups.map(g => g.groupName);
        const isAllSelected = allGroups.length && allGroups.every(v => this.selectedStudioGroups.includes(v));
        if (isAllSelected) {
            //User UNSELECTS Select All
            // this.selectedStudioGroups,
            this.selectedStudioGroups = [];
            this.selectedClients = [];
            this.selectedStatuses = [];
            // 'groupName'
            this.clearAndDisableDependents();
            this.studioGroupSelect.close(); 
            return;
        }
        this.selectedStudioGroups = [...allGroups];

        this.isDependentDisabled = false;
        this.studioGroupSelect.close(); 
        this.onStudioGroupChange();
    }

    toggleClients() {
        this.selectedClients = this.toggleSelectAll(
            this.filteredClients,
            this.selectedClients,
            'client'
        );
    }

    toggleDetailedStatuses() {
        this.selectedStatuses = this.toggleSelectAll(
            this.filteredStatuses,
            this.selectedStatuses,
            'statusName'
        );
    }

    onStudioGroupChange() {
        // if (!this.selectedStudioGroups.length) {
        //     this.clearAndDisableDependents();
        //     return;
        // }
        if (!this.selectedStudioGroups || this.selectedStudioGroups.length === 0) {

            // Disable dependent dropdowns
            this.selectedClients = [];
            this.selectedStatuses = [];

            this.filteredClients = [];
            this.filteredStatuses = [];

            this.isDependentDisabled = true;

            setTimeout(() => {
                this.clientSelect?.options.forEach(option => option.deselect());
                this.statusSelect?.options.forEach(option => option.deselect());
            });

            // Clear tables
            this.resetTables();

            // Clear ticket detail table
            // this.dataSource.data = [];
            // this.totalCount = 0;

            return;  // IMPORTANT: STOP HERE
        }
        this.isDependentDisabled = false;
        this.loaderService.start();

        this.summaryService
            .getClientAndDetailedStatusByAIGroup(this.selectedStudioGroups, this.startDate, this.endDate)
            .subscribe({
                next: res => {
                    this.filteredClients = res.clients;
                    this.filteredStatuses = res.detailedStatuses;

                    // Auto-select all by default
                    this.selectedClients = res.clients.map(c => c.client);
                    this.selectedStatuses = res.detailedStatuses.map(s => s.statusName);

                    // Load pivot table
                    this.applyFilters();
                    this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [], [], null, null)
                    this.studioGroupSelect.close();
                },
                error: err => {
                    console.error('Failed to load dependent dropdowns', err);
                },
                complete: () => {
                    this.loaderService.stop();
                }
            });
    }
    
    clearAndDisableDependents() {
        this.isDependentDisabled = true;
        this.selectedClients = [];
        this.selectedStatuses = [];
        this.filteredClients = [];
        this.filteredStatuses = [];
        this.resetTables();
    }
    resetTables() {
        this.summaryData = [];
        this.filteredRows = [];
        this.allRows = [];
        this.columns = [];
        this.grandTotal = {};
        this.dataSource.data = [];
        this.totalCount = 0;
        this.showNoTicketData = true;
    }



    onClientChange() {
        // Guard – if nothing selected, clear status dropdown
        // if (!this.selectedStudioGroups.length || !this.selectedClients.length) {
        //     this.filteredStatuses = [];
        //     this.selectedStatuses = [];
        //     this.applyFilters(); // still allowed
        //     return;
        // }
        if (!this.selectedClients || this.selectedClients.length === 0 ||
            !this.selectedStudioGroups || this.selectedStudioGroups.length === 0) {

            this.filteredStatuses = [];
            this.selectedStatuses = [];

            // Clear pivot table
            this.filteredRows = [];
            this.summaryData = [];
            this.grandTotal = {};

            return;
        }

        this.loaderService.start();

        this.summaryService
            .getDetailedStatusByAIGroupAndClient(
                this.selectedStudioGroups,
                this.selectedClients,
                this.startDate,
                this.endDate
            )
            .subscribe({
                next: res => {
                    this.filteredStatuses = res.detailedStatuses;

                    // Auto-select all detailed statuses
                    this.selectedStatuses = res.detailedStatuses.map(s => s.statusName);

                    // Load pivot table
                    this.applyFilters();
                    this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [], [], null, null)
                    this.clientSelect.close();
                },
                
                error: err => {
                    console.error('Failed to load detailed statuses', err);
                },
                complete: () => {
                    this.loaderService.stop();
                }
            });
    }


    onStatusChange() {
        if (!this.selectedStatuses || this.selectedStatuses.length === 0) {

            // Clear pivot table only (ticket table clears when clicked)
            this.filteredRows = [];
            this.summaryData = [];
            this.grandTotal = {};

            return;
        }
        this.applyFilters();
        this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [], [], null, null)
        this.statusSelect.close();
    }

    onStatusClick(row: any, event: MouseEvent) {
        event.stopPropagation();

        this.selectedTicketStatus = row.ticketStatus;

        // Reset selected cell highlight
        this.selectedCell = null;

        // Call ticket details API
        this.getTicketFilteredData(this.selectedStudioGroups, this.selectedClients, this.selectedStatuses, [row.ticketStatus], [], this.startDate, this.endDate);
        //this.onPageChange(event, [row.ticketStatus], [])
    }

}