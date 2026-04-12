import { Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
@Injectable({ providedIn: 'root' })
export class AutocompleteService {
  setupFilter<T>(
    control: FormControl,
    options: T[],
    displayField: keyof T
  ): Observable<T[]> {
    return control.valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value, options, displayField))
    );
  }
  private _filter<T>(value: string | T, options: T[], displayField: keyof T): T[] {
    const filterValue =
      typeof value === 'string'
        ? value.toLowerCase()
        : String(value?.[displayField] ?? '').toLowerCase();
    return options.filter(option =>
      String(option[displayField]).toLowerCase().includes(filterValue)
    );
  }
}