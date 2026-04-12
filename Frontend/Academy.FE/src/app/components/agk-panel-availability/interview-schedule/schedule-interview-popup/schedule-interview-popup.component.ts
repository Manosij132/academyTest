import { AfterViewInit, ChangeDetectorRef, Component, Inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { PanelService } from '@services/panel.service';
import { panelSlotsCalenderEvent } from '../../model/panelSlotsCalenderEvent.model';
import { fileValidator } from '../../file-upload/file-upload.validator';
import { fileToBase64, fileToByteArray } from '../../file-upload/file-upload.util';
import { MatChipInputEvent } from '@angular/material/chips';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { InterviewScheduleData, SlotType } from '../heatmap-table/heatmap-table.model';
import { PanelSlotDataModel } from '../../model/panel-slot-data.model';
import { FileUploadComponent } from '../../file-upload/file-upload.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthenticationService } from '@services/authentication.service';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-schedule-interview-popup.component',
  templateUrl: './schedule-interview-popup.component.html',
  styleUrls: ['./schedule-interview-popup.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatChipsModule,
    MatDialogModule,
    MatIconModule,
    FileUploadComponent
  ]
})
export class ScheduleInterviewPopupComponent implements OnInit, AfterViewInit  {
  @ViewChild(FileUploadComponent, { static: false }) fileUploadComponent!: FileUploadComponent;
  @Input() updatedPanelData!: PanelSlotDataModel;
  @Input() panelData!: InterviewScheduleData;
  @Input() date!: string;
  @Input() time!: string;
  @Input() isEditing: boolean = false;
  scheduleForm!: FormGroup;
  panels: string[] = [];
  panelControl!: FormControl<string>;
  readonly separatorKeysCodes: number[] = [ENTER, COMMA];
  isFileUploadComponentVisible: boolean = false;

  selectable = true;
  removable = true;
  loggedinUser: any;
  popUpHeader = 'Schedule';
  popUpHeaderCancel = "Cancel";
  eventTitles= ['Globant L1 Technical interview ','Globant GK Technical interview '];
  loading: boolean = false;

  constructor(
    private panelService: PanelService,
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ScheduleInterviewPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private snackBar: MatSnackBar,
    private authenticationService : AuthenticationService,
    private cdRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.panelControl = this.fb.control<string>('', {
      validators: [Validators.required],
      nonNullable: true
    });

    if (this.isEditing) {
      this.popUpHeader = 'Edit an interview';
      this.panels.push(this.updatedPanelData.recruiter);
    } else {
       this.panels.push(this.panelData.emailId);
    }
    this.eventTitles = this.eventTitles.map(p=>p.concat(this.panelData.communityName)); 
    
    this.scheduleForm = this.fb.group({
      eventTitle: [
       this.isEditing ? this.updatedPanelData.eventTitle : (this.panelData.panel ===  'L1' ? this.eventTitles[0] : this.eventTitles[1]),
        Validators.required,
      ],
      panels: [this.panels, Validators.required],
      candidate: [
        this.isEditing ? this.updatedPanelData.candidateEmail : '',
        Validators.required,
      ],
      resume: [
        this.isEditing ? this.updatedPanelData.fileEncoded : null,
        fileValidator(50),
      ],
      scheduleDate: [new Date(this.date), Validators.required],
      scheduleTime: [this.time, Validators.required],
    });
  }

  ngAfterViewInit(){
    setTimeout(() => {
      this.cdRef.detectChanges();
      ////////////
      if (this.isEditing) {
        const base64toBlob = (b64Data: string) => {
          var sliceSize = 512;
          var byteCharacters = atob(b64Data);
          var byteArrays = [];
        
          for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);
            var byteNumbers = new Array(slice.length);
            for (var i = 0; i < slice.length; i++) {
              byteNumbers[i] = slice.charCodeAt(i);
            }
            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
          }
          return byteArrays;
        };
        const existingFile = new File(
          base64toBlob(this.updatedPanelData.fileEncoded),
          this.updatedPanelData.resumeFileName,
          { type: 'application/pdf' }
        );
        this.scheduleForm.patchValue({
          resume: existingFile,
        });
        this.fileUploadComponent.writeValue(existingFile); // Bind the file to the child component
      }
    });
  }

  async onSubmit() {
    this.scheduleForm.markAllAsTouched();

    if (this.scheduleForm.valid) {
      this.loading = true;
      var byteString = "";
      const formData = this.scheduleForm.value;
      const file = this.scheduleForm.controls['resume'].value as File;

      if(file) {

      const byteArray = await fileToBase64(file);
      byteString = byteArray.toString();

      }

      const userSession = this.authenticationService.getUserSessionDetails();

      if (userSession != null && userSession != undefined) {
        try {
          this.loggedinUser = userSession.email;
        } catch (error) {
          console.error('Failed to parse user session:', error);
          this.loading = false;
          alert('Could not retrieve user information. Please try again.');
          return; // Exit to avoid further execution
        }
      }

      try {
        const panelsEmailString = this.panels.join(',');

        var interviewSchedule = new panelSlotsCalenderEvent(
          this.data,
          this.getCombinedDateTime(),
          panelsEmailString,
          formData.candidate,
          formData.candidate,
          file == null ? "" : byteString,
          this.loggedinUser,
          file == null ? "" : file.name,
          formData.eventTitle,
          "India Standard Time"
        );

        this.scheduleInterviewData(interviewSchedule);
      } catch (error) {
        console.error('Error processing the file or scheduling:', error);
        alert(
          'An error occurred while processing your request. Please try again.'
        );
        this.loading = false;
      }
    } else {
      this.scheduleForm.markAllAsTouched();

      this.loading = false;
    }
  }

  scheduleInterviewData(data: panelSlotsCalenderEvent) {
    this.panelService.insertScheduleInterviewData(data).subscribe({
      next: (res) => {
        if (res) {
          if (!this.isEditing) {
          this.showSnackbar('Interview is scheduled');
        } else {
          this.showSnackbar('Meeting is updated');
        }
        } else {
          this.showSnackbar('Something went wrong, please connect with administrator');
        }
        this.onClose("true");
      },
      error: (err) => {
        console.error('Error scheduling interview:', err);
        this.showSnackbar(
          'An error occurred while scheduling the interview. Please try again later.'
        );
      },
    });
  }
  showSnackbar(message: string) {
    this.snackBar.open( message, 'Close', {
      duration: 5000, 
      horizontalPosition: 'center', 
      verticalPosition: 'bottom',    
    });
  }


  add(event: MatChipInputEvent): void {
    const input = event.input;
    const value = event.value;

    // Add email only if it's not empty
    if ((value || '').trim()) {
      this.panels.push(value.trim());
      this.panelControl.setValue('');
      this.scheduleForm.get('panels')?.setValue(this.panels);
    }

    // Reset the input value
    if (input) {
      input.value = '';
    }
  }

  remove(email: string): void {
    const index = this.panels.indexOf(email);

    if (index >= 0) {
      this.panels.splice(index, 1);
      this.scheduleForm.get('panels')?.setValue(this.panels);
    }
  }

  // Function to combine date and time into a formatted string
  getCombinedDateTime(): Date {
    const date = this.scheduleForm.get('scheduleDate')?.value; // This will be a Date object
    const time = this.scheduleForm.get('scheduleTime')?.value; // This will be a time string

    // Get individual components from date
    const year = date.getFullYear();
    const month = date.getMonth(); // Month is zero-based
    const day = date.getDate();

    // Split time into hours and minutes
    const [hours, minutes] = time.split(':').map(Number);

    // Create a new Date object from the date and time
    const combinedDateTime = new Date(year, month, day, hours, minutes);

    return combinedDateTime;
  }

  onClose(value:string): void {
    this.dialogRef.close(value);
  }
}

