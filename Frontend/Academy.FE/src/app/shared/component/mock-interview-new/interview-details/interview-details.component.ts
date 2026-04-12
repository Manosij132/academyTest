import { Component, ViewChild, AfterViewInit, OnInit, TemplateRef, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { FormsModule, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { DialogData } from '../common-dialog/models/dialog-data.model';
import { InterviewsService } from '../../../../services/interviews.service';
import { CommonDialogComponent } from '../common-dialog/common-dialog.component';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { TextFieldModule } from '@angular/cdk/text-field';

@Component({
  selector: 'app-interview-details',
  standalone: true,
  imports: [CommonModule, MatTableModule, TextFieldModule,MatSortModule, FormsModule,MatProgressSpinner, MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule, MatPaginatorModule, MatButtonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './interview-details.component.html',
  styleUrl: './interview-details.component.css'
})
export class InterviewDetailsComponent implements OnInit, AfterViewInit {
  interviewDetails: any[] = [];
  loading = false;
  error: string | null = null;
  addInterviewDetails: boolean = false;
  selectedInterviewDetails: any | null = null;
  interviewToBeDeleted:any | null = null;

  displayedColumns: string[] = ['id','interviewCode', 'seqId', 'sessionStartTimes', 'sessionEndTimes', 'videoPath','audioPath','transcriptPath', 'actions'];
  dataSource = new MatTableDataSource<any>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('addInterviewDetailTemplate') addInterviewDetailTemplate!: TemplateRef<any>;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

    form: FormGroup;
    dialogLoading = false;
    dialogError: string | null = null;
    searchText=''

  constructor(private dialog: MatDialog, private fb: FormBuilder, private interviewService: InterviewsService){
      this.form = this.fb.group({
          questionId: [101],
          sessionStartTimes: [["2024-02-09T10:00:00+05:30"]],
          sessionEndTimes: [["2024-02-09T11:00:00+05:30"]],
          videoPath: ['', [Validators.required]],
          audioPath: ['', [Validators.required]],
          hasAnswered: [true],
          interviewCode: ['', [Validators.required]],
          seqId: ['', [Validators.required]],
          transcriptPath: ['', [Validators.required]],
          id: [],
      });
  }

  ngOnInit() {
    this.fetchInterviewDetails()
  }

  ngAfterViewInit(): void {
    this.paginatorList.changes.subscribe((paginators) => {
      if (paginators.first) {
        this.dataSource.paginator = paginators.first;
      }
    });
  }
  applyFilter() {
  this.dataSource.filter = this.searchText.trim().toLowerCase();
    
    // if (this.dataSource.paginator) {
    //   this.dataSource.paginator.firstPage();
    // }
  }

  public fetchInterviewDetails() {
     this.interviewService.fetchInterviewDetails('').subscribe((details: any) => {
        this.interviewDetails = details?.length ? structuredClone(details) : [];
        this.dataSource.data = this.interviewDetails;
     })
  }

  public onCreateInterviewDetails(){
       this.form.reset();
  
      this.form.patchValue({
        questionId: '',
        sessionStartTimes: '',
        sessionEndTimes: '',
        videoPath: '',
        audioPath: '',
        hasAnswered:true,
        interviewCode:'',
        seqId: '',
        transcriptPath:'',
        id: '',
    }); 
    this.dialogError = null;
    this.dialogLoading = false;
    this.form.get('interviewCode')?.enable();

      const dialogData: DialogData = {
          title: 'Add Evaluation Details',
          message: '',
          confirmText: 'Add',
          cancelText: 'Cancel',
          showActions: false,
          form: this.form,
          template: this.addInterviewDetailTemplate
      };

      const dialogRef = this.dialog.open(CommonDialogComponent, {
          width: '600px',
          data: dialogData,
      });

      dialogRef.afterClosed().subscribe((result) => {
          if (result) {
              this.onSubmitDialog();
          }
          this.onCancelDialog();
      });
  }

  public backToList(callApi: boolean) {
      callApi && this.fetchInterviewDetails();
      this.addInterviewDetails = false;
      this.selectedInterviewDetails = null;
  }

  public editInterviewDetails(interviewDetail: any) {
    this.selectedInterviewDetails = structuredClone(interviewDetail);
    
    // Populate form with existing data
    this.form.patchValue({
      questionId: interviewDetail.questionId || 101,
      sessionStartTimes: interviewDetail.sessionStartTimes || ["2024-02-09T10:00:00+05:30"],
      sessionEndTimes: interviewDetail.sessionEndTimes || ["2024-02-09T11:00:00+05:30"],
      videoPath: interviewDetail.videoPath || '',
      audioPath: interviewDetail.audioPath || '',
      hasAnswered: interviewDetail.hasAnswered || true,
      interviewCode: interviewDetail.interviewCode || '',
      seqId: interviewDetail.seqId || '',
      transcriptPath: interviewDetail.transcriptPath || '',
      id: interviewDetail.id
    });
    this.form.get('interviewCode')?.disable();
    this.dialogError = null;
    this.dialogLoading = false;

    const dialogData: DialogData = {
      title: 'Edit evaluation Details',
      message: '',
      confirmText: 'Update',
      cancelText: 'Cancel',
      showActions: false,
      form: this.form,
      template: this.addInterviewDetailTemplate
    };

    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: '600px',
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.onSubmitDialog();
      }
      this.onCancelDialog();
    });
  }

  public onDeleteInterviewDetails(row: any) {
    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: '500px',
      data: {
        title: 'Delete Evaluation Details',
        message: 'Are you sure you want to delete this evaluation detail?',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        confirmButtonColor: 'warn',
        showActions: true
      }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
          this.loading = true;
          this.interviewService.deleteInterview(row).subscribe({
              next: () => {
                this.loading = false;
                  this.fetchInterviewDetails();
                
              },
              error: () => {
                  this.loading = false;
                  this.error = 'Failed to delete evaluation details.';
              }
          });
      } else {
        this.cancelDelete();
      }
    });
  }

  public confirmDelete() {
    if (!this.interviewToBeDeleted) return;
    
    this.loading = true;
    this.error = null;
    
    this.interviewService.deleteInterview(this.interviewToBeDeleted).subscribe({
      next: () => {
        this.loading = false;
        this.interviewToBeDeleted = null;
        this.dialog.closeAll();
        this.fetchInterviewDetails();
      },
      error: () => {
        this.error = 'Failed to delete evaluation details';
        this.loading = false;
      }
    });
  }

  public cancelDelete() {
    this.interviewToBeDeleted = null;
    this.error = null;
    this.dialog.closeAll();
  }

    onSubmitDialog(): void {
        if (this.form.invalid) return;

        this.dialogLoading = true;
        this.dialogError = null;
        const payload = this.form.getRawValue(); 

        (this.selectedInterviewDetails ? this.interviewService.updateInterview(payload) : this.interviewService.createInterviewDetails(payload)).subscribe({
            next: () => {
                this.dialogLoading = false;
                this.dialog.closeAll();
                this.fetchInterviewDetails();
            },
            error: () => {
                this.dialogError = 'Failed to add evaluation details';
                this.dialogLoading = false;
            }
        });
    }

    onCancelDialog(): void {
      this.form.get('interviewCode')?.enable();
        this.dialog.closeAll();
    }
}
