import { CommonModule } from "@angular/common";
import { Component, OnInit, ViewChild, AfterViewInit } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatDialogModule, MatDialog } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatPaginatorModule, MatPaginator } from "@angular/material/paginator";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatSortModule, MatSort } from "@angular/material/sort";
import { MatTableModule, MatTableDataSource } from "@angular/material/table";
import { InterviewsService } from "../../../../services/interviews.service";
import { LoaderService } from "../../../../services/loader.service";

export interface InterviewQuestion {
  id?: number;
  interviewCode?: string;
  questionNumber: number;
  videoFilePath?: string;
  audioFilePath?: string;
}

@Component({
  selector: "app-rabbitmq",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDialogModule,
    FormsModule,
  ],
  templateUrl: "./rabbitmq.component.html",
  styleUrl: "./rabbitmq.component.css",
})
export class RabbitmqComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = [
    "interviewCode",
    "questionNumber",
    "videoFilePath",
    "audioFilePath",
    "actions",
  ];
  dataSource = new MatTableDataSource<InterviewQuestion>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  rabbitMQForm: FormGroup;
  isEditing = false;
  editingId: number | null = null;
  showForm = true;
  searchText = "";
  constructor(
    private interviewService: InterviewsService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    public loaderService: LoaderService
  ) {
    this.rabbitMQForm = this.fb.group({
      interviewCode: [{ value: "", disabled: true }],
      questionNumber: ["", [Validators.required, Validators.min(1)]],
      videoFilePath: [""],
      audioFilePath: [""],
    });
  }

  ngOnInit() {
    this.loadData();
  }

  ngAfterViewInit() {
    // Set initial page size
    if (this.paginator) {
      this.paginator.pageSize = 5;
    }
  }

  ngAfterViewChecked() {
    if (this.sort && this.dataSource.sort !== this.sort) {
      this.dataSource.sort = this.sort;
    }
    if (this.paginator && this.dataSource.paginator !== this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }

  loadData() {
    this.loaderService.start();
    this.interviewService.getRabbitMQData().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.loaderService.stop();
      },
      error: (error) => {
        console.error("Error loading data:", error);
        this.snackBar.open("Error loading data", "Close", { duration: 3000 });
        this.loaderService.stop();
      },
    });
  }

  onSubmit() {
    if (this.rabbitMQForm.valid) {
      const formData = this.rabbitMQForm.value;

      if (this.isEditing && this.editingId) {
        this.updateEntry(this.editingId, formData);
      } else {
        this.createEntry(formData);
      }
    }
  }

  createEntry(data: InterviewQuestion) {
    // this.interviewService.createRabbitMQEntry(data).subscribe({
    //   next: (response) => {
    //     this.snackBar.open('Entry created successfully', 'Close', { duration: 3000 });
    //     this.resetForm();
    //     this.loadData();
    //   },
    //   error: (error) => {
    //     console.error('Error creating entry:', error);
    //     this.snackBar.open('Error creating entry', 'Close', { duration: 3000 });
    //     this.loading = false;
    //   }
    // });
  }

  updateEntry(id: number, data: InterviewQuestion) {
    // this.interviewService.updateRabbitMQEntry(id, data).subscribe({
    //   next: (response) => {
    //     this.snackBar.open('Entry updated successfully', 'Close', { duration: 3000 });
    //     this.resetForm();
    //     this.loadData();
    //   },
    //   error: (error) => {
    //     console.error('Error updating entry:', error);
    //     this.snackBar.open('Error updating entry', 'Close', { duration: 3000 });
    //     this.loading = false;
    //   }
    // });
  }

  editEntry(element: InterviewQuestion) {
    this.isEditing = true;
    this.editingId = element.id || null;
    this.showForm = true;

    this.rabbitMQForm.patchValue({
      interviewCode: element.interviewCode || "",
      questionNumber: element.questionNumber,
      videoFilePath: element.videoFilePath || "",
      audioFilePath: element.audioFilePath || "",
    });
  }

  resetForm() {
    this.rabbitMQForm.reset();
    this.isEditing = false;
    this.editingId = null;
    this.showForm = false;
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm) {
      this.resetForm();
    }
  }

  applyFilter() {
    this.dataSource.filter = this.searchText.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  retryRabbitMQEntry(data: InterviewQuestion) {
    this.loaderService.start();
    this.interviewService.retryRabbitMQEntry(data).subscribe({
      next: (data) => {
        // this.dataSource.data = data;
        this.loaderService.stop();
        this.loadData();
      },
    });
  }
}
