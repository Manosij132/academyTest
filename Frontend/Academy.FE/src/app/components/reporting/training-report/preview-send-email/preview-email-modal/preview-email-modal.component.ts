import { Component, Inject, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { AcademyHttpService } from '@services/academy-http.service';
import { LoaderService } from '@services/loader.service';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-preview-email-modal',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  templateUrl: './preview-email-modal.component.html',
  styleUrl: './preview-email-modal.component.scss'
})
export class PreviewEmailModalComponent implements OnInit {
  htmlPreview!: SafeHtml;

  constructor(private readonly academyHttpService: AcademyHttpService,
    private sanitizer: DomSanitizer,
    public dialogRef: MatDialogRef<PreviewEmailModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { loadPreview: string, emailBody: string, }
  ) { }

  ngOnInit(): void {
    // this.previewReportData();
    // this.htmlPreview = this.loadPreview;
    const domParser = new DOMParser();
    const previewDocument = domParser.parseFromString(this.data.loadPreview, 'text/html');
    const targetEmailBodyDiv = previewDocument.getElementById('training-report-email-body');
    if (targetEmailBodyDiv) {
      targetEmailBodyDiv.innerHTML = this.data.emailBody;
    }
    const modifiedPreviewDoc = previewDocument.body.innerHTML;
    // Sanitize the final string before rendering
    this.htmlPreview = this.sanitizer.bypassSecurityTrustHtml(modifiedPreviewDoc);
  }

  // previewReportData() {
  //   if (this.data?.bookmarkId) {
  //     this.academyHttpService.previewReportData(this.data?.bookmarkId)
  //       .subscribe({
  //         next: (response: any) => {
  //           if (response && response.success) {
  //             if (response.data === 'Data is more') {

  //             } else {
  //               this.loadPreview = this.sanitizer.bypassSecurityTrustHtml(response.data);
  //             }
  //           }
  //         },
  //         complete: () => {
  //         },
  //       });
  //   }
  // }

  onCancel() {
    this.dialogRef.close();
  }
}
