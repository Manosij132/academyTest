import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommunitySelectionRatio } from '../../model/community-selection-ratio.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'mf-app-editslotmodel',
  templateUrl: './edit-slot-model.component.html',
  styleUrls: ['./edit-slot-model.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
    ]
})
export class EditSlotmodelComponent implements OnInit {

  positionToBeFilled: number | undefined;
  dropRatio: number | undefined;
  offersToBeRolledOut: number | undefined;
  l1SelectionRatio: number | undefined;
  gkSelectionRatio: number | undefined;

  communitySelectionRatio: CommunitySelectionRatio = {
    tdc:"",
    communityId:0,
    startDate: new Date,
    endDate: new Date,
    l1SelectionRatio: 0,
    gkSelectionRatio: 0
 };

 l1Visible: boolean = false;
 gkVisible: boolean = false;

  constructor(public dialogRef: MatDialogRef<EditSlotmodelComponent>,
    @Inject(MAT_DIALOG_DATA)  public data: any
  ) { }

  save(): void {
    // Optionally, you can return the input values when the dialog is closed
    const result = {
      positionToBeFilled: this.positionToBeFilled,
      dropRatio: this.dropRatio,
      offersToBeRolledOut: this.offersToBeRolledOut,
      l1SelectionRatio: this.l1SelectionRatio,
      gkSelectionRatio: this.gkSelectionRatio
    };
    this.dialogRef.close(result);
  }

  close(): void{
    this.dialogRef.close();
  }

  ngOnInit(): void {
    
    this.positionToBeFilled = this.data.seniority.positionToBeFilled;
    this.dropRatio = this.data.seniority.dropRatio;
    this.offersToBeRolledOut = this.data.seniority.offersToBeRolledOut;
    if(this.data.seniority.l1SelectionRatio != undefined && this.data.seniority.l1SelectionRatio != null)
      {
        this.l1SelectionRatio = this.data.seniority.l1SelectionRatio;
      }
      else
      {
        this.l1SelectionRatio = this.data.selectionRatio.l1SelectionRatio;
      }
      if(this.data.seniority.gkSelectionRatio != undefined && this.data.seniority.gkSelectionRatio != null)
        {
          this.gkSelectionRatio = this.data.seniority.gkSelectionRatio;
        }
        else
        {
          this.gkSelectionRatio = this.data.selectionRatio.gkSelectionRatio;
        }

    this.l1Visible = this.data.isL1Visible;
    this.gkVisible = this.data.isGKVisible;
    
    this.communitySelectionRatio.l1SelectionRatio = this.data.selectionRatio.l1SelectionRatio;
    this.communitySelectionRatio.gkSelectionRatio = this.data.selectionRatio.gkSelectionRatio;

  }
}
