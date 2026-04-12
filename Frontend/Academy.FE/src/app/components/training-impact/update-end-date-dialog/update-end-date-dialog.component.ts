import { CommonModule } from "@angular/common";
import { Component, Inject, OnInit } from "@angular/core";
import {
  AbstractControl, FormBuilder, FormGroup, FormsModule,
  ReactiveFormsModule, Validators
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatNativeDateModule } from "@angular/material/core";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatTableModule } from "@angular/material/table";

@Component({
  selector: "app-update-end-date-dialog",
  templateUrl: "./update-end-date-dialog.component.html",
  styleUrls: ["./update-end-date-dialog.component.css"],
  standalone: true,
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
    ReactiveFormsModule,
    MatCheckboxModule,
  ],
})
export class UpdateEndDateDialogComponent implements OnInit {
  newActivity = {
    endDate: "",
  };
  color = "#eeeeee";
  form!: FormGroup;
  minDate = new Date();
  constructor(
    public dialogRef: MatDialogRef<UpdateEndDateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private formBuilder: FormBuilder
  ) { }

  ngOnInit() {
    this.form = this.formBuilder.group({
      endDate: [this.newActivity.endDate, [Validators.required]],
    });
  }

  onSubmit() {
    if (this.form.valid) {
      console.log(this.form.value);
      this.dialogRef.close(this.form.value);
      this.form.reset();
    } else {
      this.form.markAllAsTouched();
    }
  }

  // --- The key part: A getter to expose form controls ---
  get formControls(): { [key: string]: AbstractControl } {
    return this.form.controls;
  }
}
