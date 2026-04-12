import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'mf-app-panel-efficiency-evaluation',
  templateUrl: './panel-efficiency-evaluation.component.html',
  styleUrls: ['./panel-efficiency-evaluation.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,]
})
export class PanelEfficiencyEvaluationComponent implements OnInit {
panelEfficiencyList: any;
avgRating: number=0;
  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {
    this.panelEfficiencyList=data.Data;
  }
ngOnInit() {
  if(this.panelEfficiencyList && this.panelEfficiencyList.length>0)
  {
  const sum = this.panelEfficiencyList.reduce((acc: number, item: any) => acc + item.finalRatingInNumber, 0);
  this.avgRating= sum / this.panelEfficiencyList.length;
  }
}
}
