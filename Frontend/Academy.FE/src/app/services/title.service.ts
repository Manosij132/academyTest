import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TitleService {

  constructor() { }
  private titleSubject = new Subject<string>();
  title$ = this.titleSubject.asObservable();

  set(title: string) {
    this.titleSubject.next(title);
  }
}
