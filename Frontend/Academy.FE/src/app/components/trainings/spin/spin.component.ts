import { Component, inject, signal, ViewChild } from "@angular/core";
import {
  FormBuilder, FormControl, FormsModule, ReactiveFormsModule,
  Validators
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxChange, MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInput, MatInputModule } from "@angular/material/input";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatStepper, MatStepperModule } from "@angular/material/stepper";
import { ToastrService } from "ngx-toastr";
import { AcademyHttpService } from "@services/academy-http.service";
import { LoaderService } from "@services/loader.service";
import { SpinTrainingRequest, User, UserTrainingMapping } from "@shared/dto/spin-training.request";
// import {MatIconModule} from '@angular/material/icon';
import { AsyncPipe, CommonModule } from "@angular/common";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatChipsModule } from "@angular/material/chips";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { MatExpansionModule } from "@angular/material/expansion";
import { MatIconModule } from "@angular/material/icon";
import { MatListModule } from "@angular/material/list";
import { MatRadioChange, MatRadioModule } from "@angular/material/radio";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { ActivatedRoute, Router } from "@angular/router";
import {
  BehaviorSubject, debounceTime, distinctUntilChanged, filter,
  finalize, forkJoin, map, Observable, startWith
} from "rxjs";
import { Seniority, TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: "app-spin",
  standalone: true,
  imports: [
    MatRadioModule,
    MatSlideToggleModule,
    MatProgressBarModule,
    MatButtonModule,
    MatStepperModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatCheckboxModule,
    MatIconModule,
    MatSelectModule,
    MatChipsModule,
    MatListModule,
    CommonModule,
    MatExpansionModule,
    AsyncPipe,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatSlideToggleModule,
    MatDialogModule,
  ],
  templateUrl: "./spin.component.html",
  styleUrls: ["./spin.component.scss"],
  providers: [],
})
export class SpinComponent {
  readonly dialog = inject(MatDialog);

  readonly panelOpenState = signal(false);

  private _formBuilder = inject(FormBuilder);

  firstFormGroup = this._formBuilder.group({
    firstCtrl: ["", Validators.required],
  });

  secondFormGroup = this._formBuilder.group({
    secondCtrl: ["", Validators.required],
  });

  thirdFormGroup = this._formBuilder.group({
    thirdCtrl: ["", Validators.required],
  });
  isLinear = false;

  toppingsControl = new FormControl();
  topControl = new FormControl();
  empControl = new FormControl();
  accountTrainingControl = new FormControl();
  selected: SpinTrainingRequest = new SpinTrainingRequest();
  selectedUser: any = [];
  users: any;
  showIndivisual: boolean = false;
  showAccount: boolean = true;
  trainingList!: any[];
  progress: number = 0;
  employees: any[] = [];
  filteredEmployees: any[] = [];
  numbers = Array(26)
    .fill(0)
    .map((x, i) => String.fromCharCode(97 + i));
  keep: string[] = [];
  filterChar = "";
  checked: string = "checked";
  ecosystems: any[] = [];
  accounts: any[] = [];
  ecosystemTrainings: any;
  employeeSkillEndorsement: any;
  transactionId: string = "";

  ecoSystemControl = new FormControl("");
  myControlTwo = new FormControl("");
  options: string[] = [];
  filteredOptions!: Observable<string[]>;
  filteredOptionsTwo!: Observable<string[]>;
  selectedTrainingControl = new FormControl("");
  selectedTrainingControlGroup = new FormControl("");
  filteredTraining!: Observable<string[]>;
  filterTrainingGroupControl!: Observable<string[]>;
  trainings: any[] = [];
  employeeList: any[] = [];

  userControl = new FormControl();
  accountControl = new FormControl();

  private filteredUsers = new BehaviorSubject<User[]>([]);
  public filteredUsers$: Observable<User[]> = this.filteredUsers.asObservable();

  filteredAccount!: Observable<any[]>;
  selectedTraining: any[] = [];

  rows: any[] = [];
  trainingSourceText: string = "";
  trainingSources: string[] = [];
  defaultTrainingSrc: string = "Globant Studios";
  othersTrainingSrc: string = "Others";
  private readonly _route = inject(ActivatedRoute);
  protected pageHeader = this._route.snapshot.data["pageHeader"];

  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private readonly router: Router,
    private loaderService: LoaderService
  ) { }

  ngOnInit() {
    this.progress = 0;
    this.selected = new SpinTrainingRequest();
    this.loadEcosystems();
    this.selected = new SpinTrainingRequest();
    this.loadAccounts();

    this.userControl.valueChanges
      .pipe(
        debounceTime(500), // Wait for 500ms after the user stops typing
        distinctUntilChanged(), // Only emit if the current value is different from the previous one
        filter((value) => {
          // If the value is not a string, use the last filter (this.lastFilter)
          // const filter = typeof value === 'string' ? value : this.lastFilter;
          // this.lastFilter = filter;     // Store the current filter for the next value
          return typeof value === "string" && value.trim().length > 2;
        }),
        map((filter) => this.loadEmployees(filter)) // Apply the filter on the users
      )
      .subscribe();
  }

  loadAccounts() {
    if (this.selectedaccount != "") return;

    this.loaderService.start();
    this.academyHttpService
      .fetchAllAccount()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.accounts = response.data;

            this.initTrainingSources(response.data);

            this.filteredAccount = this.accountControl.valueChanges.pipe(
              startWith(""),

              map((value) =>
                typeof value === "string" ? value : this.lastFilter
              ),
              map((filter) => this.filterAccount(filter))
            );
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  initTrainingSources(data: any) {
    this.trainingSources = data;
  }

  onTrainingSourceChange() {
    // Reset trainingSourceText when a different account is selected
    if (this.selected.trainingAssignmentSrc !== this.othersTrainingSrc) {
      this.trainingSourceText = "";
    }
  }

  isSubmitEnabled(): boolean {
    // Ensure that a value is selected and if 'Others' is selected, trainingSourceText is not empty
    return (
      this.selected.trainingAssignmentSrc?.length > 0 &&
      (this.selected.trainingAssignmentSrc !== this.othersTrainingSrc ||
        this.trainingSourceText.trim() !== "")
    );
  }

  onUserSelected(selectedUser: any) {
    let index = this.filteredEmployees
      .map((el) => el.employeeEmail)
      .indexOf(selectedUser.email);

    if (index !== -1) {
      let item = this.filteredEmployees[index];
      let mapping = new UserTrainingMapping();
      mapping.userId = item.employeeId;
      mapping.seniorityId = item.seniority;
      mapping.seniority =
        Seniority.find((x) => x.Id == item.seniority)?.Text ?? "";
      mapping.userEmail = item.employeeEmail;
      mapping.userImage = item.imageUrl;
      mapping.trainings = item.ecosystem;
      if (selectedUser.checked) {
        mapping.selected = selectedUser.checked;
        item.parent = false;
        this.selected.mapping.push(mapping);
      } else {
        var msp = this.selected.mapping.find(
          (x) => x.userEmail === selectedUser.email
        );
        if (msp) {
          var i = this.selected.mapping.indexOf(msp);
          this.selected.mapping.splice(i, 1);
        }
      }
    }
  }

  onTraingSelected(data: any, topping: any) {
    let selectedTraing: any = [];
    selectedTraing.push(data.value);
    this.selected.mapping.map((e) => {
      if (e.userEmail == topping.userEmail) {
        e.selectedTraning = selectedTraing;
      }
    });
  }

  private removeFirst<T>(array: T[], toRemove: T): void {
    const index = array.indexOf(toRemove);
    if (index !== -1) {
      array.splice(index, 1);
    }
  }

  filterAccount(filter: string): any {
    return this.accounts.filter((option) =>
      option.toLowerCase().includes(filter)
    );
  }

  filterTraining(value: string): string[] {
    const filterValue = value.toLowerCase();
    let t = this.selected.mapping.map((item) => item.trainings);
    let trainingsList = t[0].map((e) => e);
    this.trainings = trainingsList.map((item) => item.trainingName);
    return this.trainings.filter((option) =>
      option.toLowerCase().includes(filterValue)
    );
  }

  filterTrainingGroup(value: string): string[] {
    const filterValue = value.toLowerCase();
    let t = this.selected.mapping.map((item) => item.trainings);

    //let trainingsList = t[0].map((e) => e);
    this.trainings = this.trainingList.map((item) => item.trainingName);
    return this.trainings.filter((option) =>
      option.toLowerCase().includes(filterValue)
    );
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();
    this.options = this.ecosystems.map((e) => e.name);
    return this.options.filter((option) =>
      option.toLowerCase().includes(filterValue)
    );
  }

  private _filterTwo(value: string): string[] {
    const filteedrValue = value.toLowerCase();

    this.options = this.employeeList.map((e) => e);
    return this.options.filter((option) =>
      option.toLowerCase().includes(filteedrValue)
    );
  }

  get_FA_class(n: string) {
    return "fa-" + n;
  }

  setEcosystem(event: any) {
    this.keep = [];
    this.selected = new SpinTrainingRequest();
    this.filteredEmployees = [];
    this.employees = [];
    this.filterChar = "a";
    this.selected.ecosystem = event.value;
  }

  setAccount(event: any) {
    this.keep = [];
    this.selected = new SpinTrainingRequest();
    this.filteredEmployees = [];
    this.employees = [];
    this.filterChar = "a";
    this.selected.account = event.value;
  }

  loadEcosystems() {
    if (this.selected.ecosystem != 0) return;
    setTimeout(() => {
      this.filteredOptions = this.ecoSystemControl.valueChanges.pipe(
        startWith(" "),
        map((value) => this._filter(value || ""))
      );
    }, 10);
    this.loaderService.start();
    this.academyHttpService
      .fetchPrimaryEcosystems()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.ecosystems = response.data;
            this.ecosystems = this.ecosystems.filter((x) => x.isPrimary);
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  getEcosystemText($id: any) {
    return this.ecosystems.filter((x) => x.id == $id)[0].value;
  }

  lastFilter: string = "";
  ecosystem: any;

  loadEmployees(startsWith: string) {
    this.userControl.setValue(null);
    setTimeout(() => {
      this.filteredOptionsTwo = this.ecoSystemControl.valueChanges.pipe(
        startWith(""),
        map((value) => (typeof value === "string" ? value : this.lastFilter)),
        map((filter) => this._filterTwo(filter))
      );
    }, 10);
    this.filterChar = startsWith;

    if (this.ecosystem !== this.selected.ecosystem) {
      this.selectedaccount = [];
      this.selectedUsers = [];
      this.accountControl.setValue(null);
    }
    this.ecosystem = this.selected.ecosystem;
    this.loaderService.start();
    this.academyHttpService
      .loadEmployee(startsWith, this.selected.ecosystem, this.selected.account)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          this.employees = [];
          if (response.status === 200) {
            this.employees = [...response.data, ...this.employees];
            this.employees = this.employees.map((item) => {
              if (!(this.checked in item)) {
                return { ...item, ["checked"]: false };
              }
              return item;
            });

            this.filteredEmployees = this.employees;
            this.filteredUsers.next(
              this.employees.map((emp) => emp.employeeEmail)
            );
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  loadSkillTrainingData() {
    this.loaderService.start();
    const commaSeperatedEmployeeIds = this.selected.mapping
      .map((m) => m.userId)
      .join(",");

    forkJoin({
      $ecosystemTrainings:
        this.academyHttpService.loadSkillTrainingMetadataByEcosystem(
          this.selected.ecosystem
        ),
    })
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          this.ecosystemTrainings = response.$ecosystemTrainings.data;
          // this.employeeSkillEndorsement = response.$employeeSkillEndorsement.data;
          let distinctTrainings = this.ecosystemTrainings.flatMap(
            (skill: { trainings: any[]; skillId: any }) =>
              skill.trainings.map((training) => ({
                SkillId: skill.skillId,
                trainingName: training.trainingName,
                trainingLink: training.trainingLink,
                seniorityId: training.seniorityId,
                seniority: training.seniority,
                trainingId: training.trainingId,
                trainingDescription: training.trainingDescription,
                trainingCompletionHours: training.trainingCompletionHours,
                isMvP: training.isMvP,
                checked: training.isMvP,
              }))
          );

          for (let u = 0; u < this.selected.mapping.length; u++) {
            let $user = this.selected.mapping[u];
            $user.trainings = distinctTrainings.filter((x: any) => {
              return x.seniorityId == $user.seniorityId;
            });
            this.getParentCheckedCondition($user);
          }
          this.trainingList = [];
          distinctTrainings.forEach((el: any) => {
            let index = this.trainingList
              .map((item) => item.trainingId)
              .indexOf(el.trainingId);
            if (index === -1) {
              this.trainingList.push(el);
            }
          });

          this.filteredTraining =
            this.selectedTrainingControl.valueChanges.pipe(
              startWith<any>(""),
              map((value) =>
                typeof value === "string" ? value : this.lastFilter
              ),
              map((filter) => this.filterTraining(filter))
            );

          this.filterTrainingGroupControl =
            this.selectedTrainingControlGroup.valueChanges.pipe(
              startWith<any>(""),
              map((value) =>
                typeof value === "string" ? value : this.lastFilter
              ),
              map((filter) => this.filterTrainingGroup(filter))
            );
        },
      });
  }

  setPageAndProgress(p: number) {
    this.progress = p;
  }
  item: any;

  setUser(event: any, $item: any) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      let mapping = new UserTrainingMapping();
      mapping.userId = event.target.value;
      mapping.seniorityId = $item.seniority;
      mapping.seniority =
        Seniority.find((x) => x.Id == $item.seniority)?.Text ?? "";
      mapping.userEmail = $item.employeeEmail;
      mapping.userImage = $item.imageUrl;
      $item.checked = true;
      $item.parent = false;
      this.selected.mapping.push(mapping);
    } else {
      this.removeUser(event.target.value, $item);
    }
  }

  removeUser(userId: Number, $item: any) {
    this.selected.mapping = this.selected.mapping.filter(
      (x) => x.userId != userId
    );
    $item.checked = false;
  }

  onTrainingCheckChanged(event: any, training: any, user: any) {
    const isChecked = (event.target as HTMLInputElement).checked;
    training.checked = isChecked;
    this.getParentCheckedCondition(user);
  }

  getParentCheckedCondition($user: any) {
    $user.parent = !$user.trainings.some(
      (training: any) => training["checked"] === false
    );
  }

  getSelectedTrainingsCount($user: any) {
    return $user.trainings.filter((item: any) => item.checked).length;
  }

  onParentCheckChanged(event: any, user: UserTrainingMapping) {
    event.stopPropagation();
    const isChecked = (event.target as HTMLInputElement).checked;
    user.parent = isChecked;
    user.trainings.map((item: any) => {
      item.checked = isChecked;
    });
  }
  iAgree(event: any) {
    const isChecked = (event.target as HTMLInputElement).checked;
    this.selected.force = isChecked;
  }
  @ViewChild("stepper") stepper!: MatStepper;
  spinTrainings() {
    this.loaderService.start();
    if (
      this.selected.trainingAssignmentSrc == this.othersTrainingSrc &&
      this.trainingSourceText
    ) {
      this.selected.trainingAssignmentSrc = this.trainingSourceText;
    }

    this.selected.mapping.map((user) => ({
      ...user,
      trainings: user.trainings.filter((training) =>
        user.selectedTraning.includes(training.trainingName)
      ),
    }));
    
    this.academyHttpService
      .spinTrainings(this.selected)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.transactionId = response.data;
            this.stepper.reset();
            this.selected = new SpinTrainingRequest();
            this.selected.account = "";
            this.selected.mapping = [];
            this.selected.selectedTraning = [];
            this.trainingList = [];
            this.selectedUsers = [];
            this.ecosystem = "";
            this.resetAll();
            this.toastr.success(TOASTER_MESSAGES.CREATE_SUCCESS, "Success");
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  navigateToRequestTracker() {
    this.router.navigate(["/track/worker-request", this.transactionId]);
  }

  optionClicked(event: Event) {
    event.stopPropagation();
  }

  filterEmp(filter: string): any {
    return this.filteredEmployees
      .map((emp) => emp.employeeEmail)
      .filter((option: any) => {
        if (option.includes(filter)) return option;
      });
  }

  selectedUsers: any[] = [];

  toggleSelectionUser(event: MatCheckboxChange, user: any) {
    this.selected.mapping.forEach((el) => {
      el.selectedTraning = [];
    });
    this.selectedTraining = [];

    if (event.checked) {
      if (user === "All") {
        this.selectedUsers = this.filteredEmployees.map((e) => e.employeeEmail);
      } else {
        const userToAdd = this.selectedUsers.find((value) => value === user);
        if (!userToAdd) {
          this.selectedUsers.push(user);
        }
      }
    } else {
      if (user === "All") {
        this.selectedUsers = [];
      } else {
        const userToRemove = this.selectedUsers.find((value) => value === user);
        if (userToRemove) {
          const indexOfUserToRemove = this.selectedUsers.indexOf(userToRemove);
          this.selectedUsers.splice(indexOfUserToRemove, 1);
        }
      }
    }

    this.onUserSelected({ checked: event.checked, email: user });
  }

  optionClickedAccount(event: Event, account: any) {
    //this.toggleSelectionAccount(event,account);
  }

  selectedaccount: any = [];
  toggleSelectionAccount(event: MatCheckboxChange, account: any) {
    if (event.checked) {
      this.selectedaccount.push(account);
    } else {
      const i = this.selectedaccount.findIndex(
        (value: { id: any }) => value == account.name
      );
      this.selectedaccount.splice(i, 1);
    }
  }

  onRemoveUser(event: Event, user: any) {
    const i = this.selectedUsers.indexOf(user);
    this.removeFirst(this.selectedUsers, event);

    var msp = this.selected.mapping.find((x: any) => x.userEmail === user);
    if (msp) {
      var index = this.selected.mapping.indexOf(msp);
      this.selected.mapping.splice(index, 1);
    }

    this.selectedUsers.splice(i, 1);
    this.toppingsControl.setValue(this.selectedUsers);
  }

  onRemoveAccount(event: Event, account: any) {
    const i = this.selectedaccount.indexOf(account);
    this.selectedaccount.splice(i, 1);
  }

  radioOptions: string = "1";
  onRadioButtonChange(event: MatRadioChange) {
    if (event.value == 1) {
      this.showAccount = true;
      this.showIndivisual = false;
      this.selectedTraining = [];
    }
    if (event.value == 2) {
      this.showIndivisual = true;
      this.showAccount = false;
      this.selected.mapping.forEach((el) => {
        el.selectedTraning = [];
      });
    }
    this.selected.force = false;
  }

  isUserSelected(user: any): boolean {
    return this.selectedUsers.includes(user);
  }

  toggleGroupSelection(event: MatCheckboxChange, training: any) {
    this.selected.force = false;

    if (event.checked) {
      if (training === "All") {
        this.selectedTraining = [...this.trainings];
      } else {
        this.selectedTraining.push(training);
      }
    } else {
      if (training === "All") {
        this.selectedTraining = [];
      } else {
        let trainingIndex = this.selectedTraining.indexOf(training);
        if (trainingIndex !== -1) {
          this.selectedTraining.splice(trainingIndex, 1);
        }
      }
    }
    this.selected.mapping.forEach(
      (user) => (user.selectedTraning = this.selectedTraining)
    );

    if (this.selected.mapping.length === 0)
    {
      this.selected.selectedTraning = this.selectedTraining;
    }
  }

  isGroupSelected(training: string): boolean {
    return this.selectedTraining.includes(training);
  }

  onChipRemoved(topping: string) {
    const indexToRemove = this.selectedTraining.indexOf(topping);
    if (indexToRemove !== -1) {
      this.selectedTraining.splice(indexToRemove, 1); // Remove the selected training
    }
  }

  chipRemoved(topping: string, el: any, index: number) {
    const indexToRemove = el.selectedTraning.indexOf(topping);
    if (indexToRemove !== -1) {
      el.selectedTraning.splice(indexToRemove, 1); // Remove the selected training
    }
  }

  isSelected(training: string, el: any): boolean {
    return el.selectedTraning.includes(training);
  }

  toggleSelection(event: MatCheckboxChange, training: any, el: any) {
    this.selected.force = false;

    let index = this.selected.mapping.map((el) => el.userId).indexOf(el.userId);
   
    if (event.checked) {
      if (training === "All") {
        let trainings = this.selected.mapping
          .map((item) => item.trainings)[0]
          .map((el) => el.trainingName);
        this.selected.mapping[index].selectedTraning = trainings;
      } else {
        this.selected.mapping[index].selectedTraning.push(training);
      }
    } else {
      if (training === "All") {
        this.selected.mapping[index].selectedTraning = [];
      } else {
        let trainingIndex =
          this.selected.mapping[index].selectedTraning.indexOf(training);
        if (trainingIndex !== -1) {
          this.selected.mapping[index].selectedTraning.splice(trainingIndex, 1);
        }
      }
    }
  }

  selectAll: boolean = true;
  changeToggle() {
    this.userControl.setValue(null);
    if (this.selectAll) {
      this.selectedUsers = [];
    }
  }

  checkAccountSelection(account: any) {
    this.selected.account = account;
  }

  activeNext() {
    if (this.showAccount) {
      return this.selectedTraining.length == 0;
    }

    if (this.showIndivisual) {
      return (
        this.selected.mapping.filter((el) => el.selectedTraning.length > 0)
          .length == 0
      );
    }
    return false;
  }

  checkForceAssign(event: any) {
    this.selected.force = event.checked;
  }

  @ViewChild("nameInput") nameInput!: MatInput;

  gotoEcosystemSelection() {
    this.selected.account = "";
    this.accountControl.setValue("");
    this.selected.mapping = [];
    this.selectedUsers = [];
    this.filteredUsers.next([]);
    this.userControl.setValue("");
  }

  gotoGlobarSelection() {
    this.selected.mapping.forEach((globar) => {
      globar.selectedTraning = [];
    });
    this.selectedTraining = [];
    this.selected.trainingAssignmentSrc = this.defaultTrainingSrc;
    this.selectedTrainingControl.setValue("");
    this.selectedTrainingControlGroup.setValue("");
  }

  gotoTrainingsAssignment() {
    this.selected.trainingAssignmentSrc = "";
    this.selected.force = false;
  }

  resetAll() {
    this.gotoTrainingsAssignment();
    this.gotoGlobarSelection();
    this.gotoEcosystemSelection();
    this.ecoSystemControl.setValue("");
  }
}
