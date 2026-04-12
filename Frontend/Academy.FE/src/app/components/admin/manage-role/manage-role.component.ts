import { Component, inject, signal } from "@angular/core";
import {
  FormBuilder,  FormControl,  FormGroup,  FormsModule,
  ReactiveFormsModule,  Validators
} from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { Observable } from "rxjs";
import {
  debounceTime,  distinctUntilChanged,  finalize,  map,
  startWith,  switchMap
} from "rxjs/operators";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatCardModule } from "@angular/material/card";
import { CommonModule } from "@angular/common";
import { MatSelectModule } from "@angular/material/select";
import { MatButtonModule } from "@angular/material/button";
import { MatExpansionModule } from "@angular/material/expansion";
import { AcademyHttpService } from "@services/academy-http.service";
import { ToastrService } from "ngx-toastr";
import { LoaderService } from "@services/loader.service";
import { Roles, TOASTER_MESSAGES } from "@shared/constants/app.constants";
import { ActivatedRoute } from "@angular/router";

export interface User {
  employeeId: number;
  employeeName: string;
  globantEmailAddress: string;
  seniority: string;
  roles: Role[];
}

export interface Role {
  roleId: number;
  roleName: string;
  roleAssignment: string;
}

@Component({
  selector: "app-manage-role",
  templateUrl: "./manage-role.component.html",
  styleUrls: ["./manage-role.component.scss"],
  standalone: true,
  imports: [
    CommonModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    ReactiveFormsModule,
    MatSelectModule,
    FormsModule,
    MatButtonModule,
    MatExpansionModule,
  ],
})
export class ManageRoleComponent {
  readonly panelOpenState = signal(false);
  roleAssignments = new FormControl("");
  selectedValue: string | undefined;
  // Define userControl as FormControl of type User or string for input and selection
  userControl = new FormControl<string | User>("");
  users: User[] = [];

  // Define the filteredUsers$ observable with the correct type
  filteredUsers$: Observable<User[]>;
  // Selected user object, initially null
  selectedUser: User | null = null;
  //Get system roles list
  SystemRoles: any[] = [];
  roleAssignmentList: any[] = [];
  emproleForm!: FormGroup;
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService,
    private fb: FormBuilder
  ) {
    // Initialize filteredUsers$ with type-safe logic
    this.filteredUsers$ = this.userControl.valueChanges.pipe(
      debounceTime(300), // Wait for 300ms between keystrokes
      distinctUntilChanged(), // Only proceed if the input has changed
      startWith(""),
      map((value) => (typeof value === "string" ? value : value?.employeeName)),
      map((employeeName) =>
        employeeName ? this._filterUsers(employeeName) : this.users.slice()
      )
    );
    //Fetch all roles from constants
    this.SystemRoles = Roles;
    this.initForm();
  }

  // Initialize employee form
  initForm() {
    this.emproleForm = this.fb.group({
      employeeId: ["", Validators.required],
      selectedRole: ["", Validators.required],
      roleAssignments: [[]],
    });
  }

  // Method to filter users by employeeName
  private _filterUsers(employeeName: string): User[] {
    const filterValue = employeeName.toLowerCase();
    return this.users.filter((user) =>
      user.employeeName.toLowerCase().includes(filterValue)
    );
  }

  // Function to display the employeeName in the input field after selection
  displayFn(user: User): string {
    return user && user.employeeName ? user.employeeName : "";
  }

  // Method to handle user selection from autocomplete
  onSelectUser(user: User): void {
    this.selectedUser = user;
  }

  getUserName() {
    const searchTerm = this.userControl.value;
    if (
      typeof searchTerm === "string" &&
      searchTerm.trim() &&
      searchTerm.trim().length > 2
    ) {
      this.loaderService.start();
      this.academyHttpService
        .searchEmployee(searchTerm)
        .pipe(finalize(() => this.loaderService.stop()))
        .subscribe({
          next: (response: any) => {
            this.loaderService.stop();
            if (response.status === 200) {
              this.users = response.data;
            } else {
              this.toastr.error(
                response.errorMessage,
                "Search Employees Error"
              );
            }
          },
        });
    }
  }
  //Role Selection change
  onRoleChanged(ev: any): void {
    this.roleAssignmentList = [];
    const value = ev.value;
    this.emproleForm.get("roleAssignments")?.setValue("");
    switch (value) {
      case 4:
        this.fetchAllEcosystems();
        this.emproleForm
          .get("roleAssignments")
          ?.setValidators(Validators.required);
        break;
      case 3:
        this.fetchAllCommunities();
        this.emproleForm
          .get("roleAssignments")
          ?.setValidators(Validators.required);
        break;
      case 2:
        this.fetchAllTdcs();
        this.emproleForm
          .get("roleAssignments")
          ?.setValidators(Validators.required);
        break;
      case 5:
        this.fetchAllAccounts();
        this.emproleForm
          .get("roleAssignments")
          ?.setValidators(Validators.required);
        break;
      default:
        this.emproleForm.get("roleAssignments")?.clearValidators();
        break;
    }
    this.emproleForm.get("roleAssignments")?.updateValueAndValidity();
    this.emproleForm.get("employeeId")?.setValue(this.selectedUser?.employeeId);
  }

  fetchAllTdcs() {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllTdc()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.roleAssignmentList = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  fetchAllCommunities() {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllCommunity()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.roleAssignmentList = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  fetchAllEcosystems() {
    this.loaderService.start();
    this.academyHttpService
      .fetchPrimaryEcosystems()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.roleAssignmentList = response.data;
            this.roleAssignmentList = this.roleAssignmentList
              .filter((x) => x.isPrimary)
              .map((x) => x.name);
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  fetchAllAccounts() {
    this.loaderService.start();
    this.academyHttpService
      .fetchAllAccount()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.roleAssignmentList = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  // Save button click
  onSaveChanges() {
    if (!this.emproleForm.valid) {
      this.toastr.error("Please select the details", "Error");
      return;
    }
    this.loaderService.start();

    var request = {
      employeeId: this.emproleForm.value.employeeId,
      roleAssignments:
        this.emproleForm.value.roleAssignments === ""
          ? []
          : this.emproleForm.value.roleAssignments,
      selectedRole: this.emproleForm.value.selectedRole,
    };
    this.academyHttpService
      .updateEmployeeRole(request)
      .pipe(
        switchMap(() => this.academyHttpService.searchEmployee(this.selectedUser?.globantEmailAddress)),
        finalize(() => this.loaderService.stop())
      )
      .subscribe({
          next: (response: any) => {
            if (response.status === 200) {
              this.users = response.data;
              this.selectedUser = this.users.find(x => x.employeeId == this.selectedUser?.employeeId)!
              this.toastr.success(TOASTER_MESSAGES.UPDATE_SUCCESS, "Success");
            } else {
              this.toastr.error(response.errorMessage, "Error");
            }
          },
        });
  }

  // Reset Form
  resetForm() {
    this.selectedUser = null;
    this.roleAssignmentList = [];
    this.roleAssignments = new FormControl("");
    this.selectedValue = "";
    this.emproleForm.reset();
    this.initForm();
  }
}
