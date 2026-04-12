import { CommonModule, JsonPipe } from "@angular/common";
import {
  Component,
  EventEmitter,
  Input,
  Output,
  QueryList,
  SimpleChanges,
  ViewChild,
  ViewChildren,
} from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { InterviewsService } from "../../../../services/interviews.service";
import { MatIconModule } from "@angular/material/icon";
import { MatDialog } from "@angular/material/dialog";
import { LoaderService } from "../../../../services/loader.service";
import { parsePrompt } from "../prompt-details-dialog/prompt-parser";
import { PromptDetailsDialogComponent } from "../prompt-details-dialog/prompt-details-dialog.component";

@Component({
  selector: "app-candidate-interview-details",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatIconModule,
    MatButtonModule,
    JsonPipe,
  ],
  templateUrl: "./candidate-interview-details.component.html",
  styleUrl: "./candidate-interview-details.component.css",
})
export class CandidateInterviewDetailsComponent {
  @Input() candidate: any;
  @Input() selectedInterview: any;
  @Output() goBack = new EventEmitter();
  @ViewChildren(MatPaginator) paginatorList!: QueryList<MatPaginator>;
  activePromptID: number | undefined = 0;
  interviewDetails: any = null;
  loading = false;
  error: string | null = null;
  addInterviewDetails: boolean = false;
  selectedInterviewDetails: any | null = null;
  interviewToBeDeleted: any | null = null;

  displayedColumns: string[] = [
    "questionId",
    "question",
    "prompt",
    "comments",
    "score",
    "totalScore",
  ];
  dataSource = new MatTableDataSource<any>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private interviewService: InterviewsService,
    private dialog: MatDialog,
    private loaderService: LoaderService
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (
      changes["selectedInterview"] &&
      changes["selectedInterview"]?.currentValue?.interviewCode
    ) {
      this.fetchInterviewDetails(
        changes["selectedInterview"].currentValue?.interviewCode
      );
    }
  }
  navigateBack() {
    this.goBack.emit();
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

  public fetchInterviewDetails(id: string) {
    this.loaderService.start();
    this.interviewService
      .fetchInterviewDetailById(id)
      .subscribe((details: any) => {
        this.interviewDetails = details ? structuredClone(details) : [];
        this.dataSource.data = this.interviewDetails?.questions;
        this.loaderService.stop();
      });
  }

  public onCreateInterviewDetails() {
    this.addInterviewDetails = true;
  }

  public backToList(callApi: boolean) {
    callApi && this.fetchInterviewDetails(this.selectedInterview?.id);
    this.addInterviewDetails = false;
    this.selectedInterviewDetails = null;
  }

  public editInterviewDetails(interviewDetail: any) {
    this.selectedInterviewDetails = structuredClone(interviewDetail);
    this.addInterviewDetails = true;
  }

  public onDeleteInterviewDetails() {
    this.interviewService
      .deleteInterview(this.interviewToBeDeleted)
      .subscribe((details: any) => {
        this.interviewToBeDeleted = null;
        this.fetchInterviewDetails(this.selectedInterview?.id);
      });
  }
  onToggle(prompt: any, parsed: any) {
    this.activePromptID = prompt.questionId;

    const dialogRef = this.dialog.open(PromptDetailsDialogComponent, {
      width: '900px',        // fixed width
  maxWidth: '95vw',      // responsive for smaller screens
  height: 'auto',
      data: parsed,
    });
    dialogRef.afterClosed().subscribe(() => {
      this.activePromptID = 0;
    });
  }

  getParsedPrompt(row: any) {
    if (!row.analysis?.prompt) return null;
    return parsePrompt(row.analysis.prompt);
  }
}
