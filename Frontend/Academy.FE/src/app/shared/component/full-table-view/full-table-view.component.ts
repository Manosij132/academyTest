import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableViewService } from '../../../services/table-view.service';

@Component({
  selector: 'app-full-table-view',
  templateUrl: './full-table-view.component.html',
  styleUrls:['./full-table-view.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class FullTableViewComponent implements OnInit {
  tableData: any[] = [];
  columns: string[] = [];

  constructor(private tableViewService: TableViewService

  ) {}

  ngOnInit() {
    this.tableData = this.tableViewService.getTableData();
    const storedData = sessionStorage.getItem('fullTableData');    
    
      if (storedData) {
              this.tableData = JSON.parse(storedData);
          this.columns = Object.keys(this.tableData[0]);

          // Need this for academy data as it is double json stringified
              if ( this.columns[0].toString() === "0") {
                  this.tableData = JSON.parse(JSON.parse(storedData));
                  this.columns = Object.keys(this.tableData[0]);
                }
    }
  }
}
