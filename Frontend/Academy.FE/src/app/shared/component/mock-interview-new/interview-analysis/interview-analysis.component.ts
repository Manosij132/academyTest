import {Component, ViewChild, AfterViewInit, OnInit, TemplateRef, ViewChildren, QueryList} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import { DialogData } from '../common-dialog/models/dialog-data.model';
import { MatDialog } from '@angular/material/dialog';
import { InterviewsService } from '../../../../services/interviews.service';
import { CommonDialogComponent } from '../common-dialog/common-dialog.component';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { AIModelsService } from '../../../../services/aimodels.service';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { LoaderService } from '../../../../services/loader.service';
import { parsePrompt } from '../prompt-details-dialog/prompt-parser';
import { PromptDetailsDialogComponent } from '../prompt-details-dialog/prompt-details-dialog.component';


@Component({
  selector: 'app-interview-analysis',
  standalone:true,
    imports: [CommonModule, MatTableModule, MatSortModule, MatOptionModule,MatSelectModule,MatDialogModule, MatIconModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, MatPaginatorModule, MatButtonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './interview-analysis.component.html',
  styleUrl: './interview-analysis.component.css'
})
export class InterviewAnalysisComponent implements OnInit, AfterViewInit {
  interviewDetailsAnalysis: any[] = [];
  searchText=''
  error: string | null = null;
  addInterviewDetails: boolean = false;
  selectedInterviewDetails: any | null = null;
  selectedInterviewAnalysis: any | null = null;
  interviewToBeDeleted: any;
  aimodels:any
  activePromptID: number | undefined = 0;
  displayedColumns: string[] = ['interviewDetailId', 'modelId', 'prompt', 'score','totalScore' ,'comments', 'actions'];
  dataSource = new MatTableDataSource<any>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('addInterviewAnalysisTemplate') addInterviewAnalysisTemplate!: TemplateRef<any>;
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;

    form: FormGroup;
    dialogError: string | null = null;

  constructor(private dialog: MatDialog,private aimodelsService:AIModelsService, private fb: FormBuilder, private interviewService: InterviewsService,private loaderService:LoaderService){
      this.form = this.fb.group({
          interviewDetailId: ['', [Validators.required]],
          modelId: ['', [Validators.required]],
          prompt: [''],
          score: ['', [Validators.required]],
          totalScore: [10.0, [Validators.required]],
          comments: [true, [Validators.required]],
          // interviewCode: ['', [Validators.required]],
          id: []
      });
  }

  ngOnInit() {
    this.fetchAIModels()
    setTimeout(()=>this.fetchInterviewAnalysis(),500)

  }
  fetchAIModels() {
    this.loaderService.start();

    this.aimodelsService.getAll().subscribe({
      next: (data) => {
        this.aimodels = data;
        this.loaderService.stop();
      },
      error: (err) => {
        this.error = 'Failed to load AI Models';
        this.loaderService.stop();
      }
    });
  }

  ngAfterViewInit(): void {
    this.paginatorList.changes.subscribe((paginators) => {
      if (paginators.first) {
        this.dataSource.paginator = paginators.first;
      }
    });
  }

    ngAfterViewChecked() {
    if (this.sort && this.dataSource.sort !== this.sort) {
      this.dataSource.sort = this.sort;
    }
    if (this.paginator && this.dataSource.paginator !== this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }
  
  applyFilter() {
    this.dataSource.filter = this.searchText.trim().toLowerCase();
    
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  public fetchInterviewAnalysis() {
    this.loaderService.start();
     this.interviewService.fetchInterviewAnalysisDetails('').subscribe((details: any) => {
      let interviewDetailsAnalysis=details?.length ? structuredClone(details) : [];
      interviewDetailsAnalysis= interviewDetailsAnalysis?.map((model:any)=>{
        const aiModel=this.aimodels?.find((aiModel:any)=> aiModel.id == model.model )
        this.loaderService.stop();
        return {
          ...model,aiModelName:aiModel?.modelName ?? '-'
        }
        
      })
 

        this.interviewDetailsAnalysis = interviewDetailsAnalysis?.length ? structuredClone(interviewDetailsAnalysis) : [];
      
        this.dataSource.data = this.interviewDetailsAnalysis;
     })
  }
  
  public onCreateInterviewAnalysis(){
      this.form.reset();
      this.dialogError = null;
      this.form.get('interviewDetailId')?.enable();

      const dialogData: DialogData = {
          title: `${this.selectedInterviewAnalysis ? 'Edit': 'Add'} Evaluation Analysis`,
          message: '',
          confirmText: 'Add',
          cancelText: 'Cancel',
          showActions: false,
          form: this.form,
          template: this.addInterviewAnalysisTemplate
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
      callApi && this.fetchInterviewAnalysis();
      this.addInterviewDetails = false;
      this.selectedInterviewDetails = null;
  }

  public editInterviewDetails(interviewAnalysis: any) {
    this.selectedInterviewAnalysis = structuredClone(interviewAnalysis);
    this.selectedInterviewDetails = structuredClone(interviewAnalysis);

    this.form.patchValue({
      interviewDetailId: interviewAnalysis.interviewDetailId || '',
      modelId: interviewAnalysis.modelId || '',
      prompt: interviewAnalysis.prompt || '',
      score: interviewAnalysis.score || '',
      totalScore: interviewAnalysis.totalScore || 10.0,
      comments: interviewAnalysis.comments || '',
      id: interviewAnalysis.id
      
    });
    this.form.get('interviewDetailId')?.disable();
    this.dialogError = null;

    const dialogData: DialogData = {
      title: 'Edit Evaluation Analysis',
      message: '',
      confirmText: 'Update',
      cancelText: 'Cancel',
      showActions: false,
      form: this.form,
      template: this.addInterviewAnalysisTemplate
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

    onSubmitDialog(): void {
        if (this.form.invalid) return;

        this.loaderService.start();
        this.dialogError = null;
        const payload = this.form.getRawValue(); 

        (this.selectedInterviewDetails ? this.interviewService.updateInterviewAnalysis(payload) : this.interviewService.createInterviewAnalysis(payload)).subscribe({
            next: () => {
               this.loaderService.stop();
                this.dialog.closeAll();
                this.fetchInterviewAnalysis();
            },
            error: () => {
                this.dialogError = 'Failed to add evaluation details';
               this.loaderService.stop();
            }
        });
    }

    onCancelDialog(): void {
        this.dialog.closeAll();
        this.selectedInterviewAnalysis=null;
    }
    onToggle(prompt:any,parsed:any) {
        this.activePromptID = prompt.id;
        const dialogRef = this.dialog.open(PromptDetailsDialogComponent, {
          width: "500px",
          data: parsed,
        });
        dialogRef.afterClosed().subscribe(() => {
          this.activePromptID = 0;
        });
      }

      getParsedPrompt(row: any) {
        if (!row?.prompt) return null;
        return parsePrompt(row.prompt);
      }
}
