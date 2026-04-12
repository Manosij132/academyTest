import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { DatePipe } from '@angular/common';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DATE_FORMATS, MAT_NATIVE_DATE_FORMATS, provideNativeDateAdapter } from '@angular/material/core';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ToastrService } from 'ngx-toastr';
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { Profile, ProfileService } from '@services/profile.service';
import { InterviewsService } from '@services/interviews.service';
import { finalize, map, Observable, of, startWith } from 'rxjs';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { LoaderService } from '@services/loader.service';
import { FlatpickrDirective, provideFlatpickrDefaults } from 'angularx-flatpickr';
import { DataService, FitmentType } from "@services/data.service";
import { AutocompleteService } from '@services/autocomplete.service';

@Component({
  selector: 'app-schedule-mock-interview-dialog',
  standalone: true,
  imports: [
    CommonModule, MatFormFieldModule, MatSelectModule, FormsModule, ReactiveFormsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatAutocompleteModule,
    MatSnackBarModule,
    FlatpickrDirective
  ],
  providers: [
    provideNativeDateAdapter(),
    { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS },
    DatePipe,
    provideFlatpickrDefaults({
      enableTime: true,
      dateFormat: 'Y-m-d H:i',
      minDate: 'today'
    })
  ],
  templateUrl: './schedule-mock-interview-dialog.component.html',
  styleUrl: './schedule-mock-interview-dialog.component.css'

})
export class ScheduleMockInterviewDialogComponent {

  profiles: Profile[] = [];
  sectionStatus: any[] = [];
  interviewCode: string = '';
  interviewForm!: FormGroup;
  filteredProfiles!: Observable<Profile[]>;
  searchControl = new FormControl<Profile | string>('');
  isScheduleDisabled: boolean = true;
  fitmentType: FitmentType[] = [];
  scheduleError: string | null = null;
  constructor(
    private profileService: ProfileService,
    private interviewsService: InterviewsService,
    private toastr: ToastrService,
    public dialogRef: MatDialogRef<ScheduleMockInterviewDialogComponent>,
    private loaderService: LoaderService,
    private autoCompleteService: AutocompleteService,
    private dataService: DataService,
    @Inject(MAT_DIALOG_DATA) public data: { employeeId: number, name: string, email: string },
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.fetchProfiles();
    this.fetchFitmentType();
  }

  initForm(): void {
    this.interviewForm = new FormGroup({
      interviewType: new FormControl(null, Validators.required),
      profile: this.searchControl, // Connect autocomplete control directly
      interviewDateTime: new FormControl(null, Validators.required),
      ccEmail: new FormControl(null)
    });

    this.searchControl.valueChanges.subscribe((profile) => {
      if (typeof profile === 'object' && profile !== null) {
        this.isScheduleDisabled = false;
        this.populateSectionStatus(profile);
      } else {
        this.isScheduleDisabled = true;
        this.sectionStatus = [];
      }
    });
  }

  fetchFitmentType() {
    this.loaderService.start();
    this.dataService.getAllFitmentTypes().subscribe({
      next: (data) => {
        this.fitmentType = data;
        this.loaderService.stop();
      },
      error: (err) => {
        this.loaderService.stop();
      },
    });
  }

  fetchProfiles(): void {
    this.profileService.getAll().subscribe((profiles) => {
      this.profiles = profiles;

      this.filteredProfiles = this.searchControl.valueChanges.pipe(
        startWith(''),
        map(value => typeof value === 'string' ? value : value?.profileName ?? ''),
        map(name => this.filterProfiles(name))
      );
    });
  }
  private filterProfiles(name: string): Profile[] {
    const filterValue = name.toLowerCase();
    return this.profiles.filter(profile =>
      profile.profileName.toLowerCase().includes(filterValue)
    );
  }

  displayFn(profile: Profile): string {
    return profile?.profileName || '';
  }

  generateInterviewCode(): void {
    if (isPlatformBrowser(this.platformId)) {
      const arr = new Uint16Array(4);
      window.crypto.getRandomValues(arr);
      this.interviewCode = Array.from(arr, num => (100 + (num % 900))).join('-');
    }
  }

  populateSectionStatus(profile: Profile): void {
    this.sectionStatus = [];

    const skillsAndSections = profile.skillsAndSections || [];

    for (const skill of skillsAndSections) {
      const skillId = skill.skillId;

      for (const section of skill.sections || []) {
        this.sectionStatus.push({
          skill: skillId,
          section: section.name,
          subsections: []
        });
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSchedule(): void {
    if (this.interviewForm.valid) {
      this.scheduleError = null;
      this.isScheduleDisabled = true;
      this.generateInterviewCode();
      const formValue = this.interviewForm.value;
      const date = new Date(formValue.interviewDateTime);
      const formattedDate =
        date.getFullYear() + "-" +
        String(date.getMonth() + 1).padStart(2, "0") + "-" +
        String(date.getDate()).padStart(2, "0") + "T" +
        String(date.getHours()).padStart(2, "0") + ":" +
        String(date.getMinutes()).padStart(2, "0") + ":" +
        String(date.getSeconds()).padStart(2, "0") +
        ".000Z";
      const interview = {
        scheduleDateTime: formattedDate,
        profileId: formValue.profile?.profileId,
        candidateId: this.data.employeeId,
        candidate: { id: this.data.employeeId, name: this.data.name, email: this.data.email },
        ccEmailIds: formValue.ccEmail,
      };

      this.loaderService.start();
      this.interviewsService.create(interview)
        .pipe(finalize(() => {
          this.isScheduleDisabled = false;
          this.loaderService.stop()
        }))
        .subscribe({
          next: (res) => {
            this.toastr.success('Evaluation scheduled successfully!', 'Success');
            this.dialogRef.close(interview);
          },
          error: (err) => {
            console.error('Error while scheduling evaluation:', err);
            const status = err?.status;
            const msg = err?.error?.message || err?.message || '';
            if (status === 409 || msg.toLowerCase().includes('already scheduled') || msg.toLowerCase().includes('duplicate')) {
              // Toastr toast is already shown by the global API interceptor (top-right)
              this.scheduleError = 'An evaluation is already scheduled for this candidate under the selected profile. Please choose a different profile.';
            } else {
              // Toastr toast is already shown by the global API interceptor
              this.scheduleError = 'Failed to schedule evaluation. Please try again.';
            }
          }
        });
    } else {
      console.warn('Evaluation form is invalid:', this.interviewForm.errors);
    }
  }

  onInterviewTypeChange(val: any): void {
    const selectedFitment = this.fitmentType.find(f => f.id === val);
    if (!selectedFitment) {
      this.filteredProfiles = of([]);
      return;
    }
    const fitmentPrefix = selectedFitment.name.replace(/\s/g, '');
    const filtered = this.profiles.filter(profile => {
      if (!profile.profileName) return false;
      const prefix = profile.profileName.split('_')[0];
      return prefix === fitmentPrefix;
    });
    this.filteredProfiles = this.autoCompleteService.setupFilter(
      this.searchControl,
      filtered,
      'profileName'
    );
    // Reset selected profile
    this.searchControl.reset();
  }
}

