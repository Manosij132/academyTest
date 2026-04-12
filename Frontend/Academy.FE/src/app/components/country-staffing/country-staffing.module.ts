import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CountryStaffingComponent } from './country-staffing.component';
import { SummaryByStatusComponent } from './summary/summary.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';

@NgModule({
    declarations: [
        CountryStaffingComponent,
        SummaryByStatusComponent,
    ],
    imports: [
        CommonModule,
        RouterModule,
        MatFormFieldModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatTableModule
    ],
    exports: [
        CountryStaffingComponent,
        SummaryByStatusComponent,
    ]
})
export class CountryStaffingModule { }
