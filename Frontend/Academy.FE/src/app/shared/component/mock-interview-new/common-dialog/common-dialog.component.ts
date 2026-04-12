import { Component, Inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { DialogData } from './models/dialog-data.model';

@Component({
  standalone: true,
  selector: 'app-common-dialog',
  templateUrl: './common-dialog.component.html',
  styleUrls: ['./common-dialog.component.css'],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule
  ]
})
export class CommonDialogComponent {
  safeHtmlMessage: SafeHtml;
  isLoading = false;

  constructor(
      public dialogRef: MatDialogRef<CommonDialogComponent>,
      @Inject(MAT_DIALOG_DATA) public data: DialogData,
      private sanitizer: DomSanitizer
  ) {
    this.safeHtmlMessage = this.sanitizer.bypassSecurityTrustHtml(data.message || '');
  }

  onSubmit(): void {
    if (this.data.form && this.data.form.invalid) {
      this.data.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  isConfirmDisabled(): boolean {
    return this.isLoading || (this.data.form ? this.data.form.invalid : false) || 
    (this.data.isInvalidCandidateRef?.() ?? false);
  }

  getConfirmText(): string {
    return this.data.confirmText || 'OK';
  }
}
