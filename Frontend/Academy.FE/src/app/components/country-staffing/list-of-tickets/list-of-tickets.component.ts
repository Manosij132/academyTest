import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { ReactiveFormsModule, FormsModule, FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { TicketEditDialogComponent } from '@shared/component/ticket-edit-dialog/ticket-edit-dialog.component';
import { CommonModule } from '@angular/common';
import { TicketsService } from '@services/tickets.service';
import { LoaderService } from "@services/loader.service";
import { formatDate } from '@angular/common';
import { MatSort } from '@angular/material/sort';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-list-of-tickets',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatDatepickerModule,
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        CommonModule,
        MatInputModule,
        MatIconModule
    ],
    templateUrl: './list-of-tickets.component.html',
    styleUrls: ['./list-of-tickets.component.scss']
})
export class ListOfTicketsComponent implements OnInit {

    displayedColumns: string[] = [
        'index', 'requestID', 'detailedStatus', 'ticketStatus',
        'monthClosure', 'aging', 'positionID', 'handler', 'handler2',
        'stage', 'startDate', 'recordCount'
    ];

    dataSource = new MatTableDataSource<any>();
    pageSize = 50;
    currentPage = 1;
    totalRecords = 0;
    startDate: string | null = null;
    endDate: string | null = null;
    dateField = "SubmitDate";
    searchText: string | null = null;
    searchControl = new FormControl('');
    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;


    constructor(private ticketsService: TicketsService, private dialog: MatDialog, private toastr: ToastrService, private loaderService: LoaderService) { }

    ngOnInit(): void {
        //this.setupSearchFilter();
        this.setupSearch();
        this.loadTickets();
    }

    loadTickets() {
        this.loaderService.start();
        
        this.ticketsService.getTickets({
            dateField: this.dateField,
            startDate: this.startDate ?? undefined,
            endDate: this.endDate ?? undefined,
            searchText: this.searchText ?? "",
            pageNumber: this.currentPage,
            pageSize: this.pageSize
        })
            .pipe(finalize(() => this.loaderService.stop()))
            .subscribe({
                next: response => {
                    this.dataSource.data = response?.data ?? [];
                    this.totalRecords = response?.totalRecords ?? 0;
                    this.dataSource.filter = this.searchText?.trim().toLowerCase() || '';

                    console.log(
                        'Rows:', this.dataSource.data.length,
                        'Total:', this.totalRecords
                    );

                    setTimeout(() => {
                        //this.dataSource.paginator = this.paginator;
                        this.dataSource.sort = this.sort;
                    });
                },
                error: err => console.error('Failed to load tickets', err)
            });
    }

    setupSearch() {
        this.searchControl.valueChanges
        .pipe(
            debounceTime(400),
            distinctUntilChanged(),
            switchMap(searchText => {
                this.loaderService.start(); 

                this.searchText = searchText;

                return this.ticketsService.getTickets({
                    dateField: this.dateField,
                    startDate: this.startDate ?? undefined,
                    endDate: this.endDate ?? undefined,
                    searchText: searchText ?? "",
                    pageNumber: this.currentPage,
                    pageSize: this.pageSize
                }).pipe(
                    finalize(() => this.loaderService.stop()) 
                );
            })
        )
        .subscribe({
            next: response => {
                this.dataSource.data = response?.data ?? [];
                this.totalRecords = response?.totalRecords ?? 0;
                this.dataSource.filter = (this.searchText || '').trim().toLowerCase();

                setTimeout(() => {
                    this.dataSource.sort = this.sort;
                });
            },
            error: err => console.error('Failed to load tickets', err)
        });
}


    // setupSearchFilter() {
    //     this.dataSource.filterPredicate = (data: any, filter: string): boolean => {

    //         if (!filter || filter.length < 3) {
    //             return true;
    //         }

    //         const searchValue = filter.trim().toLowerCase();

    //         const searchableFields = [
    //             'requestID',
    //             'detailedStatus',
    //             'ticketStatus',
    //             'monthClosure',
    //             'positionID',
    //             'handler',
    //             'handler2',
    //             'stage'
    //         ];

    //         return searchableFields.some(field => {
    //             const value = data[field];
    //             return (
    //                 typeof value === 'string' &&
    //                 value.toLowerCase().includes(searchValue)
    //             );
    //         });
    //     };
    // }

    // applySearch() {
    //     this.setupSearchFilter();
    //     this.dataSource.filter = this.searchText?.trim().toLowerCase() || '';
    //     this.loadTickets();
    // }


    onDateRangeSelected() {
        const startEl = document.querySelector('input[matStartDate]') as HTMLInputElement;
        const endEl = document.querySelector('input[matEndDate]') as HTMLInputElement;

        if (startEl?.value && endEl?.value) {
            this.startDate = formatDate(startEl.value, 'yyyy-MM-dd', 'en-US');
            this.endDate = formatDate(endEl.value, 'yyyy-MM-dd', 'en-US');

            this.loadTickets();
        }
    }

    onPageChange(event: any) {
        this.pageSize = event.pageSize;
        this.currentPage = event.pageIndex + 1;
        this.loadTickets();
    }

    openEditDialog(row: any) {
        const dialogRef = this.dialog.open(TicketEditDialogComponent, {
            width: '450px',
            data: row
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.ticketsService.updateEditableTicketFields(row.requestID, result).subscribe(() => {
                    this.loadTickets();
                    this.toastr.success('Ticket updated successfully!');
                }, error => {
                    this.toastr.error('Update failed. Please try again.');
                });
            }
        });
    }
}
