import { Component, EventEmitter, Input, OnInit, Output, } from '@angular/core';
import { BookmarkForms } from '@shared/dto/bookmark-form.dto';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';
import { AcademyHttpService } from '@services/academy-http.service';
import { LoaderService } from '@services/loader.service';
import { CommonModule } from '@angular/common';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-generate-report',
  standalone: true,
  imports: [CommonModule, MatPaginatorModule, // Import MatPaginatorModule here
    MatTableModule, // Import MatTableModule here
  ],
  templateUrl: './generate-report.component.html',
  styleUrl: './generate-report.component.css'
})
export class GenerateReportComponent implements OnInit {
  data: any[] = []; // Array to hold the API response data
  displayedColumns: string[] = []; // Columns to display in the table
  paginatedData: any[] = []; // Data for the current page
  pageSize: number = 10; // Number of items per page
  currentPage: number = 0; // Current page index
  exportUrl: string = ''; 

  @Input() reportDataForm!: BookmarkForms;
  @Output() updateDataSize = new EventEmitter<number>();
  @Output() exportUrlGenerated = new EventEmitter<string>();
  color = "#bfd732";

  constructor(private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService, private fb: FormBuilder, private route: ActivatedRoute) {
  }

  isNumeric(value: any): boolean {
    return !isNaN(parseFloat(value)) && isFinite(value);
  }

  ngOnInit(): void {
    this.fetchData(this.reportDataForm);
  }

  // Fetch report data
  fetchData(request: BookmarkForms): void {
    this.exportUrl = '';
    this.exportUrlGenerated.emit('');
    if (request.BookMarkId && request.BookMarkId > 0) {
      this.loaderService.start();
      this.academyHttpService
        .fetchBookmarkById(request.BookMarkId)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (bookmarkResponse: any) => {
            if (bookmarkResponse.status === 200) {
              const fullRequest = BookmarkForms.fromBookmarkDto(
                bookmarkResponse.data,
                request.BookMarkId
              );
              this.callViewReportApi(fullRequest);
            } else {
              this.toastr.error(bookmarkResponse.errorMessage, 'Fetch Bookmark Error');
            }
          },
        });
    } else {
      this.callViewReportApi(request);
    }
  }

  exportReport(request: BookmarkForms): void {
    if (request.BookMarkId && request.BookMarkId > 0) {
      this.loaderService.start();
      this.academyHttpService
        .fetchBookmarkById(request.BookMarkId)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (bookmarkResponse: any) => {
            if (bookmarkResponse.status === 200) {
              const fullRequest = BookmarkForms.fromBookmarkDto(
                bookmarkResponse.data,
                request.BookMarkId
              );
              this.callExportApi(fullRequest);
            } else {
              this.toastr.error(bookmarkResponse.errorMessage, 'Fetch Bookmark Error');
            }
          },
        });
    } else {
      this.callExportApi(request);
    }
  }

  private callViewReportApi(request: any): void {
    this.loaderService.start();
    this.academyHttpService
      .getReportData(request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.data = JSON.parse(response.data);
            this.displayedColumns = this.getColumns(this.data);
            this.updatePaginatedData();
            this.updateDataSize.emit(this.data.length);
          } else {
            this.toastr.error(response.errorMessage, 'Report data get Error');
          }
        },
      });
  }

  private callExportApi(request: any): void {
    this.loaderService.start();
    this.academyHttpService
      .exportReportData(request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.data?.includes('http')) {
            this.exportUrl = response.data;
            this.exportUrlGenerated.emit(this.exportUrl);
            this.toastr.success(
              'Export Successful, please click on view report to access the report',
              'Success'
            );
          } else {
            this.toastr.error(response.data || 'Unknown error', 'Export Error');
          }
        },
      });
  }

  // Get colums of report data
  getColumns(data: any[]): string[] {
    if (data.length > 0) {
      return Object.keys(data[0]); // Get column names from the first object
    }
    return [];
  }

  // Update pagainated data
  updatePaginatedData(): void {
    const startIndex = this.currentPage * this.pageSize;
    this.paginatedData = this.data.slice(startIndex, startIndex + this.pageSize);
  }

  // On page change
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex; // Update current page index
    this.pageSize = event.pageSize; // Update page size if changed
    this.updatePaginatedData(); // Update paginated data
  }
}
