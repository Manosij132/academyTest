import { Component, Inject, Input, OnChanges, PLATFORM_ID, SimpleChanges } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { AcademyHttpService } from "@services/academy-http.service";
import { ActivatedRoute } from "@angular/router";
import { SafeHtml } from "@angular/platform-browser";
import { MatExpansionModule } from "@angular/material/expansion";
import { EmailColumnsModel } from "@shared/dto/bookmark-form.dto";
import { QuillModule } from "ngx-quill";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { PreviewEmailModalComponent } from "@components/reporting/training-report/preview-send-email/preview-email-modal/preview-email-modal.component";
import { ToastrService } from "ngx-toastr";
import { LoaderService } from "@services/loader.service";
import { finalize } from "rxjs";
import { multiEmailValidator } from "@shared/validators/multi-email.validator";
import { isPlatformBrowser } from "@angular/common";
import { CommonModule } from "@angular/common";

export interface ReportEmailRequest {
  bookMarkId: number;
  emailTo: string;
  emailCC: string;
  emailSubject: string;
  isDataMore: boolean;
}
@Component({
  selector: "app-preview-send-email",
  standalone: true,
  imports: [
    ReactiveFormsModule, // Import for standalone component
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatExpansionModule,
    QuillModule,
    MatDialogModule,
    CommonModule
  ],
  templateUrl: "./preview-send-email.component.html",
  styleUrl: "./preview-send-email.component.css",
})
export class PreviewSendEmailComponent implements OnChanges {
  // Declare the FormGroup that will manage the form controls
  emailForm!: FormGroup;
  quillEditorConfig = {
    toolbar: [
      [{ size: ["12px", false, "14px", "16px"] }],
      ["bold", "italic", "underline"],
      [{ list: "ordered" }, { list: "bullet" }],
      [{ color: [] }, { background: [] }],
      ["clean"],
    ],
  };
  loadPreview!: SafeHtml;
  @Input() bookmarkId!: number;
  @Input() sendEmailFields!: EmailColumnsModel;
  @Input() isDisableEmailBody: boolean = false;
  isSendEmail: boolean = false;
  isQuillReady = false;
  constructor(
    private fb: FormBuilder,
    private readonly academyHttpService: AcademyHttpService,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }
  /**
   * Initializes the form group with controls and their validators.
   */
  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      import("quill")
        .then((QuillModule) => {
          const Quill = QuillModule.default;
          let Size = Quill.import("attributors/style/size") as any;
          Size.whitelist = ["12px", false, "14px", "16px"];
          // Register the updated Size Attributor with Quill
          Quill.register(Size, true);
          const Block = Quill.import("blots/block") as any;
          class DivBlot extends Block { }
          DivBlot["blotName"] = "block";
          DivBlot["tagName"] = "DIV";
          Quill.register(DivBlot, true);
          this.isQuillReady = true;
        })
        .catch((err) => {
          console.error("Error lazy-loading Quill:", err);
        });
    }
    this.initializeForm();
  }

  initializeForm() {
    if (!this.emailForm) {
      this.emailForm = this.fb.group({
        to: ["", [Validators.required, multiEmailValidator()]],
        cc: ["", [multiEmailValidator()]],
        body: ["", [Validators.required]],
        subject: ["", Validators.required],
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes &&
      changes["sendEmailFields"] &&
      changes["sendEmailFields"].currentValue
    ) {
      this.sendEmailFields = changes["sendEmailFields"].currentValue;
      this.initializeForm();
      this.emailForm.patchValue({
        to: this.sendEmailFields.emailTo,
        cc: this.sendEmailFields.emailCC,
        subject: this.sendEmailFields.emailSubject,
        body: this.sendEmailFields.emailBody,
      });
    }
    if (
      changes &&
      changes["isDisableEmailBody"]
    ) {
      this.isDisableEmailBody = changes["isDisableEmailBody"].currentValue;
      const emailBodyField = this.emailForm.get('body');
      if (this.isDisableEmailBody) {
        emailBodyField?.setValidators([]);
        emailBodyField?.patchValue("");
      } else {
        emailBodyField?.setValidators([Validators.required]);
        emailBodyField?.patchValue(this.sendEmailFields.emailBody);
      }
    }
  }

  /**
   * Convenience getter for easy access to form controls in the template.
   * Example: f['controlName'] instead of emailForm.controls['controlName']
   */
  get f() {
    return this.emailForm.controls;
  }

  /**
   * Handles the form submission.
   * It checks if the form is valid and logs the form data to the console.
   * In a real application, this is where you would send the data to a backend service.
   */
  onSendEmail(): void {
    this.isSendEmail = true;
    if (this.emailForm.invalid) {
      return;
    }
    this.loaderService.start();
    this.academyHttpService
      .sendReportOnEmail({
        bookMarkId: +this.bookmarkId,
        emailCC: this._removeLeadingTrailingCommas(
          this.emailForm.get("cc")?.value
        ),
        emailSubject: this.emailForm.get("subject")?.value,
        emailTo: this._removeLeadingTrailingCommas(
          this.emailForm.get("to")?.value
        ),
        emailBody: this.getEmailBody,
        isDataMore: !this.loadPreview,
      } as ReportEmailRequest)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe((response) => {
        console.log("Email sent!", response);
        // Optionally reset the form
        this.isSendEmail = false;
        this.toastr.success("Emaill has been sent successfully", "");
      });
  }

  get isInvalidEmailBody() {
    return (
      (this.isSendEmail || this.emailForm.get("body")?.touched) &&
      this.emailForm.get("body")?.hasError("required")
    );
  }

  private _removeLeadingTrailingCommas(emails: string): string {
    if (!emails?.trim()) {
      return "";
    }

    return emails
      .trim()
      .split(",")
      .map((e: string) => e.trim())
      .filter((e: string) => e)
      .join(", ");
  }

  onPreviewEmailButtonClick(): void {
    this.previewReportData();
  }

  previewReportData() {
    if (this.bookmarkId) {
      this.academyHttpService.previewReportData(this.bookmarkId).subscribe({
        next: (response: any) => {
          if (response && response.success) {
            if (response.data !== "Data is more") {
              const dialogRef = this.dialog.open(PreviewEmailModalComponent, {
                width: "100%", // Use a percentage of the viewport width
                maxWidth: "1000px", // Set a max-width to prevent it from becoming too large on wide screens
                panelClass: "responsive-dialog", // Optional: for additional custom styling
                data: {
                  loadPreview: response.data,
                  emailBody: this.getEmailBody,
                },
              });

              dialogRef.afterClosed().subscribe((result) => {
                console.log("The dialog was closed with result:", result);
                // 'result' will be 'true' or 'false' from the dialog component
              });
            }
          }
        },
        complete: () => { },
      });
    }
  }
  // To add new line with br tag back into to div which was getting removed
  get getEmailBody() {
    const regex = /<div>\s*<\/div>/g;
    const replacement = "<div><br></div>";
    const originalHtml = this.emailForm.get("body")?.value;
    const formattedEmailHtml = originalHtml.replace(regex, replacement);
    return formattedEmailHtml;
  }
  get emailBodyPlaceholder() {
    return this.isDisableEmailBody ? "No manual input is required, as this will be an auto generated email." : "Compose your email body...";
  }
}
