import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './table.component.html',
  styleUrl: './table.component.css'
})
export class TableComponent {

  @Input() headers: string[] = []; 
  @Input() data: any[] = []; 
  @Input() keys: string[] = [];
  @Input() routes: any;
  @Output() rowClicked = new EventEmitter<number>();

  openUrl(key: string): void {
    this.router.navigate([this.routes.get(key)]);
  }

  constructor(private router: Router) { }
  ngOnInit() {
    this.sortDataByScheduledDate();
  }
  
  rowClick(event:any){
this.rowClicked.emit(event);
  }

  sortDataByScheduledDate(): void {

    this.data.sort((a, b) => {
      const dateA = new Date(this.formatDate(a.scheduledDate));  
      const dateB = new Date(this.formatDate(b.scheduledDate));  

      return dateB.getTime() - dateA.getTime(); 
    });
  }

  
  formatDate(dateString: string): string {
    const [day, month, year, hour] = dateString.split(' ');
    return `${year}-${month}-${day}T${hour}:00:00`; 
  }
}
