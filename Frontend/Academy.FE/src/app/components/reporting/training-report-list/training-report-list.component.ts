import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PageEvent } from '@angular/material/paginator'; // Import PageEvent
import { MatPaginatorModule } from '@angular/material/paginator';
import { CommonModule } from '@angular/common';
import { AcademyHttpService } from '@services/academy-http.service';
import { ToastrService } from 'ngx-toastr';
import { LoaderService } from '@services/loader.service';
import { finalize } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: 'app-training-report-list',
  standalone: true,
  imports: [CommonModule, MatPaginatorModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './training-report-list.component.html',
  styleUrls: ['./training-report-list.component.scss']
})
export class TrainingReportListComponent implements OnInit {
  bookmarks: any[] = []; // Array to hold bookmarks
  paginatedBookmarks: any[] = []; // Array to hold paginated bookmarks
  pageSize: number = 5; // Number of items per page
  currentPage: number = 0; // Current page index
  activitytype = "Training";

  constructor(private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService, private router: Router) {
    //
  }

  ngOnInit(): void {
    this.loadBookmarks();
  }

  // Fetch bookmarks list data
  loadBookmarks(): void {
    this.fetchBookmarks();
  }

  // Update paginated bookmarks
  updatePaginatedBookmarks(): void {
    const startIndex = this.currentPage * this.pageSize;
    this.paginatedBookmarks = this.bookmarks.slice(startIndex, startIndex + this.pageSize);
  }

  // On page change
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex; // Update current page index
    this.pageSize = event.pageSize; // Update page size if changed
    this.updatePaginatedBookmarks(); // Update paginated bookmarks
  }

  // Edit bookmarks record from list
  editBookmark(bookmarkId: number): void {
    this.router.navigate(['/trainingreport', { id: bookmarkId }]);
  }

  // Create new report
  createNewReport(): void {
    this.router.navigate(['/trainingreport']);
  }

  // Fetch bookmarks
  fetchBookmarks() {
    this.loaderService.start();
    this.academyHttpService
      .fetchBookmarkList()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          this.bookmarks = response.data;
          if (response.status === 200) {
            this.bookmarks = response.data;
            this.updatePaginatedBookmarks(); // Update paginated bookmarks
          } else {
            this.toastr.error(response.errorMessage, "Trainings load Error");
          }
        },
      });
  }

  // Delete bookmark from list
  deleteBookmark(bookMarkId: number) {
    this.loaderService.start();
    this.academyHttpService
      .deleteBookmark(bookMarkId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.toastr.success(response?.message || TOASTER_MESSAGES.DELETE_SUCCESS, "Success");
          } else {
            this.toastr.error(response.errorMessage, "Delete bookmark Error");
          }
        },
        complete: () => {
          this.loadBookmarks(); // Reload bookmarks after deletion
        },
      });
  }
   // Edit bookmarks record from list
   viewBookmark(bookmarkId: number, bookmarkName: string): void {
    this.router.navigate(['/trainingreport', { id: bookmarkId, autoGenerate: true, name: bookmarkName }]);
  }
}