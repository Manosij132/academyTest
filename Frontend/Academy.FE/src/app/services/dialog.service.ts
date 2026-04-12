import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { ModalDialogComponent } from "../shared/component/modal-dialog/modal-dialog.component";
import { ModalConfirmDialogComponent } from "../shared/component/modal-confirm-dialog/modal-confirm-dialog.component";

// Interface for the dialog configuration
export interface DialogData {
  component: any; // The component to be displayed in the dialog
  componentProps?: any; // Input properties for the dynamic component
  title?: string; // Optional title for the dialog
  panelClass?: string; // Optional panel class for styling
}

@Injectable({
  providedIn: "root",
})
export class DialogService {
  // dialog = Inject(MatDialog);

  constructor(private dialog: MatDialog) {}

  openDialog(data: DialogData): MatDialogRef<ModalDialogComponent> {
    return this.dialog.open(ModalDialogComponent, {
      data: data,
      panelClass: data.panelClass || "custom-dialog", // Use default or passed panel class
      width: "80%", // or a specific width like '500px'
      // other MatDialogConfig options can be added here
    });
  }

  openConfirmDialog(data: DialogData): MatDialogRef<ModalConfirmDialogComponent> {
    return this.dialog.open(ModalConfirmDialogComponent, {
      data: data,
      panelClass: "custom-dialog", // Use default or passed panel class
      width: "20%", // or a specific width like '500px'
      // other MatDialogConfig options can be added here
    });
  }
}
