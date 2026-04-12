import { Component, Inject, Input, OnChanges, OnInit } from '@angular/core';
import { Calendar, CalendarOptions } from '@fullcalendar/core'; // useful for typechecking
import dayGridPlugin from '@fullcalendar/daygrid';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { PanelService } from '@services/panel.service';
import { PanelSlotDetailModel } from '../../model/panel-slot-detail.model';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'mf-app-panel-slot-calendar-popup',
  templateUrl: './panel-slot-calendar-popup.component.html',
  styleUrls: ['./panel-slot-calendar-popup.component.css'],
  standalone: true,
  imports:[
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule
  ]
})
export class PanelSlotCalendarPopupComponent implements OnInit, OnChanges {

  calendarOptions: CalendarOptions = {
    initialView: 'dayGridMonth',
    plugins: [dayGridPlugin],
    events: [],
    contentHeight: 'auto',
    headerToolbar: {
      left: 'prev,next',
      center: 'title',
      right: 'dayGridWeek,dayGridDay' // user can switch between the two
    },
  };

   panelSlotDetail?: PanelSlotDetailModel[] = [];

  constructor(private panelService:PanelService, private dialogRef: MatDialogRef<PanelSlotCalendarPopupComponent>, @Inject(MAT_DIALOG_DATA)  public data: any) { }

  ngAfterViewInit(): void {
  }
  ngOnInit(): void {
    this.getPanelSlotDetailData(this.data.panelId, this.data.startDate);
  }
  ngOnChanges(): void {
  }

  getPanelSlotDetailData(panelId: number, startDate: Date){
    this.panelService.getPanelSlotDetail(panelId).subscribe((res) => {
      if(res !== undefined && res !== null){
        this.panelSlotDetail = res;
        this.calendarOptions = {
          initialDate: startDate,
          initialView: 'dayGridMonth',
          plugins: [dayGridPlugin],
          events: this.panelSlotDetail,
          contentHeight: 40,
        };
        window.setTimeout(() => {
          var calendarEl = document.getElementById('calendar') as HTMLElement;
          var calendar = new Calendar(calendarEl,this.calendarOptions);
          calendar.setOption('height', 400);
          calendar.render();
        }, 75);
      }
    })
  }

}

