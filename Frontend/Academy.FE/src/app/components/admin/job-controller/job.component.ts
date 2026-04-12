import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';
import { AcademyHttpService } from '@services/academy-http.service';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { LoaderService } from '@services/loader.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface Job {
  jobName: string;
  jobDescription: string;
  isActive: boolean;
}

@Component({
  selector: 'app-manage-training',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule
  ],
  templateUrl: './job.component.html',
  styleUrls: ['./job.component.scss']
})
export class JobComponent implements OnInit {
  displayedColumns: string[] = ["jobname", "jobdescription", "status", "action"];
  dataSource = new MatTableDataSource<Job>();
  pageSize: number = 10;
  pageIndex: number = 0;
  totalPages: number = 0;
  totalItems: number = 0;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  color = '#bfd732';

  private readonly _route = inject(ActivatedRoute)
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private fb: FormBuilder,
    private loaderService: LoaderService
  ) { }

  jobs: Job[] = [];

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs() {
    this.loaderService.start();
    this.academyHttpService
      .fetchJobs()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          this.loaderService.stop();
          if (response.status === 200) {
            this.dataSource.data = response.data;
          } else {
            this.toastr.error(
              response.errorMessage,
              "Error While Fetching Data"
            );
          }
        },
      });
  }
}
