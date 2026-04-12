import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from "@angular/material/button";
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UpdateTicketDto } from '@shared/Interface/ticket.model';
import { MatOptionModule } from '@angular/material/core';

@Component({
  selector: 'app-ticket-edit-dialog',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule
  ],
  templateUrl: './ticket-edit-dialog.component.html',
  styleUrls: ['./ticket-edit-dialog.component.scss']
})
export class TicketEditDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<TicketEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.form = this.fb.group({
      detailedStatus: [data.detailedStatus || ''],
      monthClosure: [data.monthClosure || ''],
      ticketStatus: [data.ticketStatus || ''],
      comments: [data.comments || '']
    });
  }

  save() {
    const updated: UpdateTicketDto = this.form.value;
    this.dialogRef.close(updated);
  }

  close() {
    this.dialogRef.close();
  }
}
