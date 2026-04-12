import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import {
  FormBuilder,  FormControl,  FormGroup,  FormsModule,
  ReactiveFormsModule
} from "@angular/forms";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatButtonModule } from "@angular/material/button";
import { MatNativeDateModule } from "@angular/material/core";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { AcademyHttpService } from "@services/academy-http.service";
import { CommonModule } from "@angular/common";
import { ToastrService } from "ngx-toastr";
import { filter, finalize, map, Observable, of } from "rxjs";
import { LoaderService } from "@services/loader.service";
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

interface EmployeeData {
  employeeId: number;
  employeeName: string;
  employeeEmail: string;
  careerMentorEmail: string | null;
  position: string | null;
  project: string | null;
  client: string | null;
  tdc: string | null;
  imageUrl: string;
  status: string | null;
  trainingCompletetionScore: number;
  proficiencyScore: number;
  seniority: string;
  baseLocation: string | null;
  totalTrainings: number;
  inProgressTrainings: number;
  completedTrainings: number;
  dojoGexLeaderEmail: string | null;
  dojoDetailId: number;
}

@Component({
  standalone: true,
  selector: "app-update-gex-leader",
  templateUrl: "./update-gex-leader.component.html",
  styleUrls: ["./update-gex-leader.component.css"],
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatInputModule,
    MatFormFieldModule,
    MatNativeDateModule,
    MatButtonModule,
    MatDatepickerModule,
    CommonModule,
  ],
})
export class UpdateGexLeaderComponent implements OnInit {
  @Output() dojoGexLeaderUpdated = new EventEmitter<string>();
  @Input() globar: any;

  myForm!: FormGroup;
  filteredOptions!: Observable<EmployeeData[]>;
  userControl = new FormControl();
  selectedUsers: any[] = [];
  filteredUsers!: Observable<any[]>;
  employees: any[] = [];
  filteredEmployees: any[] = [];
  filterChar = "";

  constructor(
    private fb: FormBuilder,
    private academyService: AcademyHttpService,
    private loaderService: LoaderService,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.myForm = this.fb.group({
      autocompleteInput: [""],
      startDate: [null],
      endDate: [null],
    });
    this.myForm
      .get("autocompleteInput")!
      .valueChanges.pipe(
        filter((value) => {
          return typeof value === "string" && value.trim().length > 2;
        }),
        map((filter) => this.getGexLeaders(filter))
      )
      .subscribe((data) => {
        console.log(data);
      });
  }

  private getGexLeaders(startsWith: string) {
    this.loaderService.start();
    this.academyService
      .gexLeaderStartsWith(startsWith)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.employees = [...response.data, ...this.employees];
            this.filteredEmployees = this.employees;
            this.filteredUsers = of(this.employees.map((emp) => emp));
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  // Display function for autocomplete to show the employeeName
  displayFn(item: EmployeeData | string | null): string {
    // 1. Check if the 'item' is null or undefined
    if (!item) {
      return "";
    }
    // 2. Check if the 'item' is already a string (typed by user)
    if (typeof item === "string") {
      return item;
    }
    // 3. If 'item' is an object, return the property you want to display
    // Ensure 'item.employeeName' exists and is a string
    return item.employeeEmail || "";
  }
  onSubmit(): void {
    if (this.myForm.valid) {
      const selectedItem = this.myForm.get("autocompleteInput")!
        .value as EmployeeData;

      const request = {
        dojoDetailId: this.globar.dojoDetailId,
        dojoStartDate: new Date(),
        employeeId: this.globar.employeeId,
        dojoGexLeaderEmail: selectedItem.employeeEmail,
        dojoGexGlobarEmail: this.globar.employeeEmail,
      };

      this.academyService
        .updateDejoGexLeader(request)
        .subscribe((response: any) => {
          console.log(response);
          if (response.success) {
            this.dojoGexLeaderUpdated.emit(selectedItem.employeeEmail);
           this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
          } else {
            this.dojoGexLeaderUpdated.emit("");
            this.toastr.error(response.errorMessage, "Error");
          }
        });
    } else {
      console.log("Form is invalid.");
    }
  }
}
