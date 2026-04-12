import { Routes } from "@angular/router";
import { LandingComponent } from "@components/landing/landing.component";
import { LoginComponent } from "@components/login/login.component";
import { NotfoundComponent } from "@shared/component/notfound/notfound.component";
import { ManageRoleComponent } from "@components/admin/manage-role/manage-role.component";
import { RequestTrackerComponent } from "./shared/component/request-tracker/request-tracker.component";
import { ManageComponent } from "@components/trainings/manage/manage.component";
import { SpinComponent } from "@components/trainings/spin/spin.component";
import { TrackerListComponent } from "@components/tracker-list/tracker-list.component";
import { DashboardComponent } from "@components/dashboard/dashboard.component";
import { MockInterviewComponent } from "@components/interview/mock-interview/mock-interview.component";
import { interviewAuthGuard } from "@guards/interview-auth.guard";
import { AddBulkActivityComponent } from "@components/activity/add-bulk-activity/add-bulk-activity.component";
import { TrainingReportListComponent } from "@components/reporting/training-report-list/training-report-list.component";
import { TrainingReportComponent } from "@components/reporting/training-report/training-report.component";
import { JobComponent } from "@components/admin/job-controller/job.component";
import { TrainingImpactComponent } from "@components/training-impact/training-impact/training-impact.component";
import { BookmarkFilterDataResolver } from "@components/reporting/training-report/bookmark-filter/bookmark-filter-data.resolver";
import { CandidatesComponent } from "@shared/component/mock-interview-new/candidates/candidates/candidates.component";
import { SenioritiesComponent } from "@shared/component/mock-interview-new/seniorities/seniorities/seniorities.component";
import { SkillsComponent } from "@shared/component/mock-interview-new/skills/skills.component";
import { ProfilesComponent } from "@shared/component/mock-interview-new/profiles/profiles/profiles.component";
import { QuestionsComponent } from "@shared/component/mock-interview-new/questions/questions/questions.component";
import { AIModelsComponent } from "@shared/component/mock-interview-new/aimodels/aimodels/aimodels.component";
import { InterviewsComponent } from "@shared/component/mock-interview-new/interviews/interviews/interviews.component";
import { PromptsComponent } from "@shared/component/mock-interview-new/prompts/prompts/prompts.component";
import { InterviewAnalysisComponent } from "@shared/component/mock-interview-new/interview-analysis/interview-analysis.component";
import { ModelScoringComponent } from "@shared/component/mock-interview-new/model-scoring/model-scoring.component";
import { MockInterviewLayoutComponent } from "@shared/component/mock-interview-new/mock-interview-layout/mock-interview-layout.component";
import { StartInterviewComponent } from "@shared/component/mock-interview-new/start-interview/start-interview.component";
import { RabbitmqComponent } from "@shared/component/mock-interview-new/rabbitmq/rabbitmq.component";
import { ViewInterviewVideoSummaryComponent } from "@shared/component/mock-interview-new/view-interview-video-summary/view-interview-video-summary.component";
import { DojoEngagenmentReportComponent } from "@components/reporting/dojo-activity/dojo-activity.component";
import { ManageTrainingComponent } from "@components/admin/manage-training/manage-training.component";
import { authGuard } from '@guards/auth.guard';
import { MockInterviewDetailsComponent } from '@components/interview/mock-interview-details/mock-interview-details.component';
import { ErrorPageComponentComponent } from '@shared/component/error-page-component/error-page-component.component';
import { FullTableViewComponent } from '@shared/component/full-table-view/full-table-view.component';
import { InterviewDetailsComponent } from "@shared/component/mock-interview-new/interview-details/interview-details.component";
import { AssignedThroughReportComponent } from "@components/reporting/assigned-through-report/assigned-through-report.component";
import { SummaryByStatusComponent } from "@components/country-staffing/summary/summary.component";
import { ListOfTicketsComponent } from "@components/country-staffing/list-of-tickets/list-of-tickets.component";
import { CountryStaffingComponent } from "@components/country-staffing/country-staffing.component";
import { PanelDashboardComponent } from "@components/agk-panel-availability/panel-dashboard/panel-dashboard.component";
import { PanelListComponent } from "@components/agk-panel-availability/panel-list/panel-list.component";
import { SlotManagementComponent } from "@components/agk-panel-availability/slot-management/slot-management.component";
import { PanelEfficiencyReportComponent } from "@components/agk-panel-availability/panel-efficiency-report/panel-efficiency-report.component";
import { InterviewScheduleComponent } from "@components/agk-panel-availability/interview-schedule/interview-schedule.component";
import { roleGuard } from "@guards/role.guard";
import { EvaluationReportComponent } from "@components/reporting/evaluation-report/evaluation-report.component";

export const routes: Routes = [
  { path: "", redirectTo: "/login", pathMatch: "full" }, // Redirect to login on empty path
  { path: "login", component: LoginComponent },
  {
    path: "",
    component: LandingComponent,
    canActivate: [authGuard],
    
    children: [
      { path: "dashboard/:id", component: DashboardComponent },
      {
        path: "list",
        component: TrackerListComponent,
        data: { pageHeader: "Tracker List" },
      },
      {
        path: "training-impact",
        component: TrainingImpactComponent,
        data: { pageHeader: "Globers Transitioned from DOJO to Projects – Training Impact Update" }
      },
      {
        path: "staffing",
        component: CountryStaffingComponent,
        data: { pageHeader: "DOJO details" },
      },
      {
        path: "staffing/summary",
        component: SummaryByStatusComponent
      },
      {
        path: "staffing/list-of-tickets",
        component: ListOfTicketsComponent
      },
      {
        path: "trainings/manage",
        component: ManageComponent,
        data: { pageHeader: "Add Training" },
      },
      {
        path: "trainings/spin",
        component: SpinComponent,
        data: { pageHeader: "Assign Trainings" },
      },

      {
        path: "trainings/bulk-activities",
        component: AddBulkActivityComponent,
      },
      { path: "track/worker-request/:id", component: RequestTrackerComponent },
      {
        path: "adm/manage/role",
        component: ManageRoleComponent,
        data: {
          pageHeader: "Manage Role",
        },
      },
      {
        path: "adm/manage/training",
        component: ManageTrainingComponent,
        data: {
          pageHeader: "Manage Training",
        },
      },
      {
        path: "adm/job",
        component: JobComponent,
        data: {
          pageHeader: "Job Controller",
        },
      },
      {
        path: "reports/dojo-engagement",
        component: DojoEngagenmentReportComponent,
        data: { pageHeader: "Dojo Engagement Report" },
      },
      {
        path: "reports/assigndojotrainingreport",
        component: AssignedThroughReportComponent,
        data: { pageHeader: "Assigned Through Training Report" },
      },
      {
        path: "trainingreportbookmarks",
        component: TrainingReportListComponent,
      },
      {
        path: "evaluationreportbookmarks",
        component: EvaluationReportComponent,
      },
      {
        path: "trainingreport",
        component: TrainingReportComponent,
        resolve: {
          bookmarkFilterData: BookmarkFilterDataResolver
        }
      },
      {
        path: "mockInterview",
        component: MockInterviewLayoutComponent,
        children: [
          { path: "candidates", component: CandidatesComponent },
          { path: "seniority", component: SenioritiesComponent },
          { path: "skill", component: SkillsComponent },
          { path: "profile", component: ProfilesComponent },
          { path: "questions", component: QuestionsComponent },
          { path: "evaluation", component: InterviewsComponent },
          { path: "aiModel", component: AIModelsComponent },
          { path: "prompts", component: PromptsComponent },
          { path: "interviewAnalysis", component: InterviewAnalysisComponent },
          { path: "interviewScoring", component: ModelScoringComponent },
          { path: "rabbitMq", component: RabbitmqComponent }
        ],
      },
      { path: "interview-details/:id", component: InterviewDetailsComponent },
      {
        path: "mock-interview-details/:id",
        component: MockInterviewDetailsComponent,
      },
      { path: 'full-table-view', component: FullTableViewComponent },
            
      //AGK Panel Availability
      {
        path: "agk/panel-availability/dashboard",
        component: PanelDashboardComponent,
        canActivate: [roleGuard], 
        data: {
          pageHeader: "Dashboard",
          roles: ['SystemAdmin', 'Recruiter'] 
        },
       
      },
      {
        path: "agk/panel-availability/panels",
        component: PanelListComponent, 
        canActivate: [roleGuard], 
        data: {
          pageHeader: "Panels",
          roles: ['SystemAdmin', 'Recruiter'] 
        },
      },
      {
        path: "agk/panel-availability/slot-management",
        component: SlotManagementComponent, 
        canActivate: [roleGuard], 
        data: {
          pageHeader: "Slot Management",
          roles: ['SystemAdmin', 'Recruiter'] 
        },
      },
      {
        path: "agk/panel-availability/panel-efficiency-report",
        component: PanelEfficiencyReportComponent, 
        canActivate: [roleGuard], 
        data: {
          pageHeader: "Panel Efficiency Report",
          roles: ['SystemAdmin', 'Recruiter'] 
        },
      },
      {
        path: "agk/panel-availability/interview-schedule",
        component: InterviewScheduleComponent,
        canActivate: [roleGuard], 
        data: {
          pageHeader: "Interview Schedule",
          roles: ['SystemAdmin', 'Recruiter'] 
        },
      }
      
    ],
  },
  { path: 'interview/:code', component: StartInterviewComponent },
  { path: 'view-interview/:code', component: ViewInterviewVideoSummaryComponent },
  { path: "invalid", component: ErrorPageComponentComponent },

  { path: "error/notfound", component: NotfoundComponent },
  { path: "**", redirectTo: "error/notfound" },
];
