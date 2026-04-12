import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TableViewService {
  private tableData: any = null;

  setTableData(data: any) {
    this.tableData = data;
  }

  getTableData() {
    return this.tableData;
  }
}