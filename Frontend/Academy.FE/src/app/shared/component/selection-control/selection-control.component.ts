import { CommonModule } from "@angular/common";
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import { MatChipsModule } from "@angular/material/chips";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatRadioModule } from "@angular/material/radio";
import { MatSelectModule } from "@angular/material/select";

export interface SelectionOption {
  value: any; // Allow any type for the value
  viewValue: string;
  disabled?: boolean;
}

export type ControlType = "radio" | "select" | "button-group" | "chips";

interface StatusClasses {
  [status: string]: string;
}

@Component({
  standalone: true,
  selector: "app-selection-control",
  templateUrl: "./selection-control.component.html",
  styleUrls: ["./selection-control.component.css"],
  imports: [
    MatChipsModule,
    MatRadioModule,
    MatFormFieldModule,
    MatSelectModule,
    CommonModule,
    FormsModule,
  ],
})
export class SelectionControlComponent implements OnInit, OnChanges {
  @Input() options: SelectionOption[] = [];
  @Input() controlType: ControlType = "button-group"; // Default to radio buttons
  @Input() label: string = "";
  @Input() selectedValue: any;
  @Output() selectionChange = new EventEmitter<any>();
  selectedChips: SelectionOption[] = [];
  @Input() statusClasses: StatusClasses = {}; // New input property
  displayedOptions: SelectionOption[] = [];
  @Input() statusOrder: string[] = []; // 

  constructor() {}

  ngOnInit() {
    console.log("statusClasses", this.statusClasses);
    this.setDisplayedOptions();
    
    if (this.selectedValue && Array.isArray(this.selectedValue)) {
      this.selectedChips = this.options.filter((o) =>
        this.selectedValue.includes(o.value)
      );
    }
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes["options"] || changes["statusClasses"]) {
      this.setDisplayedOptions();
    }
  }

  selectValue(value: any) {
    this.selectedValue = value;
    this.onSelectionChange();
  }

  onSelectionChange() {
    this.selectionChange.emit(this.selectedValue);
  }

  onChipClick(option: SelectionOption, index: number) {
    console.log(`Chip clicked: ${option.viewValue} at index ${index}`);
  }

  getClasses(value: any): string {
    return this.selectedValue === value ? this.statusClasses[value] : "";
  }

  trackByFn(index: number, item: any) {
    return item.value; // Use a unique identifier for tracking
  }

  setDisplayedOptions() {
    if (!this.statusOrder || this.statusOrder.length === 0) {
      this.displayedOptions = [...this.options]; // Use original order if no statusOrder is provided
      return;
    }

    this.displayedOptions = [...this.options].sort((a, b) => {
      const aIndex = this.statusOrder.indexOf(a.value);
      const bIndex = this.statusOrder.indexOf(b.value);

      //Handle cases where values are not found in statusOrder
      if (aIndex === -1 && bIndex === -1) return 0; // Both not found, keep original order
      if (aIndex === -1) return 1; // a not found, b comes first
      if (bIndex === -1) return -1; // b not found, a comes first

      return aIndex - bIndex;
    });
  }
  
}
