import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { Component, Inject, OnInit, ViewChild, ElementRef, Renderer2, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl } from "@angular/forms";
import { Observable } from "rxjs";
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule, MatOptionSelectionChange } from "@angular/material/core";
import { MatSelect, MatSelectModule } from "@angular/material/select";
import { MatIconModule } from "@angular/material/icon";
import { MatDividerModule } from "@angular/material/divider";
import { MatChipsModule } from "@angular/material/chips";
import { MatGridListModule } from '@angular/material/grid-list';
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { Employee } from "@shared/Interface/employee.model";
import { UpdateGXLeader, UpdateMentees } from "@shared/Interface/UpdateGxLeader.model";
import { LoaderService } from "@services/loader.service";
import { AcademyHttpService } from "@services/academy-http.service";

@Component({
  selector: 'mf-app-gx-mentees-dialog',
  standalone: true,
  templateUrl: 'gx-mentees-dialog.component.html',
  styleUrls: ['./gx-mentees-dialog.component.css'],
  imports: [
    MatDialogModule, MatGridListModule, MatSelectModule, MatDividerModule, MatChipsModule, MatAutocompleteModule, MatSlideToggleModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
    MatIconModule,
    CommonModule
  ]
})
export class GxMenteesDialogComponent implements OnInit {
  @ViewChild('searchInput') searchInput: ElementRef;
  @ViewChild('searchLocationInput') searchLocationInput : ElementRef;
  employeenew = new Employee();
  updateMentees = new UpdateMentees();
  selectedEmp: any;
  name?: string;
  email?: string;
  leaderSelected: boolean = false;
  id?: number;
  BetterMeLeaderEmail?: string;
  proposedDojoGxLeader?: string;
  searchGroup = new FormControl('');
  searchLocationGroup = new FormControl('');
  filteredLeader: Observable<string[]> | undefined;
  allLeaders: any;
  allCountries: any;
  minMenteeCount: number = 4;
  isAutoSuggestLeader: boolean = false;
  autoSuggestLeader: any;
  showLeaderDetails: boolean = false;
  isClientMatched: boolean = false;
  isProjectMatched: boolean = false;
  matchingLeaderNotFound: string = '';
  isvalidate: boolean = false;
  checked = false;
  disabled: boolean = false;
  today: Date = new Date();
  
  // communities: any;
  propusedTdc?: string;
  locations: string[] = [];
  communities: string[] = [];
  selectedCommunity: string[] = [];
  selectedTdc: string[] = [];
  selectedMentees: any[] = [];

  constructor(private LoaderService: LoaderService, private snackBar: MatSnackBar, 
    public dialogRef: MatDialogRef<GxMenteesDialogComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: any, private el: ElementRef, private renderer: Renderer2, 
    private readonly academyHttpService: AcademyHttpService) {
  }

  close(data?: any) {
    this.dialogRef.close(data);
  }
  ;

  ngOnInit() {
    //Remove Own name if exist in proposedGX leader list
    this.data.mentees = (this.data.mentees ?? []).filter(
          (leader: any) => leader.id !== this.data.employee.employeeId
    );
    if (this.data.employee.proposedDojoGxLeader != null || this.data.employee.proposedDojoGxLeader != "") {
      this.proposedDojoGxLeader = this.data.employee.proposedDojoGxLeader;
      this.propusedTdc = this.data.employee.tdc;
      this.leaderSelected = true;
      this.showLeaderDetails = true;
      this.data.selectedLeader = this.data.mentees.find((leader: any) => leader.globantEmailAddress === this.proposedDojoGxLeader);
    }
    this.allLeaders = this.data.mentees;
    this.selectedMentees = this.allLeaders.filter((x: any)=> this.data.selectedMentees.includes(x.employeeId));
    this.data.mentees = this.allLeaders.filter((x: any)=> x.tdc.includes(this.propusedTdc) && x.community.includes(this.data.employee.community) && x.employeeEmail != this.data.employee.employeeEmail && !x.proposedDojoGxLeader);
    this.data.mentees.push(this.selectedMentees);
    this.locations = this.data.locations;
    this.communities = this.data.communities;
    this.selectedTdc = [this.data.employee.tdc];
    this.selectedCommunity = [this.data.employee.community];
  }  

  search(value: any) {
    this.data.leaders = this.allLeaders.filter(function (element: { employeeName: string; }) {
      return (element.employeeName.includes(value));

    });
  }

  searchLocation(value: any) {
    this.data.locations = this.locations.filter((x: any)=> x.includes(value));
  }

  AutoSuggestLeader(event: any) {
    if (event.target.checked) {
      this.autoSuggestLeader = this.data.leaders.filter((x: any) =>x.menteesCount < 1  && (x.client == this.data.employee.client ||  x.project == this.data.employee.project))[0];
      if (this.autoSuggestLeader != undefined) {
        if (this.autoSuggestLeader.project == this.data.employee.project) {
          this.isProjectMatched = true;
        }
        if (this.autoSuggestLeader.client == this.data.employee.client) {
          this.isClientMatched = true;
        }
        this.isAutoSuggestLeader = true;
        this.showLeaderDetails = true;
        this.searchInput.nativeElement.setAttribute('disabled', '');
        //document.getElementById('searchInput')?.setAttribute('disabled', '')
        this.proposedDojoGxLeader = this.autoSuggestLeader.globantEmailAddress;
        this.data.selectedLeader = this.data.leaders.find((leader: any) => leader.globantEmailAddress === this.autoSuggestLeader.globantEmailAddress);
      }
      else {

        this.matchingLeaderNotFound = 'No suggestions available.';
        this.isAutoSuggestLeader = false;
        this.showLeaderDetails = false;
        this.proposedDojoGxLeader = '';
        this.data.selectedLeader = {};
        this.searchInput.nativeElement?.removeAttribute('disabled');
        //document.getElementById('searchInput')?.removeAttribute('disabled');
      }
    }
    else {
      this.matchingLeaderNotFound = '';
      this.isAutoSuggestLeader = false;
      this.showLeaderDetails = false;
      this.proposedDojoGxLeader = '';
      this.data.selectedLeader = {};
      this.searchInput.nativeElement?.removeAttribute('disabled');
      //document.getElementById('searchInput')?.removeAttribute('disabled');
      //when we uncheck the checkbox-if it had prev leader value in textbox then show its detail back
      if (this.data.employee.proposedDojoGxLeader != null || this.data.employee.proposedDojoGxLeader != "") {
        this.proposedDojoGxLeader = this.data.employee.proposedDojoGxLeader;
        this.leaderSelected = true;
        this.showLeaderDetails = true;
        this.data.selectedLeader = this.data.leaders.find((leader: any) => leader.globantEmailAddress === this.proposedDojoGxLeader);
      }
    }
  }

  clearSearch(event: any) {
    this.data.leaders = this.allLeaders;
  }

  clearLocationSearch(event: any) {
    this.data.locations = this.locations;
  }

  onSelect(selectedEmp: any): void {
    if (this.selectedMentees.length > 0) {
      this.updateMentees.EmployeeId = this.selectedMentees.map(x => x.employeeId);
      this.updateMentees.DojoGxLeaderEmail = this.proposedDojoGxLeader;
      this.updateMentees.DojoGxGlobarEmail = selectedEmp.employeeEmail;
      this.updateMentees.GloberName = selectedEmp.employeeName;
      this.updateMentees.ProposedLeaderName = selectedEmp.employeeName;
      this.updateMentees.ProposedLeaderSeniority = selectedEmp.seniority;
      this.updateMentees.GloberSeniority = selectedEmp.seniority == null ? "" : selectedEmp.seniority;

      this.academyHttpService.UpdateMentees(this.updateMentees).subscribe({
        next: (data) => {
          //Success popup - shweta borse
          this.snackBar.open('Data successfully updated', 'Close', {
            duration: 3000,
            verticalPosition: 'top',
            horizontalPosition: 'center'
          });
          this.close(data);
        },
        error: (err) => {
          //Error popup - shweta borse
          this.snackBar.open('Update unsuccessful. Please try again.', 'Close', {
            duration: 3000,
            verticalPosition: 'top',
            horizontalPosition: 'center',
            panelClass: ['snackbar-error']
          });
          this.dialogRef.close();
          // Log error for debugging
          console.error('Update failed:', err);
        }
      });
    } else {
      this.isvalidate = true;
    }
  }  

  onOptionsSelected(event: MatAutocompleteSelectedEvent) {
    this.proposedDojoGxLeader = event.option.value;
    this.leaderSelected = true;
    this.showLeaderDetails = true;
    if (!event.option.value) {
      this.leaderSelected = false;
      this.showLeaderDetails = false;
    }
    this.data.selectedLeader = this.data.leaders.find((leader: any) => leader.globantEmailAddress === event.option.value);
    if (this.data.selectedLeader.client == this.data.employee.client) {
      this.isClientMatched = true;
    }
    if (this.data.project == this.data.employee.project) {
      this.isProjectMatched = true;
    }
  }

  onOptionsLocationSelected(event: MatAutocompleteSelectedEvent) {
    this.propusedTdc = event.option.value;
    this.data.leaders = this.allLeaders.filter((x: any)=> x.tdc.includes(this.propusedTdc));
    // this.leaderSelected = true;
    // this.showLeaderDetails = true;
    // if (!event.option.value) {
    //   this.leaderSelected = false;
    //   this.showLeaderDetails = false;
    // }
    // this.data.selectedLeader = this.data.leaders.find((leader: any) => leader.globantEmailAddress === event.option.value);
    // if (this.data.selectedLeader.client == this.data.employee.client) {
    //   this.isClientMatched = true;
    // }
    // if (this.data.project == this.data.employee.project) {
    //   this.isProjectMatched = true;
    // }
  }

  onCommunityChanged(option: MatOptionSelectionChange) {
      if (!option.isUserInput) return;
  
      const value = option.source.value;
      const isChecked = option.source.selected;
      const allValues = this.communities;
  
      if (value === 'ALL') {
        this.selectedCommunity = isChecked ? ['ALL', ...allValues] : [];
      } else {
        if (isChecked) {
          this.selectedCommunity = [...new Set([...this.selectedCommunity, value])];
        } else {
          this.selectedCommunity =
            this.selectedCommunity.filter(v => v !== value);
        }
  
        const allSelected = allValues.every(v =>
          this.selectedCommunity.includes(v)
        );
  
        this.selectedCommunity = allSelected
          ? ['ALL', ...allValues]
          : this.selectedCommunity.filter(v => v !== 'ALL');
      }    
      this.data.mentees = this.allLeaders.filter((x: any) =>
                                                  this.selectedCommunity.some((community: string) => x.community.includes(community)) && this.selectedTdc.some((tdc: string) => x.tdc.includes(tdc))  && !x.proposedDojoGxLeader
                                                );
      // this.request.community =
      //   this.selectedCommunity.includes('ALL')
      //     ? []
      //     : this.selectedCommunity;
    }
  
    onCountryChanged(option: MatOptionSelectionChange) {
      if (!option.isUserInput) {
        return;
      }
      const value = option.source.value;
      const isChecked = option.source.selected;
      const allValues = this.data.locations;
      if (value === 'ALL') {
        if (isChecked) {
          this.selectedTdc = ['ALL', ...allValues];
        } else {
          this.selectedTdc = [];
        }
      } else {
        if (isChecked) {
          this.selectedTdc = [...new Set([...this.selectedTdc, value])];
        } else {
          this.selectedTdc = this.selectedTdc.filter((v: any) => v !== value);
        }
        const allSelected = allValues.every((v: any) =>
          this.selectedTdc.includes(v)
        );
        this.selectedTdc = allSelected
          ? ['ALL', ...allValues]
          : this.selectedTdc.filter(v => v !== 'ALL');
      }
      this.data.mentees = this.allLeaders.filter((x: any) =>
                                                  this.selectedTdc.some((tdc: string) => x.tdc.includes(tdc)) && this.selectedCommunity.some((community: string) => x.community.includes(community))  && !x.proposedDojoGxLeader
                                                );
      // this.request.country = this.selectedTdc.includes('ALL') ? [] : this.selectedTdc;    
    }

    onMenteesChanged(option: MatOptionSelectionChange) {
      if (!option.isUserInput) {
        return;
      }
      const value = option.source.value;
      const isChecked = option.source.selected;
      const allValues = this.data.mentees;
      if (value === 'ALL') {
        if (isChecked) {
          this.selectedMentees = ['ALL', ...allValues];
        } else {
          this.selectedMentees = [];
        }
      } else {
        if (isChecked) {
          this.selectedMentees = [...new Set([...this.selectedMentees, value])];
        } else {
          this.selectedMentees = this.selectedMentees.filter((v: any) => v !== value);
        }
        const allSelected = allValues.every((v: any) =>
          this.selectedMentees.includes(v)
        );
        this.selectedMentees = allSelected
          ? ['ALL', ...allValues]
          : this.selectedMentees.filter(v => v !== 'ALL');
      }
      // this.data.mentees = this.allLeaders.filter((x: any) =>
      //                                             this.selectedMentees.some((employeeId: string) => {x.employeeId.includes(employeeId); console.log(employeeId); })
      //                                           );
      // this.request.country = this.selectedTdc.includes('ALL') ? [] : this.selectedTdc;    
    }

  disabaleValidation() {
    this.isvalidate = false;
    this.isClientMatched = false;
    this.isProjectMatched = false;
  }
  
  hasSelectedMentees() {
    return this.selectedMentees && Object.keys(this.selectedMentees).length > 0;
  }

  @ViewChild('communitySelect') communitySelect!: MatSelect;
    @ViewChild('countrySelect') countrySelect!: MatSelect;
    @ViewChild('menteesSelect') menteesSelect: MatSelect;
  ngAfterViewInit() {
      // this.dataSource.paginator = this.paginator;
      // this.dataSource.sort = this.sort;
      this.communitySelect.optionSelectionChanges.subscribe(
        (event: MatOptionSelectionChange) => this.onCommunityChanged(event)
      );
  
      this.countrySelect.optionSelectionChanges.subscribe(
        (event: MatOptionSelectionChange) => this.onCountryChanged(event)
      );

      this.menteesSelect.optionSelectionChanges.subscribe(
        (event: MatOptionSelectionChange) => this.onMenteesChanged(event)
      );
      
    }
}