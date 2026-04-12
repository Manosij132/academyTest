import { CommonModule } from "@angular/common";
import { Component, Inject, OnInit, signal } from "@angular/core";
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatNativeDateModule } from "@angular/material/core";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatTableModule } from "@angular/material/table";
import { AcademyHttpService } from "@services/academy-http.service";
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';

export interface ActivityDetail {
    dojoDetailId: number;
    employeeId: number;
    employeeName: string;
    ticketNumber: number;
    comments: string;
    assignedThroughTraining: boolean;
    activityDetail: string[];
    isFocused?: boolean;
    projectName: string;
    client: string;
    positionTitle: string;
    skills: string;
}

@Component({
    selector: "app-update-training-impact-dialog",
    templateUrl: "./update-training-impact-dialog.component.html",
    styleUrls: ["./update-training-impact-dialog.component.css"],
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatSelectModule,
        MatIconModule,
        MatButtonModule,
        MatTableModule,
        MatDialogModule,
        ReactiveFormsModule,
        MatCheckboxModule,
        MatTabsModule,
        MatExpansionModule
    ],
})
export class UpdateTrainingImpactDialogComponent implements OnInit {
    newActivity = {
        assignedThroughTraining: false,
        comment: "",
        ticket: "",
    };
    color = "#eeeeee";
    form!: FormGroup;
    empEmails: string[] = [];
    activities: ActivityDetail[] = [];
    readonly panelOpenState = signal(false);
    showError: boolean = false;
    isFocused: boolean = false;

    displayedColumns: string[] = ['employeeName', 'ticketNumber', 'activityDetail', 'assignedThroughTraining', 'comments'];

    constructor(
        public dialogRef: MatDialogRef<UpdateTrainingImpactDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private formBuilder: FormBuilder,
        private readonly academyHttpService: AcademyHttpService,
    ) {
        this.empEmails = data;
    }

    onFocus(activity: ActivityDetail) {
        activity.isFocused = true; 
    }

    onBlur(activity: ActivityDetail) {
        activity.isFocused = false;
    }

    saveChanges() {
        this.showError = true;
        const invalidComments = this.activities.some(activity => !activity.comments || activity.comments.trim() === '');
        if (invalidComments) {
            return;
        }

        const activities = Array.from(this.activities).map((i) => {
            return {
                dojoDetailId: i.dojoDetailId,
                assignedThroughTraining: i.assignedThroughTraining,
                comments: i.comments,
                ticketNumber: i.ticketNumber,
            };
        });

        this.dialogRef.close(activities);
    }

    ngOnInit() {
        this.form = this.formBuilder.group({
            assignedThroughTraining: [
                this.newActivity.assignedThroughTraining,
                [Validators.required],
            ],
            comment: [this.newActivity.comment, [Validators.required]],
            ticket: [this.newActivity.ticket],
        });

        if (this.empEmails.length) {
            this.academyHttpService.fetchDojoEmployeeBusinessOrientedActivityDetails(this.empEmails).subscribe({
                next: (response: any) => {
                    if (response.data) {
                        this.activities = response.data;
                    }
                },
                error: (err) => {
                    // Handle error if needed
                    console.error('API error:', err);
                }
            });
        }
    }
}