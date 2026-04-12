import { DataSource } from '@angular/cdk/collections';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { BehaviorSubject, Observable } from 'rxjs';
import { of as observableOf, merge } from 'rxjs';
import {map} from 'rxjs/operators'
import { PanelGrid } from './panel-grid.model';

export class DataTableDataSource extends DataSource<PanelGrid> {
    data: PanelGrid[] = [];
    paginator: MatPaginator | undefined;
    sort: MatSort | undefined;
    get filter(): string { return this.filter$.getValue(); }
    set filter(value: string) { this.filter$.next(value); }
    filter$!: BehaviorSubject<string>;
  
    constructor(public pannelGridList : PanelGrid[]) {
      super();
      this.filter$ = new BehaviorSubject(" ");
      this.data = pannelGridList;
    }
    

    connect(): Observable<PanelGrid[]> {
      if (!this.paginator || !this.sort) {
        return observableOf(this.data);
      }
      
      // Combine everything that affects the rendered data into one update
      // stream for the data-table to consume.
      return merge(
        observableOf(this.data), 
        this.paginator.page, 
        this.sort.sortChange,
        this.filter$)
        .pipe(map(() => {
          return this.getFilteredData(this.getPagedData(this.getSortedData([...this.data ])));
        }));
    }

    disconnect(): void {}

    private getFilteredData(data: PanelGrid[]) {
      if(this.filter !== undefined && this.filter !== null && this.filter !== " "){
      return data.filter(d => d.panelName !== undefined && d.panelName !== null && d.panelName.toLowerCase().includes(this.filter));
      }
      else{
        return data;
      }
    }
  
    private getPagedData(data: PanelGrid[]): PanelGrid[] {
      if (this.paginator) {
        const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
        //var slicData = data.splice(startIndex, this.paginator.pageSize);
        return data;
      } else {
        return data;
      }
    }
  
    private getSortedData(data: PanelGrid[]): PanelGrid[] {
      if (!this.sort || !this.sort.active || this.sort.direction === '') {
        return data;
      }
  
      return data.sort((a, b) => {
        const isAsc = this.sort?.direction === 'asc';
        switch (this.sort?.active) {
          case 'emailId' : return compare(a.emailId,b.emailId,isAsc);
          case 'panelName': return compare(a.panelName, b.panelName, isAsc);
          case 'panelType': return compare(a.panelType, b.panelType, isAsc);
          case 'seniorityName': return compare(a.seniorityName, b.seniorityName, isAsc);
          case 'communityName': return compare(a.communityName, b.communityName, isAsc);
          case 'slotCount': return compare(a.slotCount, b.slotCount, isAsc);
          case 'nonUtilizedSlot':return compare(a.nonUtilizedSlot,b.nonUtilizedSlot,isAsc);
          case 'deficit':return compare(a.deficit,b.deficit,isAsc);
          case 'quater':return compare(a.quater,b.quater,isAsc);
          default: return 0;
        }
      });
    }
    
  }
  
  function compare(a: string | number, b: string | number, isAsc: boolean): number {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
  }