import { CommonModule } from "@angular/common";
import { Component, OnInit, Output, EventEmitter } from "@angular/core";
import { NavigationEnd, RouterModule } from "@angular/router";
import { Router } from "@angular/router";
import { PrivilegedUserDirective } from "../../directives/privileged-user.directive";
import { MockInterviewMenuComponent } from "../mock-interview-new/mock-interview-menu/mock-interview-menu.component";
import { CountryStaffingComponent } from "../../../components/country-staffing/country-staffing.component";
import { AuthenticationService } from "@services/authentication.service";

@Component({
    selector: "app-sidebar",
    standalone: true,
    imports: [RouterModule, CommonModule, PrivilegedUserDirective, MockInterviewMenuComponent, CountryStaffingComponent],
    templateUrl: "./sidebar.component.html",
    styleUrl: "./sidebar.component.scss",
})
export class SidebarComponent implements OnInit {
    url: any = "";
    isMockInterviewCollapsed = false;
    isCountryStaffingCollapsed = false;
    @Output() navToggleEvent = new EventEmitter<string>();
    isProductionEnv: boolean = false;
    isTrainingsCollapsed: boolean = false;
    isExportReportCollapse: boolean = false;
    isAdminCollapsed: boolean = false;
    isPanelAvailabilityCollapsed: boolean = false;
 isAdminOrRecruiterRole: boolean = false;
 
constructor(
   private router: Router,
   private authService: AuthenticationService
 ) {
   this.url = this.router.url;
 }

 ngOnInit(): void {
    const roles = this.authService.userDetails?.roles || [];

    this.isAdminOrRecruiterRole = roles?.some(role =>
     role.roleName && ['SystemAdmin', 'Recruiter'].includes(role.roleName)
    );

   this.router.events.subscribe((event) => {
     if (event instanceof NavigationEnd) {
       this.url = event.urlAfterRedirects;

       this.isMockInterviewCollapsed = this.url.startsWith("/mockInterview");
     }
   });
 }

    loadScriptToggle(value: any) {
        this.navToggleEvent.emit(value);
        // this.isTrainingsCollapsed ? this.toggleTrainings() : this.toggleAdmin();
    }

    getMenuClick() {
        setTimeout(() => {
            this.url = this.router.url;
        }, 50);
    }

    toggleTrainings() {
        this.isTrainingsCollapsed = !this.isTrainingsCollapsed;
        if (this.isTrainingsCollapsed) {
            this.isAdminCollapsed = false;
            this.isMockInterviewCollapsed = false;
            this.isExportReportCollapse = false;
            this.isPanelAvailabilityCollapsed = false;
        }
    }

    toggleReports() {
        this.isExportReportCollapse = !this.isExportReportCollapse;
        if (this.isExportReportCollapse) {
            this.isAdminCollapsed = false;
            this.isMockInterviewCollapsed = false;
            this.isTrainingsCollapsed = false;
            this.isPanelAvailabilityCollapsed = false;
        }
    }

    toggleAdmin() {
        this.isAdminCollapsed = !this.isAdminCollapsed;
        if (this.isAdminCollapsed) {
            this.isTrainingsCollapsed = false;
            this.isMockInterviewCollapsed = false;
            this.isExportReportCollapse = false;
            this.isPanelAvailabilityCollapsed = false;
        }
    }


    toggleMockInterview() {
        this.isMockInterviewCollapsed = !this.isMockInterviewCollapsed;
        if (this.isMockInterviewCollapsed) {
            this.isTrainingsCollapsed = false;
            this.isAdminCollapsed = false;
            this.isExportReportCollapse = false;
            this.isPanelAvailabilityCollapsed = false;
        }
    }

    toggleCountryStaffing() {
        this.isCountryStaffingCollapsed = !this.isCountryStaffingCollapsed;
        if (this.isCountryStaffingCollapsed) {
            this.isTrainingsCollapsed = false;
            this.isAdminCollapsed = false;
            this.isExportReportCollapse = false;
            this.isPanelAvailabilityCollapsed = false;
        }
    }

    togglePanelAvailability() {
        this.isPanelAvailabilityCollapsed = !this.isPanelAvailabilityCollapsed;
        if(this.isPanelAvailabilityCollapsed){
            this.isTrainingsCollapsed=false;
            this.isAdminCollapsed=false;
            this.isExportReportCollapse=false;
            this.isMockInterviewCollapsed = false;
        }
    }
}