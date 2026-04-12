// mat-multi-select.component.ts
import { Component, Input, forwardRef, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatSelect, MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxChange, MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common'; // Important for *ngFor
import { Subject, takeUntil, debounceTime } from 'rxjs';
import { NgSelectModule, NgSelectComponent } from '@ng-select/ng-select';

/**
 * A reusable, standalone Angular component for a Material multi-select dropdown with a "Select All" checkbox and search functionality.
 * It implements ControlValueAccessor to integrate seamlessly with Angular's Reactive and Template-driven Forms.
 */
@Component({
  selector: 'app-mat-multi-select',
  templateUrl: './mat-multi-select.component.html',
  styleUrls: ['./mat-multi-select.component.css'],
  standalone: true, // This makes the component standalone
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    NgSelectModule
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MatMultiSelectComponent),
      multi: true
    }
  ]
})
export class MatMultiSelectComponent implements ControlValueAccessor, OnInit, OnDestroy {
  @Input() label: string = '';
  @Input() options: any[] = [];
  @Input() valueKey: string = '';
  @Input() displayKey: string = '';
  @Input() required: boolean = false;
  @Input() name: string = '';
  @Input() placeholder: string = '';
  isFocused!: boolean;

  @ViewChild(NgSelectComponent) ngSelect!: NgSelectComponent;

  selectedValues: any[] = [];
  isDisabled: boolean = false;
  searchControl = new FormControl('');
  private destroy$ = new Subject<void>();

  private onChange: (value: any[]) => void = () => { };
  private onTouched: () => void = () => { };

  constructor() { }

  /**
   * Called when the ng-select dropdown is opened.
   * Ensures the "Select All" checkbox state is updated immediately.
   */
  onOpen(): void {
  }

  ngOnInit(): void {
    // This is where you would handle search logic if it were implemented inside the component.
    // As ng-select has a built-in search, we can use that instead.
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Returns the display text for a given option, handling both objects and simple values.
   */
  getOptionDisplay(option: any): any {
    return this.displayKey && typeof option === 'object' ? option[this.displayKey] : option;
  }

  writeValue(value: any[] | null): void {
    if (value) {
      this.selectedValues = value;
    } else {
      this.selectedValues = [];
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  onModelChange(event: any[]): void {
    // This is the core change: map the selected objects to their values
    // before emitting the change to the parent form.
    const valueToEmit = this.selectedValues.map(item => this.getOptionValue(item));
    this.onChange(valueToEmit);
    this.onTouched();
  }

  toggleSelectAll(event: MatCheckboxChange): void {
    if (event.checked) {
      // Correctly map all options to their values
      this.selectedValues = this.options.map(option => this.getOptionValue(option));
    } else {
      this.selectedValues = [];
    }
    this.onChange(this.selectedValues);
    this.onTouched();
  }

  get isSelectAllChecked () {
    return  this.selectedValues && this.options.length > 0  && this.selectedValues.length === this.options.length;
  }

  onSelectAllClick(event: Event): void {
    event.stopPropagation();
    this.ngSelect.close();
  }

  isItemSelected(item: any): boolean {
    const itemValue = this.getOptionValue(item);
    return this.selectedValues?.includes(itemValue);
  }

  selectAll() {
    this.selectedValues = this.options.map((x) => this.getOptionValue(x));
  }

  unselectAll() {
    this.selectedValues = [];
  }

  /**
   * This is the function required by ng-select to compare the model value
   * with the options list. It ensures the component can correctly identify
   * which options are selected, silencing the console warning.
   */
  compareOptions = (item: any, selected: any): boolean => {
    if (!item || !selected) {
      return false;
    }
    // Compare the item from the options list to the selected value from the model.
    // The selected value is a primitive (string/number) based on the valueKey.
    return this.getOptionValue(item) === selected;
  }

  /**
   * Returns the value for a given option, handling both objects and simple values.
   */
  getOptionValue(option: any): any {
    return this.valueKey && typeof option === 'object' ? option[this.valueKey] : option;
  }
}