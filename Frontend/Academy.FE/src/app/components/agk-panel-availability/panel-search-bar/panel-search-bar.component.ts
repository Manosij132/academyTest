import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { PanelFilter } from '../model/panel-filter.model';
import { PanelService } from '@services/panel.service';
import { Community } from '../model/community.model';
import { TDC } from '../model/tdc.model';
import { Seniority } from '../model/seniority.model';
import { Panel } from '../model/panel.model';
import { forkJoin, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatSelectModule } from '@angular/material/select';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'mf-app-panel-search-bar',
  templateUrl: './panel-search-bar.component.html',
  styleUrls: ['./panel-search-bar.component.css'],
  standalone: true,
  imports: [
    CommonModule, 
    MatDatepickerModule, 
    MatSelectModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule
  ]
})

export class PanelSearchBarComponent implements OnInit, OnDestroy {
  @Input() isPanelSearchTermVisible: boolean = true;
  @Output() search = new EventEmitter<PanelFilter>();
  @Output() resetFilters = new EventEmitter<PanelFilter>();

  tdcs: TDC[] = [];
  communities: Community[] = [];
  seniorities: Seniority[] = [];
  panels: Panel[] = [];
  panelFilterForm = new FormGroup({
    tdc: new FormControl([]),
    communities: new FormControl<any|null>([]),
    seniorities: new FormControl<any|null>([]),
    panelTypes: new FormControl<any|null>([]),
    startDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    endDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    searchTerm: new FormControl<any|null>(''),
    availableSlots: new FormControl<any|null>(false),
    isDeficit: new FormControl<any|null>(false),
  });

  private destroy$ = new Subject<void>();

  constructor(private panelService: PanelService) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnInit(): void {
    this.initializeLookups();
    this.setDefaultDates();
  }

  onSubmit() {
    if (!this.panelFilterForm.valid) {
      return;
    }

    let panelFilter = this.setPanelFilterValues();
    this.search.emit(panelFilter);
  }

  setPanelFilterValues(){
    const formValues = this.panelFilterForm.value;
    const panelFilter = new PanelFilter();

    panelFilter.tDCs = formValues.tdc || [];
    panelFilter.communities = formValues.communities;
    panelFilter.seniorities = formValues.seniorities;
    panelFilter.panelTypes = formValues.panelTypes;
    panelFilter.startDate = new Date(formValues.startDate || '').toISOString() || '';
    panelFilter.endDate = new Date(formValues.endDate || '').toISOString() || '';;
    panelFilter.searchTerm = formValues.searchTerm;
    panelFilter.availableSlots = formValues.availableSlots;
    panelFilter.isDeficit = formValues.isDeficit;

    return panelFilter;
  }

  onReset($event: Event) {
    $event.stopPropagation();
    this.panelFilterForm.reset();
    this.setDefaultDates();
    let panelFilter = this.setPanelFilterValues();

    this.resetFilters.emit(panelFilter);
  }

  private initializeLookups() {
    forkJoin([
      this.panelService.getCommunityData(),
      this.panelService.getTDCData(),
      this.panelService.getSeniorityData(),
      this.panelService.getAllPanelData(),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([communities, tdcs, seniorities, panels]) => {
        this.communities = communities ?? [];
        this.tdcs = tdcs ?? [];
        this.seniorities = seniorities ?? [];
        this.panels = panels ?? [];
      });
  }

  private setDefaultDates() {
    const now = new Date();
    
    const currentMonthStartDate = new Date(now.getFullYear(), now.getMonth(), 1);
    const currentMonthEndDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  
    this.panelFilterForm.controls.startDate.setValue(currentMonthStartDate);
    this.panelFilterForm.controls.endDate.setValue(currentMonthEndDate);
  
    this.panelFilterForm.updateValueAndValidity();
  }
}
