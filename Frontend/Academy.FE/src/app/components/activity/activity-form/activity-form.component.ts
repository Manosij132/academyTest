import { Component, EventEmitter, Input, Output } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
} from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: "app-activity-form",
  standalone: true,
  templateUrl: "./activity-form.component.html",
  styleUrls: ["./activity-form.component.css"],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule 
  ],
})
export class ActivityFormComponent {
  @Input() activityOptions: any[] = [];
  @Input() accountDetailsOptions: any[] = [];
  @Input() mode: "single" | "bulk" = "single";
  @Input() initialData: any = null;

  @Output() submitForm = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;

  allowedActivityNames: string[] = [
    "Upskilling - Globant University Academy",
    "Self Paced Training",
    "Business Oriented Academy",
    "Reskilling",
  ];

  activitySourceOptions = [
    { id: 1, name: "Globant University" },
    { id: 2, name: "Udemy" },
    { id: 3, name: "Other" },
  ];

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  this.form = this.fb.group(
    {
      activityId: ["", Validators.required],
      activityName: [""],
      activitySource: [{ value: "", disabled: true }],
      activityDetail: ["", Validators.required],
      comments: [""],
      startDate: [today, Validators.required],
      endDate: [null, Validators.required],
      status: ["Pending"],
      account: [[]],
      confirmDelete: [false],
    },
    { validators: this.dateRangeValidator }
  );

  // ✅ PREFILL DATA (EDIT MODE)
  if (this.initialData) {
    this.form.patchValue({
      activityId: this.initialData.activityId,
      activityName: this.initialData.activityName,
      activitySource: this.initialData.activitySource,
      activityDetail: this.initialData.activityDetail,
      comments: this.initialData.comments,
      startDate: this.initialData.startDate,
      endDate: this.initialData.endDate,
      status: this.initialData.status,
      account: this.parseAccount(this.initialData.account)
    });

    // enable source if needed
    this.onActivitySelect(this.initialData.activityId);

    this.form.get('activityId')?.disable();
    this.form.get('activitySource')?.disable();
    this.form.get('activityDetail')?.disable();
  }

  this.form.get("activityId")?.valueChanges.subscribe((id) => {
    this.onActivitySelect(id);
  });

  this.form.get("activitySource")?.valueChanges.subscribe((source) => {
    const detailControl = this.form.get("activityDetail");

    if (source === "Globant University") {
      detailControl?.setValue(
        "https://university.globant.com/group/",
        { emitEvent: false }
      );
    }
  });
}

onDelete() {
  if (!this.initialData) return;

  //Clone original object 
  const payload = {
    ...this.initialData,
    action: 'delete'
  };

  this.submitForm.emit(payload);
}

parseAccount(account: any): string[] {
  if (!account) return [];

  // Already array
  if (Array.isArray(account)) {
    return account
      .map(x => x?.trim())
      .filter(x => x);
  }

  if (typeof account === 'string') {
    let values: string[];

    if (account.includes('#|')) {
      // Priority separator
      values = account.split('#|');
    } else {
      // fallback
      values = account.split(',');
    }

    return values
      .map(x => x.trim())
      .filter(x => x);
  }

  return [];
}

  onActivitySelect(selectedId: number) {
    const selected = this.activityOptions.find((o) => o.id === selectedId);
    const sourceControl = this.form.get("activitySource");

    if (!selected) return;

    this.form.patchValue({ activityName: selected.name });

    const isAllowed = this.allowedActivityNames.includes(selected.name);

    if (isAllowed) {
      sourceControl?.enable();
    } else {
      sourceControl?.disable();
      sourceControl?.setValue("");
    }
  }

  dateRangeValidator(group: FormGroup) {
    const start = group.get("startDate")?.value;
    const end = group.get("endDate")?.value;

    if (start && end && new Date(end) < new Date(start)) {
      return { dateInvalid: true };
    }
    return null;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitForm.emit(this.form.getRawValue());
  }

  onCancel() {
    this.cancel.emit();
  }

  get formControls(): { [key: string]: AbstractControl } {
    return this.form.controls;
  }
}