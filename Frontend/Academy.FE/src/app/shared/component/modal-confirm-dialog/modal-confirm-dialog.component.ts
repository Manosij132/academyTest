import { Component, inject, OnInit } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import {
  MatDialogActions,
  MatDialogClose,
  MatDialogTitle,
  MatDialogContent,
  MatDialogRef,
  MAT_DIALOG_DATA,
} from "@angular/material/dialog";

@Component({
  standalone: true,
  selector: "app-modal-confirm-dialog",
  templateUrl: "./modal-confirm-dialog.component.html",
  styleUrls: ["./modal-confirm-dialog.component.css"],
  imports: [
    MatButtonModule,
    MatDialogActions,
    MatDialogClose,
    MatDialogTitle,
    MatDialogContent,
  ],
})
export class ModalConfirmDialogComponent implements OnInit {
  readonly dialogRef = inject(MatDialogRef<ModalConfirmDialogComponent>);
  data = inject(MAT_DIALOG_DATA);
  dialogResult: any = {
    confirm: true,
  };
  constructor() {}

  ngOnInit() {}

  onCancel() {
    this.dialogResult.confirm = false;
    this.dialogRef.close();
  }
}
