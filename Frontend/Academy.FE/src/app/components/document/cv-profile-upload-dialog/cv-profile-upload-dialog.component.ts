import { Component } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { MatIconModule } from "@angular/material/icon";
import { MatButtonModule } from "@angular/material/button";
import { MatTableModule } from "@angular/material/table";
import { finalize } from "rxjs";
import { LoaderService } from "@services/loader.service";
import { AcademyHttpService } from "@services/academy-http.service";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Inject } from "@angular/core";
import { MatDialogModule } from "@angular/material/dialog";
import { ToastrService } from "ngx-toastr";
import { MatTooltipModule } from "@angular/material/tooltip";

@Component({
  selector: "app-info-dialog",
  standalone: true,
  templateUrl: "./cv-profile-upload-dialog.component.html",
  styleUrls: ["./cv-profile-upload-dialog.component.css"],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatDialogModule,
    MatTooltipModule
  ],
})
export class CvProfileUploadDialogComponent {
  employee: any = {};
  selectedFile: File | null = null;
  docType = '';
  docTypeId: number;
  documentTypes: any[] = [];
  viewFileUpload = false;
  existingDocLink = '';
  allowedTypes = '';

  /* Drag & Drop */
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();

    if (event.dataTransfer?.files.length) {
      this.selectedFile = event.dataTransfer.files[0];
    }
  }
  
  constructor(
    public dialogRef: MatDialogRef<CvProfileUploadDialogComponent>,
    private loaderService: LoaderService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService
  ) {
    this.employee = data?.employee;
    this.viewFileUpload = false;
    this.existingDocLink = '';
    this.allowedTypes = '.pdf,.doc,.docx,.ppt,.pptx';
  }

  ngOnInit() {
    console.log(this.employee);
    this.loadDocumentTypes();
  }

  loadDocumentTypes() {
    this.academyHttpService.fetchAllAdocumentType().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.documentTypes = res.data;
          const cvType = this.documentTypes.find(
            x => x.documentType.toLowerCase() === 'cv'
          );

          if (cvType) {
            this.docTypeId = cvType.documentTypeId;
          }
        } else {
          console.error('Failed to load document types');
        }
      },
      error: (err) => {
        console.error('Error fetching document types', err);
      }
    });
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  clearFile(fileInput: HTMLInputElement) {
    this.selectedFile = null; fileInput.value = '';
  }

  onUpload() {
    if (!this.selectedFile) return;

    this.loaderService.start();
    this.academyHttpService
      .uploadEmployeeCV(this.selectedFile, this.employee.employeeId, this.employee.community, this.docTypeId, this.existingDocLink)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            console.log(response)
            this.toastr.success('File uploaded successfully.');
            this.dialogRef.close();
          } else {
            this.toastr.error(response.errorMessage, "Error");
            this.dialogRef.close();
          }
        },
      });
  }

  uploadCVClicked() {
    this.selectedFile = null;
    const cvDocType = this.documentTypes.find(type => type.documentType === 'cv');
    this.docType = cvDocType.DocumentType;
    this.docTypeId = cvDocType.DocumentTypeId;
    this.viewFileUpload = true;
    this.existingDocLink = this.employee?.cvLink;
    this.allowedTypes = '.docx';
  }

  uploadProfileClicked() {
    this.selectedFile = null;
    const cvDocType = this.documentTypes.find(type => type.documentType === 'profile');
    this.docType = cvDocType.DocumentType;
    this.docTypeId = cvDocType.DocumentTypeId;
    this.viewFileUpload = true;
    this.existingDocLink = this.employee?.profileLink;
    this.allowedTypes = '.pptx';
  }

  cancelFileUpload() {
    this.selectedFile = null;
    const cvDocType = this.documentTypes.find(type => type.documentType === 'cv');
    this.docType = cvDocType.DocumentType;
    this.docTypeId = cvDocType.DocumentTypeId;
    this.viewFileUpload = false;
    this.existingDocLink = '';
    this.allowedTypes = '.docx';
  }
}
