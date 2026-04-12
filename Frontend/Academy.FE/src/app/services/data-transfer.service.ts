import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataTransferService {

  private employee = new BehaviorSubject<any>(null);
  private dashboard = new BehaviorSubject<any>(null);
  employee$ = this.employee.asObservable();
  dashboard$ = this.dashboard.asObservable();
  employeeData = []
  updateEmployee(data: any) {
    this.employee.next(data);
  }
  updateDashboard(data: any) {
    this.dashboard.next(data);
  }
}
