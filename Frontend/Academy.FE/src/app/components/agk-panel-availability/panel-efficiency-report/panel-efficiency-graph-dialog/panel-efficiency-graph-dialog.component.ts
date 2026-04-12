import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { NgxChartsModule } from '@swimlane/ngx-charts';

@Component({
  selector: 'mf-app-panel-efficiency-graph-dialog',
  templateUrl: './panel-efficiency-graph-dialog.component.html',
  styleUrls: ['./panel-efficiency-graph-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    NgxChartsModule,
    MatDialogModule,
    NgxChartsModule
  ]
})
export class PanelEfficiencyGraphDialogComponent {
  colorScheme = {
    domain: ['#5AA454', '#A10A28', '#C7B42C', '#AAAAAA']
  };

  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {
    console.log('Dialog Data:', data); // Log to verify data
  }

}
