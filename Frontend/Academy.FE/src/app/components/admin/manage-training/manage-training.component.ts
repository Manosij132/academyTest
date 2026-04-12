import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AcademyHttpService } from '@services/academy-http.service';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { LoaderService } from '@services/loader.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { debounceTime, distinctUntilChanged, finalize, Observable, switchMap } from 'rxjs';
import { AcademyResponse } from '@shared/dto/academy-response.dto';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AdminManageTraining } from '@shared/Interface/admin-manage-training';

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
  ],
  templateUrl: './manage-training.component.html',
  styleUrl: './manage-training.component.scss'
})

export class ManageTrainingComponent implements OnInit {
  displayedColumns: string[] = ["priority", "trainingname"];
  trainings: AdminManageTraining[] = [];
  dataSource = new MatTableDataSource<AdminManageTraining>();
  pageSize: number = 10;
  pageIndex: number = 0;
  totalPages: number = 0;
  totalItems: number = 0;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  color = '#bfd732';
  private readonly _route = inject(ActivatedRoute)
  protected pageHeader = this._route.snapshot.data["pageHeader"];
  snrForm!: FormGroup;
  trainingControl = new FormControl<string>("");
  filteredTraining$: Observable<string | null>;
  selectedTraining: Set<AdminManageTraining> = new Set();

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private fb: FormBuilder,
    private loaderService: LoaderService
  ) {
    this.filteredTraining$ = this.trainingControl.valueChanges.pipe(
      debounceTime(300), // Wait for 300ms between keystrokes
      distinctUntilChanged(), // Only proceed if the input has changed
    );
  }

  ngOnInit(): void {
    this.setTrainingMasterList('', this.pageIndex, this.pageSize);
    this.initForm()

    this.snrForm.get('trainingName')!.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        switchMap(value => {
          return this.academyHttpService
            .fetchPagedTraining(value, this.pageIndex, this.pageSize)
        })
      )
      .subscribe({
        next: (resposne: AcademyResponse) => {
          if (resposne.success) {
            this.configurePaginator(resposne.data)
            this.trainingBind(resposne.data.items);
          }
          else {
            this.toastr.error('Something went wrong...PLease try again later');
          }
        },
      });
  }

  initForm() {
    this.snrForm = this.fb.group({
      trainingName: [''],
      trainingPriority: [null]
    });
  }

  private setTrainingMasterList(filterByTrainingName: string, pageIndex: number, pageSize: number): void {
    this.loaderService.start();
    this.academyHttpService
      .fetchPagedTraining(filterByTrainingName, pageIndex, pageSize)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (resposne: AcademyResponse) => {
          if (resposne.success) {
            this.configurePaginator(resposne.data)
            this.trainingBind(resposne.data.items);
          }
          else {
            this.toastr.error('Something went wrong...PLease try again later');
          }
        },
      });
  }

  private trainingBind(trainings: AdminManageTraining[]): void {
    this.trainings = trainings;
  }

  private configurePaginator(data: any) {
    this.totalPages = data.totalPages;
    this.pageSize = data.pageSize;
    this.pageIndex = data.pageIndex;
    console.log(this.pageIndex);
    this.totalItems = data.totalCount;
  }

  onPageChanged(e: PageEvent) {
    this.setTrainingMasterList('', e.pageIndex + 1, e.pageSize);
  }

  isSelected(row: AdminManageTraining): boolean {
    if (row.isPriortize) {
      return true;
    }
    else {
      return false;
    }
  }

  onCheckboxChange(row: AdminManageTraining): void {
    row.isPriortize = !row.isPriortize
    this.selectedTraining.add(row);
    console.log(this.selectedTraining);
    this.loaderService.start();
    this.academyHttpService.updateTrainingPriority(row)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (res: any) => {
          if (res.status === 200) {
            this.toastr.success(`${row.trainingName} priority is set to ${row.isPriortize}`, "Success");
          } else {
            this.toastr.error(res?.message || "Error", "Error");
          }
        },
        error: (err) => {
          const validationErrors = err?.error?.errors;
          if (validationErrors) {
            console.error("Validation errors:", validationErrors);
            this.toastr.error(
              "Validation error: " + JSON.stringify(validationErrors),
              "Error"
            );
          } else {
            const errMsg =
              err?.error?.message || "Unexpected error during save.";
            this.toastr.error(errMsg, "Error");
          }
        },
      });
  }
}
