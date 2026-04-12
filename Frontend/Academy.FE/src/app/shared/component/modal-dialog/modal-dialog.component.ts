import { CommonModule } from "@angular/common";
import { Component, inject, OnInit } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MAT_DIALOG_DATA, MatDialogModule } from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import { MatTableModule } from "@angular/material/table";

@Component({
  selector: "app-modal-dialog",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
  ],
  templateUrl: "./modal-dialog.component.html",
  styleUrls: ["./modal-dialog.component.css"],
})
export class ModalDialogComponent implements OnInit {
  data = inject(MAT_DIALOG_DATA);
  constructor() {}

  ngOnInit() {}

  closeCommentsPopup() {}

  onClose(){
    
  }
}
