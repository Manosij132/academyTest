import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "@environments/environment";
import { Observable } from "rxjs";
import { ApiConstants } from "@shared/constants/api.constants";
import { DataRequestOptions } from "@shared/dto/data-request-options.dto";
import { ChangeStatusRequest } from "@shared/dto/change-status-request";
import { CommentRequest } from "@shared/dto/comment-request";
import { ChangeProficiencyRequest } from "@shared/dto/update-proficiency-request";
import { SpinTrainingRequest } from "@shared/dto/spin-training.request";
import { CreateEcosystemRequest } from "@shared/dto/create-ecosystem";
import { CreateSkillRequest } from "@shared/dto/create-skill";
import { MockInterviewDetail } from "@shared/dto/interviewdetails-response";
import { UpdateDojoGexLeaderRequest } from "@shared/dto/update-dojo-gex-leader-request";
import { ExportDetailReportMetadata, ExportReportMetadata } from "@shared/dto/ExportReportMetadata";
import { BookmarkForms } from "@shared/dto/bookmark-form.dto";
import { ReportEmailRequest } from "@components/reporting/training-report/preview-send-email/preview-send-email.component";
import { UpdateGXLeader, UpdateMentees } from "@shared/Interface/UpdateGxLeader.model";
import { AcademyResponse } from "@shared/dto/academy-response.dto";
import { AdminManageTraining } from "@shared/Interface/admin-manage-training";

@Injectable({
  providedIn: "root",
})
export class AcademyHttpService {
  private apiUrl: string;
  private mockApiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = environment.apiBaseUrl + "/" + environment.apiExtension;
    this.mockApiUrl = environment.apiMockinterviewBaseURL;
  }

  authenticateUser(token: string): Observable<any> {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_ACCOUNT}/${ApiConstants.PATH_AUTHENTICATE}`;
    return this.http.get(url);
  }

  fetchTrackerList(request: DataRequestOptions) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_TRACKERLIST}`;
    return this.http.post(url, request);
  }

  fetchEmployeeDashboard(employeeId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${employeeId}`;
    return this.http.get(url);
  }

  changeTrainingStatus(request: ChangeStatusRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_CHANGE_STATUS}`;
    return this.http.post(url, request);
  }

  saveComment(request: CommentRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_POST_COMMENTS}`;
    return this.http.post(url, request);
  }

  fetchLatestComment(employeeId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_FETCH_LATEST_COMMENT}/${employeeId}`;
    return this.http.get(url);
  }

  fetchProficiencies(employeeId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_FETCH_PROFICIENCY}/${employeeId}`;
    return this.http.get(url);
  }

  updateProficiency(request: ChangeProficiencyRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_UPDATE_PROFICIENCY}`;
    return this.http.post(url, request);
  }

  updateTrainingPriority(request: AdminManageTraining) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_Training}/${ApiConstants.PATH_UPDATE_TRAINING_PRIORITY}`;
    return this.http.post(url, request);
  }

  spinTrainings(request: SpinTrainingRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_SPIN_TRAININGS}`;
    return this.http.post(url, request);
  }

  fetchProficiencyByEcosystemSkill(ecosystemId: number, skillId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_FETCH_PROFICIENCY_BY_ECOSYSTEM_SKILL}/${ecosystemId}/${skillId}`;
    return this.http.get(url);
  }

  fetchAllComments(employeeId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_FETCH_COMMENTS}/${employeeId}`;
    return this.http.get(url);
  }

  fetchRequestTracker(transactionId: string) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.FETCH_REQUEST_TRACKER}/${transactionId}`;
    return this.http.get(url);
  }

  fetchPrimaryEcosystemsForMenu() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_LOAD_ALL_ECOSYSTEMS}/formenu`;
    return this.http.get(url);
  }

  fetchPrimaryEcosystems() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_LOAD_ALL_ECOSYSTEMS}`;
    return this.http.get(url);
  }

  fetchSecondaryEcosystems(includePrimary: boolean) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_LOAD_SECONDARY_ECOSYSTEMS}`;
    return this.http.get(url);
  }

  loadEmployee(startsWith: string, ecosystemId: number, account?: string) {
    const request = {
      startswith: startsWith,
      ecosystemId: ecosystemId,
      account: account,
    };
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_LOAD_EMPLOYEES_STARTS_WITH}`;
    return this.http.post(url, request);
  }

  insertSecondaryEcosystem(request: CreateEcosystemRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_INSERT_SECONDARY_ECOSYSTEM}`;
    return this.http.post(url, request);
  }

  addBookmark(request: BookmarkForms) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_INSERT_BOOKMARK}`;
    return this.http.post(url, request);
  }

  fetchProficiencyMaster() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_FETCH_PROFICIENCY_MASTER}`;
    return this.http.get(url);
  }

  fetchSeniorities() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_FETCH_SENIORITY}`;
    return this.http.get(url);
  }

  fetchSeniority() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_FETCH_SENIORITY}`;
    return this.http.get(url);
  }

  deactivateSeniority(id: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_DEACTIVATE_SENIORITY}/${id}`;
    return this.http.delete(url);
  }

  insertSeniority(request: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_INSERT_SENIORITY}`;
    return this.http.post(url, request);
  }

  searchEmployee(keywords: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_SEARCH_USER}`;
    return this.http.post(url, { searchkeywords: keywords });
  }

  fetchAllTdc() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_TDCS}`;
    return this.http.get(url);
  }  

  fetchAllTdcCommunityForDojo() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_TDCS_COMMUNITY_DOJO}`;
    return this.http.get(url);
  }

  fetchAllAccount() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_ACCOUNTS}`;
    return this.http.get(url);
  } 
  
  fetchAllCommunity() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_COMMUNITIES}`;
    return this.http.get(url);
  }

  fetchAllClients() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_CLIENTS}`;
    return this.http.get(url);
  }

  fetchAllAiStudio() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_AISTUDIOS}`;
    return this.http.get(url);
    }

  fetchAllAiStudioAndAccount() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_AISTUDIO_ACCOUNTS}`;
    return this.http.get(url);
    }

  updateEmployeeRole(value: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.PATH_INSERT_OR_UPDATE_EMPLOYEE_ROLE}`;
    return this.http.post(url, value);
  }

  loadSkillTrainingMetadataByEcosystem(ecosystemId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_LOAD_SLILLTRAININGS_METADATA}/${ecosystemId}`;
    return this.http.get(url);
  }

  loadSkillEndorsement(
    ecosystemId: number,
    account: string,
    employeeIds: string
  ) {
    const acc = account === "" ? undefined : account;
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_LOAD_SKILL_ENDORSEMENT}/${ecosystemId}/${acc}/${employeeIds}`;
    return this.http.get(url);
  }

  insertSkill(request: CreateSkillRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_INSERT_SKILL}`;
    return this.http.post(url, request);
  }

  fetchSkills() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_LOAD_SKILLS}`;
    return this.http.get(url);
  }

  insertTraining(request: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_INSERT_TRAINING}`;
    return this.http.post(url, request);
  }

  requestReport(request: ExportReportMetadata) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_REPORT}/${ApiConstants.PATH_REPORT_EXPORT}`;
    return this.http.post(url, request);
  }

  requestDetailedReport(request: ExportDetailReportMetadata) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_REPORT}/${ApiConstants.PATH_DETAILED_REPORT_EXPORT}`;
    return this.http.post(url, request);
  }
  
  fetchCategories() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_FETCH_ALL_CATEGORIES}`;
    return this.http.get(url);
  }

  insertCategory(value: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_SKILLTRAINING}/${ApiConstants.PATH_INSERT_CATEGORY_OR_SUBCATEGORY}`;
    return this.http.post(url, value);
  }

  gexLeaderStartsWith(startsWith: string) {
    const request = {
      startswith: startsWith,
    };
    const url = `${this.apiUrl}/Master/Employees/GexLeaderStartsWith`;
    return this.http.post(url, request);
  }

  updateDejoGexLeader(request: UpdateDojoGexLeaderRequest) {
    const url = `${this.apiUrl}/Dashboard/UpdateDojoGxLeader`;
    return this.http.post(url, request);
  }

  GetAllGXLeader(communityName: string) {
    const url = `${this.apiUrl}/GXLeader/GetAllGXLeader`
    let params = new HttpParams().set("community", communityName)
    return this.http.get(url, { params: params });
  }

  getActivities() {
    const url = `${this.apiUrl}/Master/Activities/FetchAll`;
    return this.http.get(url);
  }

  private attachBearerToken() {
    if (typeof localStorage !== "undefined") {
      // Check if localStorage is available
      const token = localStorage.getItem("authToken");
      const headers = {
        headers: new HttpHeaders({ authorization: "Bearer " + token }),
      };
      return headers;
    } else {
      // Handle case when localStorage is unavailable
      console.warn(
        "localStorage is not available. Running in non-browser environment?"
      );
      return { headers: new HttpHeaders() }; // Return empty headers or some default
    }
  }

  //Get interview answer and other details

  getInterviewDetail(request: string): Observable<MockInterviewDetail> {
    const url =
      this.mockApiUrl +
      "/" +
      ApiConstants.PATH_FETCH_INTERVIEW_ANSWER_AND_DETAILS +
      "/" +
      request;
    const headers = this.attachBearerToken();
    return this.http.get<MockInterviewDetail>(url, headers);
  }

  fetchAllActivities(id: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_ACTIVITY}/${id}`;
    return this.http.get(url);
  }
  saveActivityDetails(activity: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_ACTIVITY}/${ApiConstants.PATH_INSERT_UPDATE_ACTIVITY}`;
    return this.http.post(url, activity);
  }

  getActivityMasterList() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_ACTIVITIES}`;
    return this.http.get(url);
  }

  bulkActivities(activities: any) {
    const url = `${this.apiUrl}/Activity/BulkActivities`;
    return this.http.post(url, activities);
  }

  fetchDojoMemberList(request: any) {
    const url = `${this.apiUrl}/Dojo`;
    return this.http.post(url, request);
  }

  fetchDojoActivityReportList(request: any) {
    const url = `${this.apiUrl}/Report/GetDojoActivityReport`;
    return this.http.post(url, request);
  }

  fetchAssignedThroughTrainingReportList(request: any) {
    const url = `${this.apiUrl}/Report/AssignedThroughTraining`;
    return this.http.post(url, request);
  }

  exportAssignedThroughTrainingReport(request: any) {
    const url = `${this.apiUrl}/Report/ExportAssignThroughTrainingReport`;
    return this.http.post(url, request);
  }

  exportReport(request: any) {
    const url = `${this.apiUrl}/Report/ExportDojoActivityReport`;
    return this.http.post(url, request);
  }

  updateDojoEndDate(request: any[]) {
    const url = `${this.apiUrl}/Dojo/UpdateGlobarDojoEndDates`;
    return this.http.post(url, request);
  }

  updateDojoTrainingIfno(request: any[]) {
    const url = `${this.apiUrl}/Dojo/UpdateDojoGlobarTrainingInfo`;
    return this.http.post(url, request);
  }

  fetchAllTrainings(Communities: string[] = [], Areapaths: string[] = []) {
    let params = new HttpParams();
    Communities.forEach(c => params = params.append('Communities', c));
    Areapaths.forEach(a => params = params.append('Areapaths', a));
    console.log(params);
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_TRAINING}`;
    return this.http.get(url, { params });
  }

  fetchPagedTraining(filterByTrainingName: string, pageIndex: number, pageSize: number) {
    let params = new HttpParams();
    params = params.append('FilterByName', filterByTrainingName);
    params = params.append('PageIndex', pageIndex);
    params = params.append('PageSize', pageSize);
    console.log(params);
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_TRAINING}`;
    return this.http.get<AcademyResponse>(url, { params });
  }

  fetchAllProjects(clients: string[] = []) {
    let params = new HttpParams();
    clients?.forEach(c => params = params.append('Client', c));
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_PROJECT}`;
    return this.http.get(url, { params });
  }

  fetchAllTrainingStatus() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_TRAINING_STATUS}`;
    return this.http.get(url);
  }


  fetchAllReportTypes() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_REPORT_TYPES}`;
    return this.http.get(url);
  }

  fetchAllSelectColumns(activitytype: string) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_SELECT_COLUMNS}/${activitytype}`;
    return this.http.get(url);
  }

  fetchAllGroupByColumns(activitytype: string) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_GROUP_BY_COLUMNS}/${activitytype}`;
    return this.http.get(url);
  }
  fetchAllAreaPaths() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_AREAPATHS}`;
    return this.http.get(url);
  }

  deleteBookmark(bookMarkId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_DELETE_BOOKMARK}/${bookMarkId}`;
    return this.http.delete(url);
  }

  fetchBookmarkList() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_BOOKMARK_LIST}`;
    return this.http.get(url);
  }
  fetchBookmarkById(bookMarkId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_FETCH_BOOKMARK_BYID}/${bookMarkId}`;
    return this.http.get(url);
  }

  getReportData(request: BookmarkForms) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_BOOKMARK_VIEW_REPORT}`;
    return this.http.post(url, request);
  }

  exportReportData(request: BookmarkForms) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_BOOKMARK_EXPORT_REPORT}`;
    return this.http.post(url, request);
  }

  previewReportData(bookmarkId: number) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_BOOKMARK_PREVIEW_REPORT}/${bookmarkId}`;
    return this.http.post(url, null);
  }
  sendReportOnEmail(reportEmailRequest: ReportEmailRequest) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_TrainingReport}/${ApiConstants.PATH_BOOKMARK_SEND_REPORT}`;
    return this.http.post(url, reportEmailRequest);
  }
  fetchTrainingsByCommunity(Communities: string[]) {
    let params = new HttpParams();
    Communities.forEach(c => params = params.append('Communities', c));
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_TRAINING_BY_COMMUNITY}`;
    return this.http.get(url, { params });
  }
  fetchPrimaryActivityByCommunity(Communities: string[] = []) {
    let params = new HttpParams();
    if (Communities.length) {
      params = new HttpParams({ fromObject: { Communities } });
    }
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_PRIMARY_ACTIVITES_BY_COMMUNITY}`;
    return this.http.get(url, { params });
  }
  fetchAllActivityType() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_ALL_PRIMARYACTIVITY}`;
    return this.http.get(url);
  }

  fetchTrainingByAreapathAndCommunity(Communities: string[], Areapaths: string[]) {
    let params = new HttpParams({ fromObject: { Communities } });
    Areapaths.forEach(a => { params = params.append('Areapaths', a.toString()); });
    const url = `${this.apiUrl}/${ApiConstants.CTRL_MASTER}/${ApiConstants.FETCH_TRAING_BY_AREAPATH_AND_COMMUNITY}`;
    return this.http.get(url, { params });
  }

  UpdateGXLeader(request: UpdateGXLeader) {
    const url = `${this.apiUrl}/Dojo/UpdateGXLeader`;
    return this.http.post(url, request);
  }

  UpdateMentees(request: UpdateMentees) {
    const url = `${this.apiUrl}/Dojo/UpdateMentees`;
    return this.http.post(url, request);
  }

  DeleteGXLeader(request: UpdateGXLeader) {
    const url = `${this.apiUrl}/GXLeader/DeleteGXLeader`;
    return this.http.post(url, request);
  }

  uploadEmployeeCV(file: File, employeeId: number, community: string, docTypeId: number, existingDocLink: string) {
    const formData: FormData = new FormData();
    formData.append('file', file)
    formData.append('employeeId', employeeId.toString());
    formData.append('community', community);
    formData.append('docType', docTypeId.toString());
    formData.append('existingWebContentLink', existingDocLink);

    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.PATH_UPLOADCV}`;
    return this.http.post(url, formData);
  }

  fetchJobs() {
    const url = `${this.apiUrl}/${ApiConstants.FETCH_JOBS}/${ApiConstants.PATH_FETCH_JOB_LIST}`;
    return this.http.get(url);
  }

  fetchAllAdocumentType() {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_DASHBOARD}/${ApiConstants.FETCH_ALL_DOCUMENTTYPE}`;
    return this.http.get(url);
  }

fetchDojoEmployeeBusinessOrientedActivityDetails(employeeIds: string[]) {
  const url = `${this.apiUrl}/${ApiConstants.CTRL_ACTIVITY}/${ApiConstants.FETCH_DOJO_ACTIVITY_DETAIL}`;
  const params = new HttpParams().set('employeeEmails', employeeIds.join(','));
  return this.http.get(url, { params });
}

   GetMenteesByEmail(GXLeaderEmail: string) {
    const url = `${this.apiUrl}/Dojo/GetMenteesByEmail?GXLeaderEmail=${GXLeaderEmail}`
    return this.http.get(url);
  }
}