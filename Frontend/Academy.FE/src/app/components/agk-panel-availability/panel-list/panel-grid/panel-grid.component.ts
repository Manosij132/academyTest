import { Component,OnInit,Input, ViewChild, AfterViewInit, OnChanges, SimpleChanges,Output,EventEmitter, ChangeDetectorRef} from "@angular/core";
import { PanelGrid } from "../../model/panel-grid.model";
import { MatTable, MatTableModule } from "@angular/material/table";
import { DataTableDataSource } from "../../model/dataTableDataSource.datasource";
import { MatPaginator, MatPaginatorModule, PageEvent } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { Pagination } from "../../model/pagination.model";
import { MatDialog } from '@angular/material/dialog';
import { PanelSlotCalendarPopupComponent } from "../panel-slot-calendar-popup/panel-slot-calendar-popup.component";
import { PanelSendEmailPopupComponent } from "../panel-send-email-popup/panel-send-email-popup.component";
import { SendEmailModel } from "../../model/send-email.model";
import { CommonModule } from "@angular/common";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatIconModule } from "@angular/material/icon";

@Component({
    selector: 'mf-app-panel-grid',
    templateUrl: './panel-grid.component.html',
    styleUrls: ['./panel-grid.component.css'],
    standalone: true,
    imports: [
      CommonModule,
      FormsModule,
      ReactiveFormsModule,
      MatCheckboxModule,
      MatPaginatorModule,
      MatIconModule,
      MatTableModule,
      MatPaginatorModule
    ]
  })

  export class PanelGridComponent implements OnInit,AfterViewInit,OnChanges {
    
    @Input() panels : PanelGrid[] = [];
    @Input() pageNumber : number = 0;
    showPageNumber : number = 0;
    @Input() pageSize : number = 0;
    @Input() totalRecords : number = 0;
    // @ViewChild(MatTable) table! : MatTable<PanelGrid>;
    @ViewChild(MatPaginator, {static:false}) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;
    displayedColumns = [ 'select','panelName','panelType','seniorityName','communityName', 'requiredSlots','slotCount','nonUtilizedSlot','deficit','panelAction'];
    // tableDataSource!: DataTableDataSource;
    @Output() onPaginationChanged : EventEmitter<Pagination> = new EventEmitter<Pagination>();
    isAllSendMail: boolean = false;
    multipleCheckedCount: number = 0;
    @Input() calendarStartDate = new Date();

    sendEmailModel: SendEmailModel = {
      fromEmail: '',
      globerEmail: '',
      globerLeaderEmail: '',
      subject: '',
      body: '',
      communityGKFocalEmailId: ''
    };

    sendSingleEmailModel: SendEmailModel = {
      fromEmail: '',
      globerEmail: '',
      globerLeaderEmail: '',
      subject: '',
      body: '',
      communityGKFocalEmailId: ''
    };

    constructor(public dialog: MatDialog, private changeDetectorRefs: ChangeDetectorRef){

    }

    openCalendarDialog(panelId: number, panelName: string): void {
      const dialogRef = this.dialog.open(PanelSlotCalendarPopupComponent, {
        data: { panelId: panelId, startDate : this.calendarStartDate, panelName: panelName}, width: '800px', 
      });
    }

    openSendEmailDialog(panel: any): void {
      this.sendSingleEmailModel.fromEmail = '';
      this.sendSingleEmailModel.globerEmail = panel.emailId;
      this.sendSingleEmailModel.globerLeaderEmail = panel.globantLeaderEmailId;
      this.sendSingleEmailModel.communityGKFocalEmailId = panel.communityGKFocalEmailId;
      const dialogRef = this.dialog.open(PanelSendEmailPopupComponent, {
        
        data: { panel: this.sendSingleEmailModel}, width: '700px', height: '75%'
      });
       dialogRef.afterClosed().subscribe(result => {
      });
    }

    openAllSendEmailDialog(): void{
    const dialogRef = this.dialog.open(PanelSendEmailPopupComponent, {
      data: { panel: this.sendEmailModel}, width: '700px', height: '100%'
    });
  }
    ngOnInit(): void {
    }

    rowChecked(checked: boolean, row: any)
    {
      if(checked == true) {
        this.multipleCheckedCount = this.multipleCheckedCount + 1;
        this.sendEmailModel.globerEmail = this.sendEmailModel.globerEmail? this.sendEmailModel.globerEmail + ', ' + row.emailId: row.emailId;
        this.sendEmailModel.globerLeaderEmail = this.sendEmailModel.globerLeaderEmail? this.sendEmailModel.globerLeaderEmail + ', ' + row.globantLeaderEmailId: row.globantLeaderEmailId;
        if(this.sendEmailModel.globerLeaderEmail.indexOf(row.communityGKFocalEmailId) == -1)
        {
          this.sendEmailModel.globerLeaderEmail = this.sendEmailModel.globerLeaderEmail + ", " + row.communityGKFocalEmailId;
        }
      }
      else
      {
        this.multipleCheckedCount = this.multipleCheckedCount - 1;
        this.sendEmailModel.globerEmail = this.sendEmailModel.globerEmail.replace(', ' + row.emailId, '');
        this.sendEmailModel.globerLeaderEmail = this.sendEmailModel.globerLeaderEmail.replace(', ' + row.globantLeaderEmailId,'');
      }
     
      if(this.multipleCheckedCount > 1){
        this.isAllSendMail = true;
      }
      else{
        this.isAllSendMail = false;
      }
    }

    checkedAll(checked: boolean){
        this.panels.forEach(t => (t.checked = checked));
        this.isAllSendMail = checked;
        if(checked == true)
          {
            this.sendEmailModel.globerEmail = this.panels.map(x => x.emailId).join(', ');
            this.sendEmailModel.globerLeaderEmail = [...new Set(this.panels.map(item => item.globantLeaderEmailId))].join(', ');
            let communityGKFocalEmailIds = [...new Set(this.panels.map(item => item.communityGKFocalEmailId).filter(x => x))];
            this.sendEmailModel.globerLeaderEmail = this.sendEmailModel.globerLeaderEmail + (communityGKFocalEmailIds.length > 0 ?  ", " + communityGKFocalEmailIds : "");
          }
          else{
            this.sendEmailModel.globerEmail = '';
            this.sendEmailModel.globerLeaderEmail = ''
            this.multipleCheckedCount = 0;
          }
    }

    ngAfterViewInit(): void {
      //this.tableDataSource = new DataTableDataSource(this.panels);
      if(this.paginator !== undefined && this.paginator !== null){
        if(this.pageNumber > 1){
          this.showPageNumber = this.pageNumber - 1;
        }
        this.paginator.pageIndex = this.showPageNumber;
        this.paginator.length = this.totalRecords;
        //this.tableDataSource.paginator = this.paginator;
      }
      if(this.sort !== undefined && this.sort !== null){
        //this.tableDataSource.sort = this.sort;
      }
      // if(this.table !== undefined && this.table !== null){
      //   this.table.dataSource = this.tableDataSource;
      // }
    }

    ngOnChanges(changes: SimpleChanges): void {
      // if (this.tableDataSource) {
      //   this.tableDataSource.data = this.panels;
      // }
      // if(this.table !== undefined && this.table !== null){
      //   this.table.dataSource = this.tableDataSource;
      // }
      this.changeDetectorRefs.detectChanges();
    }

    pageChanged(event: PageEvent){
      if(event !== undefined && event !== null){
        let pagination : Pagination = new Pagination();
        pagination.pageIndex = event.pageIndex;
        pagination.pageSize = event.pageSize;
        this.onPaginationChanged.emit(pagination);
        if(this.isAllSendMail == true)
          {
            this.isAllSendMail = false;
          }
      }
    }
  }

