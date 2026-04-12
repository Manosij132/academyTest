import { Component, Input, OnInit, AfterViewInit, ViewChild } from "@angular/core";
import { MatAccordion, MatExpansionModule } from "@angular/material/expansion";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { BookmarkFilterComponent } from "@components/reporting/training-report/bookmark-filter/bookmark-filter.component";
import { CommonModule } from "@angular/common";
import { GenerateReportComponent } from "./generate-report/generate-report.component";
import {
  BookmarkForms,
  EmailColumnsModel,
} from "@shared/dto/bookmark-form.dto";
import { PreviewSendEmailComponent } from "./preview-send-email/preview-send-email.component";
import { MatButton } from "@angular/material/button";
import { MatIconModule } from '@angular/material/icon'; 
import { MatTooltipModule } from '@angular/material/tooltip';


@Component({
  selector: "app-training-report",
  standalone: true,
  imports: [
    MatExpansionModule,
    CommonModule,
    RouterModule,
    BookmarkFilterComponent,
    GenerateReportComponent,
    PreviewSendEmailComponent,
    MatButton,
    MatIconModule,
    MatTooltipModule   
  ],
  templateUrl: "./training-report.component.html",
  styleUrls: ["./training-report.component.scss"],
})
export class TrainingReportComponent implements OnInit, AfterViewInit {
  exportUrl: string = '';
  reportDataForm!: BookmarkForms;
  sendEmailFields!: EmailColumnsModel;
  generateReportExpand = false;
  isDisableEmailBody = false;
  dataSizeLimit: number = 20;
  autoGenerate = false;
  private pendingAutoGenerate = false; 
  bookmarkName = '';
  @ViewChild(GenerateReportComponent) GenerateReportComponent!: GenerateReportComponent;
  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) { }
  ngOnInit(): void {
    this.route.params.subscribe((x) => {
      const bookmarkId = +x["id"];
      this.autoGenerate = x["autoGenerate"] === "true";
      this.bookmarkName = x["name"] ?? '';
      if (this.reportDataForm) {
        this.reportDataForm.BookMarkId = bookmarkId;
      } else {
        this.reportDataForm = new BookmarkForms();
        this.reportDataForm.BookMarkId = bookmarkId;
      }
      if (this.autoGenerate && bookmarkId) {
        this.generateReportExpand = true;
        this.pendingAutoGenerate = true; 
      }
    });
  }

  ngAfterViewInit(): void {
    if (this.pendingAutoGenerate && this.GenerateReportComponent) {
      setTimeout(() => {
        this.GenerateReportComponent.fetchData(this.reportDataForm);
        this.pendingAutoGenerate = false;
      }, 100); 
    }
  }

  goBackToBookmarks(): void {
    this.router.navigate(["/trainingreportbookmarks"]);
  }

  onGenerateReport(bookmarkForms: BookmarkForms) {
    this.generateReportExpand = true;
    this.exportUrl = ''; 
    this.reportDataForm = { ...bookmarkForms };
    this.bookmarkName = bookmarkForms.BookMarkName ?? ''; 
    if (this.GenerateReportComponent) {
      this.GenerateReportComponent.fetchData(this.reportDataForm);
    }
  }

  onUpdateBookmarkId(bookmarkId: number) {
    this.reportDataForm = { ...this.reportDataForm };
    this.reportDataForm.BookMarkId = bookmarkId;
  }

  onUpdateEmailFields(event: EmailColumnsModel) {
    this.sendEmailFields = { ...event };
  }

  onOpened(event: any) {
    if (this.reportDataForm && this.reportDataForm.BookMarkId) {
      this.onUpdateBookmarkId(this.reportDataForm.BookMarkId);
    }
  }

  updateDataSize(dataSize: number) {
    this.isDisableEmailBody = dataSize > this.dataSizeLimit;
  }
  onExportReport(): void {
    if (!this.GenerateReportComponent) return;
    if (this.exportUrl) {
    window.open(this.exportUrl, '_blank', 'noopener,noreferrer');
    } else {
    this.GenerateReportComponent.exportReport(this.reportDataForm);
    }
}
  onExportUrlGenerated(url: string): void {
    this.exportUrl = url;  // '' resets icon back to download
  }
}
